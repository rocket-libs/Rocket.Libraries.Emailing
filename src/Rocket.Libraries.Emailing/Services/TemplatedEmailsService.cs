using Rocket.Libraries.Emailing.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Rocket.Libraries.Emailing.Services
{
    public class TemplatedEmailsService
    {

        public async Task<EmailSendingResult> SendEmailAsync(string recepient, string subject, string template, List<TemplatePlaceholder> placeholders)
        {
            string body = GetWithPlaceholdersReplaced(GetBodyFromTemplate(template), placeholders);
            subject = GetWithPlaceholdersReplaced(subject, placeholders);

            return await new EmailBuilderService()
                .AddBody(body)
                .AddRecepient(recepient)
                .AddSubject(subject)
                .BuildAsync();
        }

        private string GetBodyFromTemplate(string template)
        {
            using (var fs = new FileStream( $"./EmailTemplates/{template}", FileMode.Open,FileAccess.Read))
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