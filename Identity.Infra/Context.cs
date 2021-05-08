using Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Identity.Infra
{
	public class Context : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole,
												IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
	{
		public Context(DbContextOptions<Context> options) : base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);

			builder.Entity<UserRole>(userRole =>
			{
				userRole.HasKey(x => new { x.UserId, x.RoleId }); // UserId and RoleId are from IdentityUserRole
				userRole.HasOne(x => x.Role)
						.WithMany(x => x.UserRoles)
						.HasForeignKey(x => x.RoleId)
						.IsRequired();
				userRole.HasOne(x => x.User)
						.WithMany(x => x.UserRoles)
						.HasForeignKey(x => x.UserId)
						.IsRequired();
			});

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
