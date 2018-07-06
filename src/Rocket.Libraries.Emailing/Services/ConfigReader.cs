using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Emailing.Services
{
    class ConfigReader
    {
        public IConfiguration ReadConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var appSettingsGlobal = "appsettings.json";
            var appSettingsEnv = $"appsettings.{environmentName}.json";
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(appSettingsGlobal, optional: false, reloadOnChange: true)
                .AddJsonFile(appSettingsEnv, optional: true);
            return builder.Build();
        }
    }
}
