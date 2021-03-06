﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions;
using Newtonsoft.Json;
using TheEventCenter.Api.Db;
using TheEventCenter.Api.Db.Models;
using TheEventCenter.Api.Helpers;
using TheEventCenter.Api.Models;
using TheEventCenter.Api.ViewModels;

namespace TheEventCenter.Api.Controllers
{
	[Route("api/[controller]")]
	public class AuthController : Controller
	{
		private readonly IMapper _mapper;
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;
		private readonly ApplicationIdentityDbContext _dbContext;
		private readonly IJwtFactory _jwtFactory;
		private readonly JsonSerializerSettings _serializerSettings;
		private readonly JwtIssuerOptions _jwtOptions;

		public AuthController(
			IMapper mapper,
			UserManager<AppUser> userManager,
			SignInManager<AppUser> signInManager,
			ApplicationIdentityDbContext dbContext,
			IJwtFactory jwtFactory,
			IOptions<JwtIssuerOptions> jwtOptions
		)
		{
			_mapper = mapper;
			_userManager = userManager;
			_signInManager = signInManager;
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
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			if (string.IsNullOrEmpty(request.UserName) || string.IsNullOrEmpty(request.Password))
			{
				return BadRequest(new AuthResponse() { Response = "Username and password required." });
			}
			var result = await _signInManager.PasswordSignInAsync(request.UserName, request.Password, false, false);

			if (!result.Succeeded)
			{
				return BadRequest(new AuthResponse() { Response = "Username or password invalid."});
			}

			var appUser = await _userManager.FindByEmailAsync(request.UserName);
			var claimsIdentity = _jwtFactory.GenerateClaimsIdentity(request.UserName, appUser.Id);

			var response = new
			{
				id = appUser.Id,
				auth_token = await _jwtFactory.GenerateEncodedToken(request.UserName, claimsIdentity),
				expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
			};

			await _userManager.SetAuthenticationTokenAsync(appUser, "TheEventCenter", "Auth_Token", response.auth_token);
			var json = JsonConvert.SerializeObject(response, _serializerSettings);
			return new OkObjectResult(json);
		}

		[HttpPost]
		[Route("register")]
		[AllowAnonymous]
		public async Task<IActionResult> Register([FromBody] RegistrationViewModel model)
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

			var appUser = await _userManager.FindByEmailAsync(userIdentity.Email);
			var claimsIdentity = _jwtFactory.GenerateClaimsIdentity(userIdentity.Email, appUser.Id);

			var response = new
			{
				id = appUser.Id,
				auth_token = await _jwtFactory.GenerateEncodedToken(userIdentity.Email, claimsIdentity),
				expires_in = (int)_jwtOptions.ValidFor.TotalSeconds
			};

			await _userManager.SetAuthenticationTokenAsync(appUser, "TheEventCenter", "Auth_Token", response.auth_token);
			var json = JsonConvert.SerializeObject(response, _serializerSettings);

			return new OkObjectResult(json);
		}

		[HttpGet]
		[Route("logout")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var user = await _userManager.FindByEmailAsync(username);
			await _userManager.RemoveAuthenticationTokenAsync(user, "TheEventCenter", "Auth_Token");
			await HttpContext.SignOutAsync();
			return new JsonResult(true);
		}
	}

	public class AuthResponse
	{
		public string Response { get; set; }
	}

	public class LoginRequest
	{
		public string UserName { get; set; }
		public string Password { get; set; }
	}
}
