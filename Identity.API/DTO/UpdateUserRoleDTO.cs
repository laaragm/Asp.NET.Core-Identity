using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.API.DTO
{
	public class UpdateUserRoleDTO
	{
		public string Email { get; set; }
		public string Role { get; set; }
		public bool Delete { get; set; }
	}
}
