using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Identity.Models;
using WebApp.IdentityDomain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WebApp.Identity.Controllers
{
	// TODO: Create service layer and remove logic from the methods
	public class HomeController : Controller
	{
		private readonly UserManager<User> UserManager;

		private readonly IUserClaimsPrincipalFactory<User> UserClaimsPrincipalFactory;

		public SignInManager<User> SignInManager { get; }

		public HomeController(UserManager<User> userManager,
			IUserClaimsPrincipalFactory<User> userClaimsPrincipalFactory,
			SignInManager<User> signInManager)
		{
			UserManager = userManager;
			UserClaimsPrincipalFactory = userClaimsPrincipalFactory;
			SignInManager = signInManager;
		}

		[HttpGet]
		public IActionResult Register()	=> View();

		[HttpPost]
		public async Task<IActionResult> Register(RegisterModel registerModel)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByNameAsync(registerModel.UserName);
				if (user == null)
				{
					user = new User
					{
						Id = Guid.NewGuid().ToString(),
						UserName = registerModel.UserName,
						Email = registerModel.Email
					};
					var result = await UserManager.CreateAsync(user, registerModel.Password);
					if (result.Succeeded)
					{
						var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
						var confirmationEmail = Url.Action("ConfirmEmailAddress", "Home", new { token = token, email = user.Email }, Request.Scheme);
						// TODO: read from appsettings
						System.IO.File.WriteAllText(@"C:\Users\Lara\Documents\confirmationEmail.txt", confirmationEmail);
					}
					else
					{
						foreach (var error in result.Errors)
							ModelState.AddModelError("", error.Description);
						return View();
					}
				}
				return View("Success");
			}
			return View();
		}

		[HttpGet]
		public async Task<IActionResult> ConfirmEmailAddress(string token, string email)
		{
			var user = await UserManager.FindByEmailAsync(email);
			if (user != null)
			{
				var result = await UserManager.ConfirmEmailAsync(user, token);
				if (result.Succeeded)
					return View("Success");
			}
			return View("Error");
		}

		[HttpPost]
		public async Task<IActionResult> Login(LoginModel loginModel)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByNameAsync(loginModel.UserName);
				if (user != null && !await UserManager.IsLockedOutAsync(user))
				{
					if (await UserManager.CheckPasswordAsync(user, loginModel.Password))
					{
						if (!await UserManager.IsEmailConfirmedAsync(user))
						{
							ModelState.AddModelError("", "Invalid email");
							return View();
						}
						await UserManager.ResetAccessFailedCountAsync(user);

						if (await UserManager.GetTwoFactorEnabledAsync(user))
						{
							var validator = await UserManager.GetValidTwoFactorProvidersAsync(user);
							if (validator.Contains("Email"))
							{
								var token = await UserManager.GenerateTwoFactorTokenAsync(user, "Email");
								// TODO: read from appsettings
								System.IO.File.WriteAllText(@"C:\Users\Lara\Documents\email2FA.txt", token);
								await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, Store2FA(user.Id, "Email"));
								return RedirectToAction("TwoFactor");
							}
						}

						var principal = await UserClaimsPrincipalFactory.CreateAsync(user);
						await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal);
						return RedirectToAction("About");

						//var signInResult = await SignInManager.PasswordSignInAsync(loginModel.UserName, loginModel.Password, false, false);
						//if (signInResult.Succeeded)
						//	return RedirectToAction("About");
					}
					await UserManager.AccessFailedAsync(user);
					if (await UserManager.IsLockedOutAsync(user))
					{
						// email must be sent informing the user that someone is trying to access his account
					}
				}
				ModelState.AddModelError("", "User or password incorrect");
			}
			return View();
		}

		public ClaimsPrincipal Store2FA(string userId, string provider)
		{
			// creating identity claims to add to principal
			var identity = new ClaimsIdentity(new List<Claim>
			{
				new Claim("sub", userId),
				new Claim("amr", provider)
			}, IdentityConstants.TwoFactorUserIdScheme);
			return new ClaimsPrincipal(identity);
		}

		[HttpGet]
		public IActionResult Login() => View();

		[HttpGet]
		[Authorize] // if it's not logged, you won't be able to go to this specific view
		public IActionResult About() => View();

		[HttpGet]
		public IActionResult Success() => View();

		public IActionResult Index() => View();

		public IActionResult Privacy() => View();

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		[HttpGet]
		public IActionResult ForgotPassword() => View();

		[HttpPost]
		public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByEmailAsync(forgotPasswordModel.Email);
				if (user != null)
				{
					var token = await UserManager.GeneratePasswordResetTokenAsync(user);
					var resetUrl = Url.Action("ResetPassword", "Home", new { token = token, email = forgotPasswordModel.Email }, Request.Scheme);

					// TODO: read from appsettings
					System.IO.File.WriteAllText(@"C:\Users\Lara\Documents\resetLink.txt", resetUrl);

					return View("Success");
				}
				else
					return RedirectToAction("UserNotFound");
			}
			return View();
		}

		[HttpGet]
		public IActionResult ResetPassword(string token, string email) 
			=> View(new ResetPasswordModel { Token = token, Email = email });

		[HttpPost]
		public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
		{
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByEmailAsync(resetPasswordModel.Email);
				if (user != null)
				{
					var result = await UserManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
					if (!result.Succeeded)
					{
						foreach (var error in result.Errors)
							ModelState.AddModelError("", error.Description);
						return View();
					}
					return View("Success");
				}
				ModelState.AddModelError("", "Invalid request");
			}
			return View();
		}

		[HttpGet]
		public IActionResult TwoFactor()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> TwoFactor(TwoFactorModel model)
		{
			// validate if token has expired
			var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
			if (!result.Succeeded)
			{
				ModelState.AddModelError("", "Token has expired");
				return View();
			}
			if (ModelState.IsValid)
			{
				var user = await UserManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));
				if (user != null)
				{
					// try to find the claims of the user
					var isValid = await UserManager.VerifyTwoFactorTokenAsync(user, result.Principal.FindFirstValue("amr"), model.Token);
					if (isValid)
					{
						await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
						var claimsPrincipal = await UserClaimsPrincipalFactory.CreateAsync(user);
						await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);
						return RedirectToAction("About");
					}
					ModelState.AddModelError("", "Invalid token");
					return View();
				}
				ModelState.AddModelError("", "Invalid request");
			}
			return View();
		}

	}
}
