using Identity.Domain;
using Identity.Services.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Services
{
	public class JwtTokenGenerator : IJwtTokenGenerator
	{
		IConfiguration Configuration { get; }
		UserManager<User> UserManager { get; }

		public JwtTokenGenerator(IConfiguration configuration, UserManager<User> userManager)
		{
			Configuration = configuration;
			UserManager = userManager;
		}

		public async Task<string> GenerateJwtToken(User user)
		{
			var claims = CreateClaims(user);
			await GetUserRoles(user, claims.ToList());
			var key = CreateEncryptionKey();
			var credential = GetCredentials(key);
			var tokenDescription = GetSecurityTokenDescriptor(claims, credential);
			var token = GenerateToken(tokenDescription);
			return token;
		}

		private IEnumerable<Claim> CreateClaims(User user)
			=> new List<Claim>
				{
					new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
					new Claim(ClaimTypes.Name, user.UserName),
				};

		private async Task GetUserRoles(User user, IList<Claim> claims)
		{
			var roles = await UserManager.GetRolesAsync(user);
			foreach (var role in roles)
				claims.Add(new Claim(ClaimTypes.Role, role));
		}

		private SymmetricSecurityKey CreateEncryptionKey()
			=> new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AuthSettings:Token").Value));

		private SigningCredentials GetCredentials(SymmetricSecurityKey key)
			=> new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

		private SecurityTokenDescriptor GetSecurityTokenDescriptor(IEnumerable<Claim> claims, SigningCredentials credential)
			=> new SecurityTokenDescriptor
				{
					Subject = new ClaimsIdentity(claims),
					Expires = DateTime.Now.AddDays(1),
					SigningCredentials = credential
				};

		private string GenerateToken(SecurityTokenDescriptor tokenDescription)
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var token = tokenHandler.CreateToken(tokenDescription);
			return tokenHandler.WriteToken(token);
		}

	}
}
