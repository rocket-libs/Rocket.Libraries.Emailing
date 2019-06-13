namespace Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using MimeKit;
    using Rocket.Libraries.Validation.Services;

    public class MailBoxAdapter : IDisposable
    {
        public MailBoxAdapter()
        {
        }

        public MailBoxAdapter(MailBoxWrapper mailBoxWrapper)
        {
            _mailBox = mailBoxWrapper;
        }

        private MailboxAdapterConfigManager _mailboxAdapterConfigManager;

        private MailBoxWrapper _mailBox;

        public void Dispose()
        {
            _mailBox = null;
            _mailboxAdapterConfigManager = null;
        }

        public MailBoxWrapper UseMailbox()
        {
            if (_mailBox == null)
            {
                _mailBox = new MailBoxWrapper(this);
            }

            return _mailBox;
        }

        public MailboxAdapterConfigManager UseConfigurator()
        {
            if (_mailboxAdapterConfigManager == null)
            {
                _mailboxAdapterConfigManager = new MailboxAdapterConfigManager(this);
            }

            return _mailboxAdapterConfigManager;
        }
    }
}