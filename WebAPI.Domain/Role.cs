using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Identity.Domain
{
	public class Role : IdentityRole<int>
	{
		public IEnumerable<UserRole> UserRoles { get; set; }
	}
}
