using Microsoft.Extensions.Configuration;
using Rocket.Libraries.Emailing.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rocket.Libraries.Emailing.Services
{
    public class Emailer
    {
        private IConfiguration _configuration;
        public Emailer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<EmailSendingResult> SendEmailAsync(string recepient, string subject, string template, List<TemplatePlaceholder> placeholders)
        {
            var emailBuilder = new EmailBuilder()
                .SetConfiguration(_configuration);

            string body = GetWithPlaceholdersReplaced(GetBodyFromTemplate($"{emailBuilder.EmailingSettings.TemplatesDirectory}\\{template}"), placeholders);
            subject = GetWithPlaceholdersReplaced(subject, placeholders);

            return await emailBuilder
                .AddBody(body)
                .AddRecepient(recepient)
                .AddSubject(subject)
                .BuildAsync();
        }

        private string GetBodyFromTemplate(string template)
        {
            using (var fs = new FileStream(template, FileMode.Open,FileAccess.Read))
            {
                using (var stream = new StreamReader(fs))
                {
                    return stream.ReadToEnd();
                }
            }
        }

        private string GetWithPlaceholdersReplaced(string body,List<TemplatePlaceholder> placeholders)
        {
            foreach(var placeholder in placeholders)
            {
                body = body.Replace(placeholder.Placeholder,placeholder.Text);
            }
            return body;
        }
    }
}