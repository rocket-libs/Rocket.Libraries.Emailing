namespace Rocket.Libraries.Emailing.Services.Receiving.Pop3
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MailKit.Net.Pop3;
    using MimeKit;
    using Rocket.Libraries.Emailing.Services.Receiving.Imap;
    using Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting;

    public class Pop3Receiver : MailReceiverBase<Pop3Client>
    {
        private ReceivingClientProvider<Pop3Client> _clientProvider;

        public override ReceivingClientProvider<Pop3Client> ClientProvider
        {
            get
            {
                if (_clientProvider == null)
                {
                    _clientProvider = new ReceivingClientProvider<Pop3Client>()
                        .SetAuthenticatorCall(async (client, settings) => await client.AuthenticateAsync(settings.User, settings.Password))
                        .SetConnectCall(async (client, settings) => await client.ConnectAsync(settings.Server, settings.Port, settings.UseSsl))
                        .SetLogFilename(LogFilename)
                        .SetLoggingConstructor((protocolLogger) => new Pop3Client(protocolLogger))
                        .SetLogLessConstructor(() => new Pop3Client())
                        .SetMailServerSettings(MailServerSettings);
                }

                return _clientProvider;
            }

            set => _clientProvider = value;
        }

        public override async Task<int> GetCountAsync()
        {
            var count = default(int);
            await MailboxAdapterTaskRunner(async (mailboxAdapter) =>
            {
                count = await Task.Run(() => mailboxAdapter.UseMailbox().MessageCount);
            });

            return count;
        }

        public override async Task<List<MimeMessage>> GetMailAsync()
        {
            var messages = default(List<MimeMessage>);
            await MailboxAdapterTaskRunner(async (mailboxAdapter) =>
            {
                messages = await new MailFetcher().GetMessagesAsync(mailboxAdapter, TerminalMessageId);
            });
            return messages;
        }

        private MailBoxAdapter GetMailBoxAdapter(Pop3Client client)
        {
            return new MailBoxAdapter()
                .UseConfigurator()
                .SetCounterFunction(() => client.Count)
                .SetIndexedMessageReader(async (index) => await client.GetMessageAsync(index))
                .Then();
        }

        private async Task MailboxAdapterTaskRunner(Func<MailBoxAdapter, Task> funcTask)
        {
            using (ClientProvider)
            {
                using (var client = await ClientProvider.GetClientAsync())
                {
                    try
                    {
                        var mailboxAdapter = GetMailBoxAdapter(client);
                        await funcTask(mailboxAdapter);
                    }
                    finally
                    {
                        client?.Disconnect(true);
                    }
                }
            }
        }
    }
}