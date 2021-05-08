using AutoMapper;
using Identity.API.Helper;
using Identity.Domain;
using Identity.Infra;
using Identity.Services;
using Identity.Services.Abstractions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Identity.API
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			var connectionString = Configuration.GetConnectionString("DbConnection");
			var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

			services.AddDbContext<Context>(
				options => options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(migrationAssembly))
			);

			services.AddIdentityCore<User>(options =>
				{
					//options.SignIn.RequireConfirmedEmail = true;
					options.Password.RequireDigit = false;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireLowercase = false;
					options.Password.RequireUppercase = false;
					options.Lockout.MaxFailedAccessAttempts = 5;
					options.Lockout.AllowedForNewUsers = true;
				})
				.AddRoles<Role>()
				.AddEntityFrameworkStores<Context>()
				.AddRoleValidator<RoleValidator<Role>>()
				.AddRoleManager<RoleManager<Role>>()
				.AddSignInManager<SignInManager<User>>()
				.AddDefaultTokenProviders();

			services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options =>
				{
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Configuration.GetSection("AuthSettings:Token").Value)),
						ValidateIssuer = false,
						ValidateAudience = false
					};
				});

			var mappingConfiguration = new MapperConfiguration(x =>
			{
				x.AddProfile(new AutoMapperProfile());
			});
			IMapper mapper = mappingConfiguration.CreateMapper();

			services.AddSingleton(mapper);
			services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

			services.AddCors();

			services.AddMvc(x =>
				{
					// ASP NET Core auth: this is basically saying that every single method in each controller will require an authorization
					// to do something (then we don't have to put [Authorize] above each method because we're already configuring this here)
					var policy = new AuthorizationPolicyBuilder()
						.RequireAuthenticatedUser()
						.Build();
					x.Filters.Add(new AuthorizeFilter(policy));
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			app.UseAuthentication();
			app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
			app.UseMvc();
		}
	}
}
