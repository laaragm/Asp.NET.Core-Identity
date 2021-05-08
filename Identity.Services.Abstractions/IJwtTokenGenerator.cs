using Identity.Domain;
using System;
using System.Threading.Tasks;

namespace Identity.Services.Abstractions
{
	public interface IJwtTokenGenerator
	{
		Task<string> GenerateJwtToken(User user);
	}
}
