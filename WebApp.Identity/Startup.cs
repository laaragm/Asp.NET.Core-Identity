using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WebApp.IdentityDomain;
using WebApp.IdentityInfra;

namespace WebApp.Identity
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
			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});


			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			// reflection is used to find a specific type inside a dll
			var migrationAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

			// TODO: read from appsettings
			var connectionString = @"Data Source=DESKTOP-1QV1RI7\SQLEXPRESS;Database=WebApp;Integrated Security = True;MultipleActiveResultSets=true;Connect Timeout=1000;";
			services.AddDbContext<CustomIdentityDbContext>(
				options => options.UseSqlServer(connectionString, 
				sql => sql.MigrationsAssembly(migrationAssembly))
			);

			services.AddIdentity<User, IdentityRole>(options => 
			{
				options.SignIn.RequireConfirmedEmail = true;
				options.Password.RequireDigit = false;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireLowercase = false;
				options.Password.RequireUppercase = false;
				options.Lockout.MaxFailedAccessAttempts = 5;
				options.Lockout.AllowedForNewUsers = true;
			})
				.AddEntityFrameworkStores<CustomIdentityDbContext>()
				.AddDefaultTokenProviders()
				.AddPasswordValidator<PasswordMinimumSecurity<User>>();

			services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipal>();

			services.Configure<DataProtectionTokenProviderOptions>(
				options => options.TokenLifespan = TimeSpan.FromHours(2)
			);

			services.ConfigureApplicationCookie(options => options.LoginPath = "/Home/Login");
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseAuthentication();

			app.UseStaticFiles();
			app.UseCookiePolicy();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
