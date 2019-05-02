using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Project4.Models;
using Project4.Services;
using System.Net;
using System.Threading.Tasks;

namespace Project4
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
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

			services.AddScoped<ITodoService, TodoService>();

			var connection = Configuration["TodoConnectionString"];
			services.AddDbContext<TodoContext>
				(options => options.UseSqlServer(connection));

			// In production, the Angular files will be served from this directory
			services.AddSpaStaticFiles(configuration =>
			{
				configuration.RootPath = "ClientApp/dist";
			});

			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = "My API", Version = "v1" });
			});

			services.AddIdentity<User, IdentityRole>(config =>
			{
				config.Password.RequireDigit = true;
				config.Password.RequiredLength = 8;
				config.Password.RequireUppercase = true;
				config.Password.RequireLowercase = true;
				config.Password.RequireNonAlphanumeric = false;
			}).AddEntityFrameworkStores<TodoContext>();

			services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.HttpOnly = true;
				options.ExpireTimeSpan = System.TimeSpan.FromMinutes(5);

				options.LoginPath = "/user/create";
				options.AccessDeniedPath = "/user/create";
				options.SlidingExpiration = true;

				options.Events.OnRedirectToLogin = context =>
				{
					context.Response.StatusCode = 401;
					return Task.CompletedTask;
				};
			});

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
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseSpaStaticFiles();

			// Enable middleware to serve generated Swagger as a JSON endpoint.
			app.UseSwagger();

			// Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
			// specifying the Swagger JSON endpoint.
			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
			});

			app.UseAuthentication();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller}/{action=Index}/{id?}");
			});

			//app.UseStatusCodePages(async context =>
			//{
			//	var response = context.HttpContext.Response;
			//	if(response.StatusCode == (int)HttpStatusCode.Unauthorized ||
			//		response.StatusCode == (int)HttpStatusCode.Forbidden) 
			//	{
			//		response.Redirect("/user/create");
			//	}
			//});

			app.UseSpa(spa =>
			{
				// To learn more about options for serving an Angular SPA from ASP.NET Core,
				// see https://go.microsoft.com/fwlink/?linkid=864501

				spa.Options.SourcePath = "ClientApp";

				if (env.IsDevelopment())
				{
					spa.UseAngularCliServer(npmScript: "start");
				}
			});


		}
	}
}
