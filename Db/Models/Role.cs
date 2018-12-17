using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TheEventCenter.Api.Db.Models
{
	public class Role
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public bool IsAdmin { get; set; }
	}
}
