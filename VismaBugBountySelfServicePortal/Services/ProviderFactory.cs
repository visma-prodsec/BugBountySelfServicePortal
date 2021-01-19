using System;
using Microsoft.Extensions.Configuration;

namespace VismaBugBountySelfServicePortal.Services
{
    public class ProviderFactory : IProviderFactory
    {

        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ProviderFactory(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public IProviderService GetProviderService(string userDomain)
        {
            if (userDomain == _configuration["HackerOneHackerEmailDomain"])
                return (IProviderService)_serviceProvider.GetService(typeof(HackerOneService)) ?? throw new ArgumentOutOfRangeException(userDomain);
            if (userDomain == _configuration["IntigritiHackerEmailDomain"])
                return (IProviderService)_serviceProvider.GetService(typeof(IntigritiService)) ?? throw new ArgumentOutOfRangeException(userDomain);
            throw new ArgumentOutOfRangeException(userDomain);
        }
    }
}
