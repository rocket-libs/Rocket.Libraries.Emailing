namespace Rocket.Libraries.Emailing.Services.Receiving.MailboxAdapting
{
    using System.Threading.Tasks;
    using MimeKit;
    using Rocket.Libraries.Validation.Services;

    public class MailBoxWrapper
    {
        private readonly MailBoxAdapter _mailBoxAdapter;

        public MailBoxWrapper()
        {
        }

        public MailBoxWrapper(MailBoxAdapter mailBoxAdapter)
        {
            _mailBoxAdapter = mailBoxAdapter;
            FailIfNotSetupCorrectly();
        }

        public virtual int MessageCount
        {
            get
            {
                return _mailBoxAdapter.UseConfigurator().FuncCounter();
            }
        }

        public virtual async Task<MimeMessage> GetMessageAsync(int index)
        {
            return await _mailBoxAdapter.UseConfigurator().FuncIndexedMessageReader(index);
        }

        private void FailIfNotSetupCorrectly()
        {
            using (var validator = new DataValidator())
            {
                validator
                    .AddFailureCondition(_mailBoxAdapter.UseConfigurator().FuncIndexedMessageReader == null, $"Function to read messages is not set", false)
                    .AddFailureCondition(_mailBoxAdapter.UseConfigurator().FuncCounter == null, $"Function to get count of messages is not set", false)
                    .ThrowExceptionOnInvalidRules();
            }
        }
    }
}
