using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VismaBugBountySelfServicePortal.Base
{
    public class AppBuilder
    {
        private readonly List<ProjectSpecifications> _specifications = new List<ProjectSpecifications>();
        private readonly IConfiguration _configuration;

        public AppBuilder(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AppBuilder BuildServices(IServiceCollection services)
        {
            foreach (var apiSpecifications in _specifications)
            {
                apiSpecifications.ConfigureServices(services);
            }

            return this;
        }

        private AppBuilder AddSpecifications(ProjectSpecifications specifications)
        {
            _specifications.Add(specifications);

            return this;
        }

        public AppBuilder AddSpecifications<T>() where T : ProjectSpecifications, new()
        {
            var appConf = new T();
            appConf.SetConfiguration(_configuration);
            return this.AddSpecifications(appConf);
        }

        public AppBuilder BuildApp(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            foreach (var apiSpecifications in _specifications)
            {
                apiSpecifications.ConfigureApp(app, serviceProvider);
            }

            return this;
        }
    }
}