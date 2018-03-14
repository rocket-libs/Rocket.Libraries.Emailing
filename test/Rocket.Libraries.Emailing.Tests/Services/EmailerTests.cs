using Microsoft.Extensions.Configuration;
using Rocket.Libraries.Emailing.Models;
using Rocket.Libraries.Emailing.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rocket.Libraries.Emailing.Tests.Services
{
    public class EmailerTests
    {
        [Fact]
        public async Task EmailGetsSent()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"Services/appsettings.json", false);

            var placeholders = new List<TemplatePlaceholder>
            {
                new TemplatePlaceholder
                {
                    Placeholder = "=text",
                    Text = "The quick brown fox jumps over the lazy dog"
                }
            };

            await new Emailer(builder.Build())
                .SendEmailAsync("nyingimaina@gmail.com", "Have A Cold", "text.htm", placeholders);
        }
    }
}
