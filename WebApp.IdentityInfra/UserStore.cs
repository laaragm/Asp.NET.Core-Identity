using Dapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using WebApp.IdentityDomain;

namespace WebApp.IdentityInfra
{
	public class UserStore : IUserStore<User>, IUserPasswordStore<User>
	{
		public UserStore()
		{

		}

		public static DbConnection GetConnection()
		{
			// TODO: read from appsettings
			var connection = new SqlConnection(@"Data Source=DESKTOP-1QV1RI7\SQLEXPRESS;Database=WebApp;Integrated Security = True;MultipleActiveResultSets=true;Connect Timeout=1000;");
			connection.Open();
			return connection;
		}

		public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
		{
			using (var connection = GetConnection())
			{
				await connection.ExecuteAsync(@"
					INSERT INTO [User]([ID], [UserName], [NormalizedUserName], [PasswordHash])
					VALUES (@id, @userName, @normalizedUserName, @passwordHash)",
					new
					{
						id = user.Id,
						userName = user.UserName,
						normalizedUserName = user.NormalizedUserName,
						passwordHash = user.PasswordHash
					});
			}
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
		{
			using (var connection = GetConnection())
			{
				await connection.ExecuteAsync("DELETE FROM [User] WHERE ID = @id",
					new
					{
						id = user.Id
					});
			}
			return IdentityResult.Success;
		}

		public void Dispose()
		{

		}

		public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			using (var connection = GetConnection())
			{
				return await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE ID = @id",
					new { id = userId });
			}
		}

		public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
		{
			using (var connection = GetConnection())
			{
				return await connection.QueryFirstOrDefaultAsync<User>("SELECT * FROM [User] WHERE NormalizedUserName = @normalizedUserName",
					new { normalizedUserName });
			}
		}

		public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
			=> Task.FromResult(user.NormalizedUserName);

		public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PasswordHash);
		}

		public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
			=> Task.FromResult(user.Id);

		public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
			=> Task.FromResult(user.UserName);

		public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
		{
			return Task.FromResult(user.PasswordHash != null);
		}

		public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
		{
			user.NormalizedUserName = normalizedName;
			return Task.CompletedTask;
		}

		public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
		{
			user.PasswordHash = passwordHash;
			return Task.CompletedTask;
		}

		public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
		{
			user.UserName = userName;
			return Task.CompletedTask;
		}

		public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
		{
			using (var connection = GetConnection())
			{
				await connection.ExecuteAsync(@"
					UPDATE [User]
					SET
						[ID] = @id,
						[UserName] = @userName,
						[NormalizedUserName] = @normalizedUserName,
						[PasswordHash] = @passwordHash
					WHERE ID = @id",
					new
					{
						id = user.Id,
						userName = user.UserName,
						normalizedUserName = user.NormalizedUserName,
						passwordHash = user.PasswordHash
					});
			}
			return IdentityResult.Success;
		}
	}
}
