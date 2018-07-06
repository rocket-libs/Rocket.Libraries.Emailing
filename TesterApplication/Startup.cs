using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SparkPostDotNet;
using SparkPostDotNet.Core;

namespace TesterApplication
{
    public class Startup
    {
        public IHostingEnvironment Env { get; }
        public IConfiguration Config { get; }

        public Startup(IHostingEnvironment env, IConfiguration config)
        {
            Config = config;
            Env = env;
        }

        // Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSparkPost();
            services.AddOptions(); // Most apps already are using this, but just in case.
            services.Configure<SparkPostOptions>(options => Config.GetSection("SparkPost").Bind(options));
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {   
        }
    }
}
