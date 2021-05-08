using AutoMapper;
using Identity.API.DTO;
using Identity.Domain;
using Identity.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.API.Controllers
{
	// TODO: Create service layer and remove logic from the methods
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		IConfiguration Configuration { get; }
		UserManager<User> UserManager { get; }
		SignInManager<User> SignInManager { get; }
		IMapper Mapper { get; }
		IJwtTokenGenerator JwtTokenGenerator { get; }

		public UserController(IConfiguration configuration, UserManager<User> userManager,
			SignInManager<User> signInManager,
			IMapper mapper,
			IJwtTokenGenerator jwtTokenGenerator)
		{
			Configuration = configuration;
			UserManager = userManager;
			SignInManager = signInManager;
			Mapper = mapper;
			JwtTokenGenerator = jwtTokenGenerator;
		}

		[HttpPost("Login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login(UserLoginDTO userLoginDTO)
		{
			try
			{
				var user = await UserManager.FindByNameAsync(userLoginDTO.UserName);
				var result = await SignInManager.CheckPasswordSignInAsync(user, userLoginDTO.Password, false);
				if (result.Succeeded)
				{
					var appUser = UserManager.Users.Where(x => x.NormalizedUserName == user.UserName.ToUpper()).FirstOrDefault();
					if (appUser != null)
					{
						var userToReturn = Mapper.Map<UserDTO>(appUser);
						return Ok(new
						{
							token = JwtTokenGenerator.GenerateJwtToken(appUser).Result,
							user = userToReturn
						});
					}
				}
				return Unauthorized();
			}
			catch (Exception exception)
			{
				return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error: {exception.Message}");
			}
		}

		[HttpPost("Register")]
		[AllowAnonymous]
		public async Task<IActionResult> Register(UserDTO userDTO)
		{
			try
			{
				var user = await UserManager.FindByNameAsync(userDTO.UserName);
				if (user == null)
				{
					user = new User
					{
						UserName = userDTO.UserName,
						Email = userDTO.UserName,
						FullName = userDTO.FullName
					};
					var result = await UserManager.CreateAsync(user, userDTO.Password);
					if (result.Succeeded)
					{
						var appUser = UserManager.Users.Where(x => x.NormalizedUserName == user.UserName.ToUpper()).FirstOrDefault();
						if (appUser != null)
						{
							var token = JwtTokenGenerator.GenerateJwtToken(appUser).Result;
							//var confirmationEmail = Url.Action("ConfirmEmailAddress", "Home", new { token = token, email = user.Email }, Request.Scheme);
							// TODO: read from appsettings
							//System.IO.File.WriteAllText(@"C:\Users\Lara\Documents\confirmationEmail.txt", confirmationEmail);
							return Ok(token);
						}
					}
				}
				return Unauthorized();
			}
			catch (Exception exception)
			{ 
				return this.StatusCode(StatusCodes.Status500InternalServerError, $"Error: {exception.Message}");
			}
		}

		[HttpGet]
		public IActionResult Get() => Ok(new UserDTO());

	}
}
