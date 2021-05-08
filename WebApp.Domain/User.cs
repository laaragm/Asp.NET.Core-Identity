using Microsoft.AspNetCore.Identity;
using System;

namespace WebApp.IdentityDomain
{
	public class User : IdentityUser
	{
		public string FullName { get; set; }
		public string Member { get; set; } = "Member";
		public string Organization { get; set; }
	}
}
