using Microsoft.AspNetCore.Identity;

namespace TheEventCenter.Api.Db.Models
{
	public class AppUser : IdentityUser
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}
}
