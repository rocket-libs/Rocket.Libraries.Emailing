using Rocket.Libraries.Emailing.Models;
using Rocket.Libraries.Emailing.Services.Sending;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TesterApplication.Tests
{
    internal class Basic
    {
        public async Task MailShotAsync()
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
                     .SendEmailAsync("nyingimaina@gmail.com", "Integration Test", "<b>Bold</b> text then <u>Underline</u>", "text.htm", placeholders, "attachment name");
            Console.WriteLine("Check your inbox");
        }
    }
}