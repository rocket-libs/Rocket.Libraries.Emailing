namespace Rocket.Libraries.Emailing.Services.Receiving.Imap
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MimeKit;
    using Rocket.Libraries.Emailing.Models.Receiving.Imap;

    public abstract class MailReceiverBase<TClient>
    {
        public abstract ReceivingClientProvider<TClient> ClientProvider { get; set; }

        protected string LogFilename { get; private set; }

        protected MailServerSettings MailServerSettings { get; private set; }

        protected string TerminalMessageId { get; private set; }

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

        public MailReceiverBase<TClient> SetLogfilename(string logFilename)
        {
            LogFilename = logFilename;
            return this;
        }

        public abstract Task<List<MimeMessage>> GetMailAsync();

        public abstract Task<int> GetCountAsync();
    }
}