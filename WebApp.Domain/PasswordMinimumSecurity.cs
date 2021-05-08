using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.IdentityDomain
{
	public class PasswordMinimumSecurity<TUser> : IPasswordValidator<TUser> where TUser : class
	{
		public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
		{
			var username = await manager.GetUserNameAsync(user);
			if (username == password)
				return IdentityResult.Failed(new IdentityError { Description = "User and password cannot be equal" });
			if (password.Contains("password") || password.Contains("senha"))
				return IdentityResult.Failed(new IdentityError { Description = "Password cannot contain word password" });

			return IdentityResult.Success;
		}
	}
}
