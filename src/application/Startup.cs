using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;

namespace application
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
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                .AddAzureADBearer(options =>
                {
                    Configuration.GetSection("AzureADB2C").Bind(options);
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("admin", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireAssertion(ctx =>
                    {
                        var name = ctx.User.FindFirst("name")?.Value;

                        return name != null && name.EndsWith("-admin", StringComparison.Ordinal);
                    });
                });

                options.AddPolicy("user", policy =>
                {
                    policy.RequireAuthenticatedUser();
                });
            });

            services.AddControllers();

           // services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            IdentityModelEventSource.ShowPII = true;
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.Use((_, next) => next());
            app.UseEndpoints(e => { e.MapControllers(); });
        }
    }
}
