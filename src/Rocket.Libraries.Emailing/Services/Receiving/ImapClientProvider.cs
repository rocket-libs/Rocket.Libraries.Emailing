namespace Rocket.Libraries.Emailing.Services.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using MailKit;
    using MailKit.Net.Imap;
    using Rocket.Libraries.Emailing.Models.Receiving;

    public class ImapClientProvider
    {
        public virtual async Task<ImapClient> GetClientAsync(ImapSettings imapSettings)
        {
            var client = new ImapClient();
            await client.ConnectAsync(imapSettings.Server, imapSettings.Port, true);

            await client.AuthenticateAsync(imapSettings.User, imapSettings.Password);
            return client;
        }
    }
}