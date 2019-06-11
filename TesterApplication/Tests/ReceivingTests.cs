using Rocket.Libraries.Emailing.Models.Receiving;
using Rocket.Libraries.Emailing.Services.Receiving;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TesterApplication.Tests
{
    public class ReceivingTests
    {
        public async Task FetchingEmailsWorksAsync()
        {
            var imapSettings = new ImapSettings
            {
                Password = "Monday4th",
                Server = "imap-mail.outlook.com",
                User = "nyingi@auto-kenya.com",
            };

            var mails = await new ImapReceiver().GetMailAsync(imapSettings, string.Empty);
            Console.WriteLine($"Found {mails.Count} mails in the inbox");
        }
    }
}