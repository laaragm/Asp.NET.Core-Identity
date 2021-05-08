using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using WebApp.IdentityDomain;

namespace WebApp.IdentityInfra
{
	public class CustomIdentityDbContext : IdentityDbContext<User>
	{
		public CustomIdentityDbContext(DbContextOptions<CustomIdentityDbContext> options) : base(options)
		{

		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<Organization>(company =>
			{
				company.ToTable("Organizations");
				company.HasKey(x => x.Id);
				company.HasMany<User>()
					.WithOne()
					.HasForeignKey(x => x.Organization)
					.IsRequired(false);
			});
		}
	}
}
