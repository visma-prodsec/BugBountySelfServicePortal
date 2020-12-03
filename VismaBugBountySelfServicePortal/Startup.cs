using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using VismaBugBountySelfServicePortal.Base;

namespace VismaBugBountySelfServicePortal
{
    public class Startup
    {
        private IConfiguration Configuration { get; }
        private readonly AppBuilder _apiBuilder;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            ILogger logger = loggerFactory.CreateLogger<Startup>();
            logger.LogInformation("Application Started");
            _apiBuilder = new AppBuilder(configuration)
                .AddSpecifications<APIProjectSpecifications>();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });
            services.AddControllersWithViews();
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;

            }).AddCookie(options =>
                {
                    options.AccessDeniedPath = "/Home/Unauthorized";
                })
              .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
              {
                  options.Authority = Configuration["VismaConnectUrl"];
                  options.ClientId = Configuration["ClientId"];
                  options.ClientSecret = Configuration["ClientSecret"];
                  options.ResponseType = "code";
                  options.Scope.Add("email");
                  options.GetClaimsFromUserInfoEndpoint = true;
                  options.SaveTokens = true;
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      NameClaimType = "name"
                  };
              });
            
            services.AddApplicationInsightsTelemetry();
            services.AddMvc(options => options.Filters.Add(new AuthorizeFilter())).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            _apiBuilder.BuildServices(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                app.UseExceptionHandler("/Home/Error");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            //app.UseMiddleware(typeof(ExceptionHandlerMiddleware));
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/home/error/{0}");
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseCookiePolicy();
            app.UseXContentTypeOptions();
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(options => options.SameOrigin());
            _apiBuilder.BuildApp(app, serviceProvider);
        }
    }
}
