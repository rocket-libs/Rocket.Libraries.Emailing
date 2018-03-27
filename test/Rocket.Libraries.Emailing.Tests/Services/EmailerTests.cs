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
            var placeholders = new List<TemplatePlaceholder>
            {
                new TemplatePlaceholder
                {
                    Placeholder = "=text",
                    Text = "The quick brown fox jumps over the lazy dog"
                }
            };

            await new Emailer()
                .SendEmailAsync("nyingimaina@gmail.com", "Have A Cold", "This is the body <b>Boldy</b>", "text.htm", placeholders,"attachment name");
        }
    }
}
