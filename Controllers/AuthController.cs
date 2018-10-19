using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using TheEventCenter.Api.Db;
using TheEventCenter.Api.Db.Models;
using TheEventCenter.Api.Helpers;
using TheEventCenter.Api.Models;
using TheEventCenter.Api.ViewModels;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TheEventCenter.Api.Controllers
{
	[Route("api/[controller]")]
	[AllowAnonymous]
	public class AuthController : Controller
	{
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		private readonly ApplicationDbContext _dbContext;
		private readonly IJwtFactory _jwtFactory;
		private readonly JsonSerializerSettings _serializerSettings;
		private readonly JwtIssuerOptions _jwtOptions;

		public AuthController(
			IMapper mapper,
			UserManager<AppUser> userManager,
			ApplicationDbContext dbContext,
			IJwtFactory jwtFactory,
			IOptions<JwtIssuerOptions> jwtOptions
		)
		{
			_mapper = mapper;
			_userManager = userManager;
			_dbContext = dbContext;
			_jwtFactory = jwtFactory;
			_serializerSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				ReferenceLoopHandling = ReferenceLoopHandling.Ignore
			};
			_jwtOptions = jwtOptions.Value;
		}

		[HttpPost("[action]")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var identity = await GetClaimsIdentity(request.UserName, request.Password);
			//return new OkObjectResult(JsonConvert.SerializeObject(identity, _serializerSettings));
			if (identity == null)
			{
				return BadRequest(Errors.AddErrorToModelState("login_failure", "Invalid username or password.", ModelState));
			}

			// Serialize and return the response
			var response = new
			{
				id = identity.Claims.Single(c => c.Type == "id").Value,
				auth_token = await _jwtFactory.GenerateEncodedToken(request.UserName, identity),
				expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
			};

			var json = JsonConvert.SerializeObject(response, _serializerSettings);
			return new OkObjectResult(json);
		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Post([FromBody] RegistrationViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			var userIdentity = _mapper.Map<AppUser>(model);
			var result = await _userManager.CreateAsync(userIdentity, model.Password);

			if (!result.Succeeded)
			{
				return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));
			}

			await _dbContext.AppUsers.AddAsync(new User { IdentityId = userIdentity.Id, Location = model.PhoneNumber });
			await _dbContext.SaveChangesAsync();

			return new OkObjectResult("Account created");
		}

		[HttpPost("user-exists")]
		public bool UserExists([FromBody]string email)
		{
			return true;
		}

		private async Task<ClaimsIdentity> GetClaimsIdentity(string userName, string password)
		{
			if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(password))
			{
				// get the user to verifty
				var userToVerify = await _userManager.FindByEmailAsync(userName);

				if (userToVerify != null)
				{
					// check the credentials  
					if (await _userManager.CheckPasswordAsync(userToVerify, password))
					{
						return await Task.FromResult(_jwtFactory.GenerateClaimsIdentity(userName, userToVerify.Id));
					}
				}
			}

			// Credentials are invalid, or account doesn't exist
			return await Task.FromResult<ClaimsIdentity>(null);
		}
	}

	public class LoginRequest
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}
