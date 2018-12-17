using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheEventCenter.Api.Db.Models;
using TheEventCenter.Api.ViewModels;

namespace TheEventCenter.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
	[Authorize]
    public class UserController : Controller
    {
		private readonly UserManager<AppUser> _userManager;

		public UserController(UserManager<AppUser> userManager)
		{
			_userManager = userManager;
		}

        [HttpGet]
		[Route("[action]")]
        public async Task<ActionResult<UserProfile>> Profile()
        {
			var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			var user = await _userManager.FindByEmailAsync(username);
			var profile = new UserProfile()
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				IsAdmin = true,
				Email = user.Email
			};
			return new ActionResult<UserProfile>(profile);
		}
    }
}
