using System;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VismaBugBountySelfServicePortal.Base
{
    public abstract class ProjectSpecifications
    {
        protected IConfiguration Configuration { get; set; }

        public virtual void Init()
        {
        }

        public void SetConfiguration(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        public virtual void ConfigureApp(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            ConfigureApp(app, serviceProvider.GetService<IWebHostEnvironment>());
        }

        protected virtual void ConfigureApp(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        public virtual IMvcBuilder MvcChain(IMvcBuilder source)
        {
            return source;
        }

        public Assembly Assembly => GetType().Assembly;
    }
}