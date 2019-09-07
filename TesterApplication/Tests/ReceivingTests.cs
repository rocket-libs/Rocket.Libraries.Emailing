using Rocket.Libraries.Emailing.Models.Receiving.Imap;
using Rocket.Libraries.Emailing.Services.Receiving.Imap;
using Rocket.Libraries.Emailing.Services.Receiving.Pop3;
using System;
using System.Threading.Tasks;

namespace TesterApplication.Tests
{
    public class ReceivingTests
    {
        public async Task FetchingEmailsWorksAsync()
        {
            var mailServerSettings = new MailServerSettings
            {
                Password = "Wednesday12th",
                Server = "pop-mail.outlook.com",
                User = "enquiries@auto-kenya.com",
                Port = 995,
                UseSsl = true
            };

            var mailCount = await new Pop3Receiver()
                .SetMailServerSettings(mailServerSettings)
                .GetCountAsync();
            Console.WriteLine($"Found {mailCount} mails in the inbox");
        }
    }
}