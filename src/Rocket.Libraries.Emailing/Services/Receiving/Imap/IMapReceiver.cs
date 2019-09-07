namespace Rocket.Libraries.Emailing.Services.Receiving.Imap
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MailKit.Net.Imap;
    using MimeKit;
    using Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting;

    public class ImapReceiver : MailReceiverBase<ImapClient>
    {
        private ReceivingClientProvider<ImapClient> _clientProvider;

        public override ReceivingClientProvider<ImapClient> ClientProvider
        {
            get
            {
                if (_clientProvider == null)
                {
                    _clientProvider = new ReceivingClientProvider<ImapClient>()
                        .SetAuthenticatorCall(async (client, settings) => await client.AuthenticateAsync(settings.User, settings.Password))
                        .SetConnectCall(async (client, settings) => await client.ConnectAsync(settings.Server, settings.Port, settings.UseSsl))
                        .SetLogFilename(LogFilename)
                        .SetLoggingConstructor((protocolLogger) => new ImapClient(protocolLogger))
                        .SetLogLessConstructor(() => new ImapClient())
                        .SetMailServerSettings(MailServerSettings);
                }

                return _clientProvider;
            }

            set => _clientProvider = value;
        }

        public override async Task<int> GetCountAsync()
        {
            var result = default(int);
            await MailboxAdapterTaskRunner(async (mailboxAdapter) =>
            {
                result = await Task.Run(() => mailboxAdapter.UseMailbox().MessageCount);
            });
            return result;
        }

        public override async Task<List<MimeMessage>> GetMailAsync()
        {
            var result = default(List<MimeMessage>);
            await MailboxAdapterTaskRunner(async (mailboxAdapter) =>
            {
                result = await new MailFetcher().GetMessagesAsync(mailboxAdapter, TerminalMessageId);
            });
            return result;
        }

        private async Task MailboxAdapterTaskRunner(Func<MailBoxAdapter, Task> funcTask)
        {
            using (ClientProvider)
            {
                using (var client = await ClientProvider.GetClientAsync())
                {
                    try
                    {
                        await client.Inbox.OpenAsync(MailKit.FolderAccess.ReadOnly);
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

        private MailBoxAdapter GetMailBoxAdapter(ImapClient client)
        {
            return new MailBoxAdapter()
                .UseConfigurator()
                .SetCounterFunction(() => client.Inbox.Count)
                .SetIndexedMessageReader(async (index) => await client.Inbox.GetMessageAsync(index))
                .Then();
        }
    }
}