namespace Rocket.Libraries.Emailing.Services.Receiving.Imap
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MimeKit;
    using Rocket.Libraries.Emailing.Models.Receiving.Imap;

    public abstract class MailReceiverBase<TClient>
    {
        public MailReceiverBase()
            : this(default)

        {
        }

        public MailReceiverBase(TClient externalClient)
        {
            this.ExternalClient = externalClient;
        }

        public abstract ReceivingClientProvider<TClient> ClientProvider { get; set; }

        public TClient ExternalClient { get; private set; }

        protected string LogFilename { get; private set; }

        protected MailServerSettings MailServerSettings { get; private set; }

        protected string TerminalMessageId { get; private set; }

        public abstract Task<int> GetCountAsync();

        public abstract Task<List<MimeMessage>> GetMailAsync();

        public MailReceiverBase<TClient> SetLogfilename(string logFilename)
        {
            LogFilename = logFilename;
            return this;
        }

        public MailReceiverBase<TClient> SetMailServerSettings(MailServerSettings mailServerSettings)
        {
            MailServerSettings = mailServerSettings;
            return this;
        }

        public MailReceiverBase<TClient> SetTerminalMessageId(string terminalMessageId)
        {
            TerminalMessageId = terminalMessageId;
            return this;
        }
    }
}