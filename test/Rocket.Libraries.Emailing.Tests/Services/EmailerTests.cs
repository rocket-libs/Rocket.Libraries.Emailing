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
        [Fact(Skip = "Cannot find the templates dir")]
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

            new Emailer()
                .SendEmail("nyingimaina@gmail.com", "Have A Cold", "This is the body <b>Boldy</b>", "text.htm", placeholders,"attachment name");
        }
    }
}
