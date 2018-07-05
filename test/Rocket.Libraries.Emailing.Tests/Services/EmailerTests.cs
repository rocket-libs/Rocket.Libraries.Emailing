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
            Environment.SetEnvironmentVariable("SPARKPOST_APIKEY", "fa0291b031781bd5dff87f1f4c6ebade277af621");
            Environment.SetEnvironmentVariable("SPARKPOST_SENDINGDOMAIN", "mail.rocketdocuments.com");

            var placeholders = new List<TemplatePlaceholder>
            {
                new TemplatePlaceholder
                {
                    Placeholder = "=text",
                    Text = "The quick brown fox jumps over the lazy dog"
                }
            };

            await new Emailer()
                .SendEmailAsync("nyingimaina@gmail.com", "Integration Tests", "The following text should be <b>Bold</b>", "text.htm", placeholders,"attachment name");
        }
    }
}
