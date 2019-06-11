namespace Rocket.Libraries.Emailing.Services.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using MailKit;
    using MailKit.Net.Imap;
    using MimeKit;
    using Rocket.Libraries.Validation.Services;

    public class InboxManager
    {
        private ImapClient _client;
        private DataValidator _dataValidator = new DataValidator();

        private bool InboxAvailable => _client != null && _client.IsConnected && Inbox != null;

        private IMailFolder Inbox
        {
            get
            {
                if (_client.Inbox.IsOpen == false)
                {
                    _client.Inbox.Open(FolderAccess.ReadOnly);
                }

                return _client.Inbox;
            }
        }

        public void SetClient(ImapClient client)
        {
            _client = client;
        }

        public virtual int GetMessageCount()
        {
            if (InboxAvailable == false)
            {
                return 0;
            }
            else
            {
                return Inbox.Count;
            }
        }

        public virtual async Task<MimeMessage> GetMessageAsync(int index)
        {
            if (InboxAvailable)
            {
                _dataValidator
                    .AddFailureCondition(() => index < 0, $"No messages are available for negative index '{index}'", false)
                    .AddFailureCondition(() => index >= Inbox.Count, $"Inbox has ${Inbox.Count} messages. Not possible to retrieve message at index '{index}'", false)
                    .ThrowExceptionOnInvalidRules();
                return await Inbox.GetMessageAsync(index);
            }
            else
            {
                _dataValidator.EvaluateImmediate(() => true, "Inbox is not available as client has not connected to server. Cannot fetch messages");
                return null;
            }
        }
    }
}