namespace Rocket.Libraries.Emailing.Services.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MimeKit;
    using Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting;

    public class MailFetcher
    {
        public async Task<List<MimeMessage>> GetMessagesAsync(MailBoxAdapter mailBoxAdapter, string terminalMessageId)
        {
            using (mailBoxAdapter)
            {
                var result = new List<MimeMessage>();
                var mailBox = mailBoxAdapter.UseMailbox();
                for (int i = mailBox.MessageCount - 1; i >= 0; i--)
                {
                    var message = await mailBox.GetMessageAsync(i);
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
        }
    }
}