﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Identity.Models
{
	public class RegisterModel
	{
		public string UserName { get; set; }
		public string Email { get; set; }

		[DataType(DataType.Password)]
		public string Password { get; set; }

		[Compare("Password")]
		[DataType(DataType.Password)]
		public string PasswordConfirmation { get; set; }
	}
}
