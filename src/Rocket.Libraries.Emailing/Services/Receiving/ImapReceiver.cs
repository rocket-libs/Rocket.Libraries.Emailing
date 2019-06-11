namespace Rocket.Libraries.Emailing.Services.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using MailKit;
    using MailKit.Net.Imap;
    using MimeKit;
    using Rocket.Libraries.Emailing.Models.Receiving;
    using Rocket.Libraries.Validation.Services;

    public class ImapReceiver
    {
        public InboxManager InboxManager { get; set; } = new InboxManager();

        public ImapClientProvider ImapClientProvider { get; set; } = new ImapClientProvider();

        public async Task<List<MimeMessage>> GetMailAsync(ImapSettings imapSettings, string terminalMessageId)
        {
            using (var client = await ImapClientProvider.GetClientAsync(imapSettings))
            {
                try
                {
                    new DataValidator()
                        .AddFailureCondition(() => imapSettings == null, $"No imap settings receieved", true)
                        .ThrowExceptionOnInvalidRules();
                    InboxManager.SetClient(client);
                    var result = new List<MimeMessage>();

                    for (int i = InboxManager.GetMessageCount() - 1; i >= 0; i--)
                    {
                        var message = await InboxManager.GetMessageAsync(i);
                        var shouldTerminateReading = message.MessageId.Equals(terminalMessageId, StringComparison.InvariantCultureIgnoreCase);
                        if (shouldTerminateReading)
                        {
                            break;
                        }
                        else
                        {
                            result.Add(message);
                        }
                    }

                    return result;
                }
                finally
                {
                    client?.Disconnect(true);
                }
            }
        }
    }
}