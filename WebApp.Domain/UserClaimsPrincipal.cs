using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.IdentityDomain
{
	public class UserClaimsPrincipal : UserClaimsPrincipalFactory<User>
	{
		public UserClaimsPrincipal(UserManager<User> userManager, IOptions<IdentityOptions> optionsAccessor) : base(userManager, optionsAccessor)
		{

		}

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
		{
			var identity = await base.GenerateClaimsAsync(user);
			identity.AddClaim(new Claim("Member", user.Member));
			return identity;
		}
	}
}
