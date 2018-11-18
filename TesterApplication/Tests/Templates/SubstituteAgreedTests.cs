using Rocket.Libraries.Emailing.Models;
using Rocket.Libraries.Emailing.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TesterApplication.Tests.Templates
{
    class SubstituteAgreedTests
    {
        public async Task SendAsync()
        {
            await new EmailBuilder()
                .AddBodyAsTemplate("substitute-agreed.htm")
                .AddFilePlaceholder("{{head}}", "sections/head.section")
                .AddRecepient("nyingimaina@gmail.com")
                .AddSender("nyingimaina@rocketdocuments.com", "Nyingi Maina")
                .AddSubject("Hi")
                .AddPlaceholders(GetPlaceholders())
                .BuildAsync();
        }

        private List<TemplatePlaceholder> GetPlaceholders()
        {
            return new List<TemplatePlaceholder>
            {
                new TemplatePlaceholder
                {
                    Placeholder = "{{substitute-name}}",
                    Text = "Jessica Jones",
                },
                new TemplatePlaceholder
                {
                    Placeholder = "{{start-date}}",
                    Text = "Today",
                },
                new TemplatePlaceholder
                {
                    Placeholder = "{{task-link}}",
                    Text = "http://google.com",
                },
                new TemplatePlaceholder
                {
                    Placeholder = "{{app-name}}",
                    Text = "Price",
                },
                new TemplatePlaceholder
                {
                    Placeholder = "{{company-name}}",
                    Text = "Free",
                }
            };
        }
    }
}
