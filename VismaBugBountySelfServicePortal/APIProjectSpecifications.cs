using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VismaBugBountySelfServicePortal.Base;
using VismaBugBountySelfServicePortal.Base.Database.DataLayer;
using VismaBugBountySelfServicePortal.Base.Database.Repository;
using VismaBugBountySelfServicePortal.Database;
using VismaBugBountySelfServicePortal.Helpers;
using VismaBugBountySelfServicePortal.Services;

namespace VismaBugBountySelfServicePortal
{
    public class APIProjectSpecifications : ProjectSpecifications
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            var emailConfig = Configuration
                .GetSection("EmailConfiguration")
                .Get<EmailConfiguration>();
            services.AddSingleton(emailConfig);

            services.AddDbContext<ApplicationDbContext>((serviceProvider, optionsBuilder) =>
            {
                var httpContext = serviceProvider.GetService<IHttpContextAccessor>()?.HttpContext;
                var httpRequest = httpContext?.Request;
                //TODO: think to a more clever way of doing this
                var needsAdmin = httpRequest != null && httpRequest.Path.HasValue &&
                                 (httpRequest.Method == "PUT" && httpRequest.Path.Value == "/api/migrate"
                                  || httpRequest.Method == "POST" && httpRequest.Path.Value == "/Asset/ImportCredentials"
                                  || httpRequest.Method == "POST" && httpRequest.Path.Value == "/Asset/DeleteAsset");
                var builder = new SqlConnectionStringBuilder(
                    Configuration.GetConnectionString("BugBountySelfServicePortalDatabase"))
                {
                    UserID = needsAdmin ? Configuration["AdminDatabaseUsername"] : Configuration["DatabaseUsername"],
                    Password = needsAdmin ? Configuration["AdminDatabasePassword"] : Configuration["DatabasePassword"],
                    ConnectTimeout = 5 * 60,
                    ConnectRetryCount = 3,
                    ConnectRetryInterval = 10
                };
                optionsBuilder.UseSqlServer(builder.ConnectionString);
                optionsBuilder.UseInternalServiceProvider(serviceProvider);
            });
            services.AddScoped<DbContext, ApplicationDbContext>();
            services.AddScoped<IDatabaseLayer, DatabaseLayer>();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IDataSeeder, DataSeeder<CommonDataSeedType>>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ICredentialService, CredentialService>();
            services.AddScoped<IAssetService, AssetService>();
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IHackerOneService, HackerOneService>();
            services.AddHttpContextAccessor();
            services.AddEntityFrameworkSqlServer();
        }
    }
}
