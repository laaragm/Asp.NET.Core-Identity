using Identity.API.DTO;
using Identity.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.API.Controllers
{
	// TODO: Create service layer and remove logic from the methods
	[Route("api/[controller]")]
	[ApiController]
	public class RoleController : ControllerBase
	{
		RoleManager<Role> RoleManager { get; }
		UserManager<User> UserManager { get; }

		public RoleController(RoleManager<Role> roleManager, UserManager<User> userManager)
		{
			RoleManager = roleManager;
			UserManager = userManager;
		}

		[HttpGet]
		[Authorize(Roles = "Admin")]
		public IActionResult Get()
		{
			return Ok(new
			{
				role = new RoleDTO(),
				updateUserRole = new UpdateUserRoleDTO()
			});
		}

		[HttpGet("{id}", Name = "Get")]
		[Authorize(Roles = "Admin, Manager")]
		public string Get(int id)
		{
			return "value";
		}

		[HttpPost("CreateRole")]
		public async Task<IActionResult> CreateRole(RoleDTO roleDTO)
		{
			try
			{
				var result = await RoleManager.CreateAsync(new Role { Name = roleDTO.Name });
				return Ok(result);
			}
			catch (Exception exception)
			{
				return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error: {exception.Message}");
			}
		}

		[HttpPut("UpdateUserRole")]
		public async Task<IActionResult> UpdateUserRole(UpdateUserRoleDTO model)
		{
			try
			{
				var user = await UserManager.FindByEmailAsync(model.Email);
				if (user != null)
				{
					if (model.Delete)
						await UserManager.RemoveFromRoleAsync(user, model.Role);
					else
						await UserManager.AddToRoleAsync(user, model.Role);
				}
				return Ok();
			}
			catch (Exception exception)
			{
				return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error: {exception.Message}");
			}
		}

	}
}
