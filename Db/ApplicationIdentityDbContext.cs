using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheEventCenter.Api.Db.Models;

namespace TheEventCenter.Api.Db
{
    public class ApplicationIdentityDbContext : IdentityDbContext
    {
		public ApplicationIdentityDbContext(DbContextOptions options) : base(options)
		{
		}

		public DbSet<User> AppUsers { get; set; }
	}
}