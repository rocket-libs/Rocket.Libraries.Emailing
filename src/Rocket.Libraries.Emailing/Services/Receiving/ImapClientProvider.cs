namespace Rocket.Libraries.Emailing.Services.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using MailKit;
    using MailKit.Net.Imap;
    using Rocket.Libraries.Emailing.Models.Receiving;

    public class ImapClientProvider : IDisposable
    {
        private ProtocolLogger ProtocolLogger { get; set; }

        public void Dispose()
        {
            ProtocolLogger?.Dispose();
            ProtocolLogger = null;
        }

        public virtual async Task<ImapClient> GetClientAsync(ImapSettings imapSettings, string logFilename)
        {
            CreateProtocolLoggerIfPossible(logFilename);
            var client = ProtocolLogger != null ? new ImapClient(ProtocolLogger) : new ImapClient();
            await client.ConnectAsync(imapSettings.Server, imapSettings.Port, imapSettings.UseSsl);
            await client.AuthenticateAsync(imapSettings.User, imapSettings.Password);
            return client;
        }

        private void CreateProtocolLoggerIfPossible(string logFilename)
        {
            var hasLogFilename = !string.IsNullOrEmpty(logFilename);
            if (hasLogFilename)
            {
                ProtocolLogger = new ProtocolLogger(logFilename);
            }
        }
    }
}