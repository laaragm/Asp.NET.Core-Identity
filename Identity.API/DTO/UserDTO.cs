using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.API.DTO
{
	public class UserDTO
	{
		public string UserName { get; set; }
		public string FullName { get; set; }

		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Compare("Password")]
		[DataType(DataType.Password)]
		public string PasswordConfirmation { get; set; }
	}
}
