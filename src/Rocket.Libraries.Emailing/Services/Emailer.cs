namespace Rocket.Libraries.Emailing.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Rocket.Libraries.Emailing.Models;

    public class Emailer
    {
        private IConfiguration _configuration;
        public Emailer()
        {
            _configuration = GetConfig();
        }

        private IConfiguration GetConfig()
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

        [Obsolete("Please use email builder for more fine-tuning and clearer process.")]
        public async Task<EmailSendingResult> SendEmailAsync(string recepient, string subject, string body, string template, List<TemplatePlaceholder> placeholders, string attachmentName)
        {
            if (_configuration == null)
            {
                throw new Exception("Configuration is not set");
            }

            var emailBuilder = new EmailBuilder()
                .SetConfiguration(_configuration);

            return await emailBuilder
                .AddPlaceholders(placeholders)
                .AddBodyAsText(body)
                .AddAttachment(template, attachmentName)
                .AddRecepient(recepient)
                .AddSubject(subject)
                .BuildAsync();
        }
    }
}