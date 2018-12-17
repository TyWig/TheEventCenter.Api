using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheEventCenter.Api.ViewModels
{
	public class UserProfile
	{
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public bool IsAdmin { get; set; }
		public string Email { get; set; }
	}
}
