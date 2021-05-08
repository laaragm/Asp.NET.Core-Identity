using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Identity.Domain
{
	public class User : IdentityUser<int>
	{
		public string FullName { get; set; }
		public string Member { get; set; } = "Member";
		public string Organization { get; set; }
		public IEnumerable<UserRole> UserRoles { get; set; }
	}
}
