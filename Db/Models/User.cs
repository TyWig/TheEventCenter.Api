﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheEventCenter.Api.Db.Models
{
	public class User
	{
		public int Id { get; set; }
		public string IdentityId { get; set; }
		public AppUser Identity { get; set; }
		public string Location { get; set; }
		public virtual Role Role { get; set; }
	}
}
