namespace Rocket.Libraries.Emailing.Services.Receiving.Imap
{
    using System;
    using System.Threading.Tasks;
    using MailKit;
    using Rocket.Libraries.Emailing.Models.Receiving.Imap;
    using Rocket.Libraries.Validation.Services;

    public class ReceivingClientProvider<TClient> : IDisposable
    {
        private Func<TClient, MailServerSettings, Task> _funcAuthenticator;

        private Func<TClient, MailServerSettings, Task> _funcConnect;

        private Func<ProtocolLogger, TClient> _funcLoggingConstructor;

        private Func<TClient> _funcLoglessConstructor;

        private string _logFilename;

        private MailServerSettings _mailServerSettings;

        private TClient externalClient;

        private ProtocolLogger ProtocolLogger { get; set; }

        public void Dispose()
        {
            ProtocolLogger?.Dispose();
            ProtocolLogger = null;
        }

        public virtual async Task<TClient> GetClientAsync()
        {
            if (externalClient != null)
            {
                return externalClient;
            }
            else
            {
                FailIfInvalid();
                CreateProtocolLoggerIfPossible();
                var client = ProtocolLogger != null ? _funcLoggingConstructor(ProtocolLogger) : _funcLoglessConstructor();
                await _funcConnect(client, _mailServerSettings);
                await _funcAuthenticator(client, _mailServerSettings);
                return client;
            }
        }

        public ReceivingClientProvider<TClient> SetAuthenticatorCall(Func<TClient, MailServerSettings, Task> funcAuthenticator)
        {
            _funcAuthenticator = funcAuthenticator;
            return this;
        }

        public ReceivingClientProvider<TClient> SetConnectCall(Func<TClient, MailServerSettings, Task> funcConnect)
        {
            _funcConnect = funcConnect;
            return this;
        }

        public ReceivingClientProvider<TClient> SetExternalClient(TClient client)
        {
            externalClient = client;
            return this;
        }

        public ReceivingClientProvider<TClient> SetLogFilename(string logFilename)
        {
            _logFilename = logFilename;
            return this;
        }

        public ReceivingClientProvider<TClient> SetLoggingConstructor(Func<ProtocolLogger, TClient> funcLoggingConstructor)
        {
            _funcLoggingConstructor = funcLoggingConstructor;
            return this;
        }

        public ReceivingClientProvider<TClient> SetLogLessConstructor(Func<TClient> funcLoglessConstrutor)
        {
            _funcLoglessConstructor = funcLoglessConstrutor;
            return this;
        }

        public ReceivingClientProvider<TClient> SetMailServerSettings(MailServerSettings mailServerSettings)
        {
            _mailServerSettings = mailServerSettings;
            return this;
        }

        private void CreateProtocolLoggerIfPossible()
        {
            var hasLogFilename = !string.IsNullOrEmpty(_logFilename);
            if (hasLogFilename)
            {
                ProtocolLogger = new ProtocolLogger(_logFilename);
            }
        }

        private void FailIfInvalid()
        {
            using (var validator = new DataValidator())
            {
                validator
                    .AddFailureCondition(_mailServerSettings == null, "Mail server settings are not available. Cannot continue", false)
                    .AddFailureCondition(_funcLoggingConstructor == null, "Logging constructor is not specified", false)
                    .AddFailureCondition(_funcLoglessConstructor == null, "Logless constructor is not specified", false)
                    .AddFailureCondition(_funcConnect == null, "Connector function is not specified. Cannot connect to mail server", false)
                    .AddFailureCondition(_funcAuthenticator == null, "Authenticator function is not specified. Cannot authenticate against mail server", false)
                    .ThrowExceptionOnInvalidRules();
            }
        }
    }
}
