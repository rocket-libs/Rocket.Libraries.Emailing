namespace Rocket.Libraries.Emailing.Services.Receiving
{
    using MailKit.Net.Imap;
    using MailKit.Net.Pop3;
    using Rocket.Libraries.Emailing.Services.Receiving.Imap;
    using Rocket.Libraries.Emailing.Services.Receiving.Pop3;
    using Rocket.Libraries.Validation.Services;

    public static class MailReceiverFactory
    {
        public static MailReceiverBase<TClient> Create<TClient>(TClient externalClient = default)
        {
            if (typeof(TClient) == typeof(ImapClient))
            {
                return new ImapReceiver() as MailReceiverBase<TClient>;
            }
            else if (typeof(TClient) == typeof(Pop3Client))
            {
                return new Pop3Receiver(externalClient as Pop3Client) as MailReceiverBase<TClient>;
            }
            else
            {
                using (var validator = new DataValidator())
                {
                    validator.EvaluateImmediate(true, $"Unsupported client type '{typeof(TClient).Name}'. Cannot create a receiver");
                }

                return null;
            }
        }
    }
}
