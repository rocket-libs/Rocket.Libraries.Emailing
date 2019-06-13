namespace Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting
{
    using System;
    using System.Threading.Tasks;
    using MimeKit;

    public class MailboxAdapterConfigManager
    {
        private readonly MailBoxAdapter _mailBoxAdapter;

        public MailboxAdapterConfigManager(MailBoxAdapter mailBoxAdapter)
        {
            _mailBoxAdapter = mailBoxAdapter;
        }

        public Func<int> FuncCounter { get; private set; }

        public Func<int, Task<MimeMessage>> FuncIndexedMessageReader { get; set; }

        public MailboxAdapterConfigManager SetCounterFunction(Func<int> funcCounter)
        {
            FuncCounter = funcCounter;
            return this;
        }

        public MailboxAdapterConfigManager SetIndexedMessageReader(Func<int, Task<MimeMessage>> funcIndexedMessageReader)
        {
            FuncIndexedMessageReader = funcIndexedMessageReader;
            return this;
        }

        public MailBoxAdapter Then()
        {
            return _mailBoxAdapter;
        }
    }
}