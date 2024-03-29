namespace Rocket.Libraries.Emailing.Services.Sending
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Rocket.Libraries.Emailing.Models.Sending;
    using Rocket.Libraries.Emailing.Services.Sending.TemplatePreprocessing.LoopsPreprocessing;
    using Rocket.Libraries.Validation.Services;
    using SparkPostDotNet;
    using SparkPostDotNet.Transmissions;
    using Options = Microsoft.Extensions.Options.Options;

    public class EmailBuilder
    {
        private Dictionary<string, string> _attachmentFiles = new Dictionary<string, string>();

        private string _attachmentName;

        private string _attachmentTemplate;

        private string _body;

        private List<string> _bodyTemplateLines;

        //private FilePlaceholderProcessor _filePlaceholderProcessor = new FilePlaceholderProcessor();
        private List<string> _ccList;

        private EmailingSettings _emailingSettings;

        private List<FilePlaceholder> _filePlaceholders = new List<FilePlaceholder>();

        private ILogger _logger;

        private PdfWriter _pdfWriter;

        private List<TemplatePlaceholder> _placeholders = new List<TemplatePlaceholder>();

        private object _placeholdersObject;

        private PlaceholderWriter _placeholderWriter;

        private string _primaryRecepient;

        private List<string> _recepients = new List<string>();

        private SenderInformation _senderInformation;

        private IOptions<SparkPostOptions> _sparkPostOptions;

        private string _subject;

        private TemplateReader _templateReader;

        public EmailBuilder()
        {
            SetConfiguration(new ConfigReader().ReadConfiguration());
            CleanUp();
        }

        public ImmutableList<TemplatePlaceholder> PlaceHolders => _placeholders != null ? _placeholders.ToImmutableList() : ImmutableList<TemplatePlaceholder>.Empty;

        private EmailingSettings EmailingSettings
        {
            get
            {
                return _emailingSettings;
            }
        }

        private ILogger Logger
        {
            get
            {
                if (_logger != null)
                {
                    return _logger;
                }
                else
                {
                    if (LoggerFactory == null)
                    {
                        return null;
                    }
                    else
                    {
                        _logger = LoggerFactory.CreateLogger<EmailBuilder>();
                        return _logger;
                    }
                }
            }
        }

        private ILoggerFactory LoggerFactory { get; set; }

        private PdfWriter PdfWriter
        {
            get
            {
                if (_pdfWriter == null)
                {
                    _pdfWriter = new PdfWriter();
                }

                return _pdfWriter;
            }
        }

        private PlaceholderWriter PlaceholderWriter
        {
            get
            {
                if (_placeholderWriter == null)
                {
                    _placeholderWriter = new PlaceholderWriter();
                }

                return _placeholderWriter;
            }
        }

        private TemplateReader TemplateReader
        {
            get
            {
                if (_templateReader == null)
                {
                    _templateReader = new TemplateReader(EmailingSettings);
                }

                return _templateReader;
            }
        }

        public EmailBuilder AddAttachmentAsTemplate(string attachementFile, string attachmentName)
        {
            _attachmentTemplate = attachementFile;
            _attachmentName = attachmentName;
            return this;
        }

        public EmailBuilder AddAttachmentFile(string attachmentFile, string name)
        {
            _attachmentFiles.Add(name, attachmentFile);
            return this;
        }

        public EmailBuilder AddBCCRecepient(string recepient)
        {
            return QueueRecepient(recepient);
        }

        public EmailBuilder AddBodyAsTemplate(string templateFile)
        {
            _bodyTemplateLines = TemplateReader.GetContentFromTemplate(templateFile);
            AddBodyAsText(GetStringFromList(_bodyTemplateLines));
            return this;
        }

        public EmailBuilder AddBodyAsText(string body)
        {
            _body = body;
            return this;
        }

        public EmailBuilder AddCCRecepient(string recepient)
        {
            _ccList.Add(recepient);
            return QueueRecepient(recepient);
        }

        public EmailBuilder AddFilePlaceholder(string placeholder, string file)
        {
            _filePlaceholders.Add(new FilePlaceholder
            {
                File = file,
                Placeholder = placeholder,
            });
            return this;
        }

        public EmailBuilder AddLoggerFactory(ILoggerFactory loggerFactory)
        {
            loggerFactory = loggerFactory;
            return this;
        }

        public EmailBuilder AddPlaceholder(string name, object value)
        {
            var placeholder = new TemplatePlaceholder
            {
                Placeholder = "{{" + name + "}}",
                Text = value.ToString(),
            };
            _placeholders.Add(placeholder);
            return this;
        }

        public EmailBuilder AddPlaceholders(List<TemplatePlaceholder> placeholders)
        {
            _placeholders = placeholders;
            return this;
        }

        public EmailBuilder AddPlaceholdersObject(object placeholdersObject)
        {
            this._placeholdersObject = placeholdersObject;
            return this;
        }

        [Obsolete("This method defaults to BCC. Please Use the more descriptive 'SetPrimaryRecepient', 'AddBCCRecepient' and 'AddCCRecepient' methods")]
        public EmailBuilder AddRecepient(string recepient)
        {
            return QueueRecepient(recepient);
        }

        public EmailBuilder AddSender(string email, string name)
        {
            FailOnInvalidEmail(email);
            _senderInformation = new SenderInformation
            {
                SenderEmail = email,
                SenderName = name,
            };
            return this;
        }

        public EmailBuilder AddSubject(string subject)
        {
            _subject = subject;
            return this;
        }

        public async Task<EmailSendingResult> BuildAsync()
        {
            try
            {
                var filePlaceholderProcessor = new FilePlaceholderProcessor(TemplateReader);
                PreprocessObjectTemplatesIfRequired();
                FailIfContentMissing();
                PreprocessForDevelopmentIfNeeded();
                var sparkPostClient = new SparkPostClient(_sparkPostOptions);
                var transmission = new Transmission();
                transmission.Content.From.EMail = _senderInformation.SenderEmail;
                transmission.Content.From.Name = PlaceholderWriter.GetWithPlaceholdersReplaced(_senderInformation.SenderName, _placeholders);
                transmission.Content.Subject = PlaceholderWriter.GetWithPlaceholdersReplaced(_subject, _placeholders);
                _body = filePlaceholderProcessor.PreprocessFilePlaceholdersIfRequired(_body, _filePlaceholders);
                transmission.Content.Html = PlaceholderWriter.GetWithPlaceholdersReplaced(_body, _placeholders);
                Logger?.LogDebug("Out going email body");
                Logger?.LogDebug(transmission.Content.Html);
                InjectRecepients(transmission);
                SpecifyCCsIfAvailable(transmission);

                AppendAttachmentFromTemplateIfExists(transmission);
                AppendAttachmentFromFilesIfExists(transmission);

                ThrowExceptionOnUnResolvedPlaceholder(transmission.Content.Html);
                await sparkPostClient.CreateTransmission(transmission);
                return new EmailSendingResult { Succeeded = true };
            }
            finally
            {
                CleanUp();
            }
        }

        /// <summary>
        /// Sets the primary recepient for the message. There can only be one
        /// </summary>
        /// <param name="recepient">The email address of the primary recepient</param>
        /// <returns>Instance of the <see cref="EmailBuilder"/></returns>
        public EmailBuilder SetPrimaryRecepient(string recepient)
        {
            _primaryRecepient = recepient;
            return QueueRecepient(recepient);
        }

        internal EmailBuilder SetConfiguration(IConfiguration configuration)
        {
            _emailingSettings = configuration.GetSection("Emailing").Get<EmailingSettings>();
            _sparkPostOptions = Options.Create(configuration.GetSection("SparkPost").Get<SparkPostOptions>());
            if (_emailingSettings == null)
            {
                throw new NullReferenceException("Could not find settings for emailing in your appsettings.json file");
            }

            if (_sparkPostOptions == null)
            {
                throw new NullReferenceException("Could not find SparkPost integration settings in your appsettings.json file");
            }

            return this;
        }

        private void AddHeaderToValueIfPrimaryRecepientAvailable(Recipient recipient)
        {
            var hasPrimaryRecepient = string.IsNullOrEmpty(_primaryRecepient) == false;
            if (hasPrimaryRecepient)
            {
                recipient.Address.HeaderTo = _primaryRecepient;
            }
        }

        private void AppendAttachment(Transmission transmission, byte[] attachmentBytes, string attachmentName)
        {
            var attachment = new Attachment();
            attachment.Data = attachmentBytes;
            attachment.Name = attachmentName;
            attachment.Type = "application/pdf";
            transmission.Content.Attachments.Add(attachment);
        }

        private void AppendAttachmentFromFilesIfExists(Transmission transmission)
        {
            foreach (var item in _attachmentFiles)
            {
                using (var validator = new DataValidator())
                {
                    validator.EvaluateImmediate(!File.Exists(item.Value), $"Could not find attachment file at '{item.Value}'");
                }
                var attachmentBytes = File.ReadAllBytes(item.Value);
                AppendAttachment(transmission, attachmentBytes, item.Key);
            }
        }

        private void AppendAttachmentFromTemplateIfExists(Transmission transmission)
        {
            var hasAttachmentTemplate = !string.IsNullOrEmpty(_attachmentTemplate);
            if (hasAttachmentTemplate)
            {
                var attachmentLines = TemplateReader.GetContentFromTemplate(_attachmentTemplate);
                var attachmentContent = GetStringFromList(attachmentLines);
                AppendAttachment(transmission, PdfWriter.GetPdfBytes(PlaceholderWriter.GetWithPlaceholdersReplaced(attachmentContent, _placeholders)), _attachmentName);
            }
            else
            {
                return;
            }
        }

        private void CleanUp()
        {
            AddBodyAsText(string.Empty)
                .AddAttachmentAsTemplate(string.Empty, string.Empty)
                .AddSubject(string.Empty);
            _recepients = new List<string>();
            _senderInformation = new SenderInformation();
            _ccList = new List<string>();
        }

        private void FailIfContentMissing()
        {
            var contents = new Dictionary<string, string>
            {
                {"Subject", _subject },
                {"Body", _body },
                {"Sender Email", _senderInformation?.SenderEmail },
                {"Sender Name", _senderInformation?.SenderName },
            };
            foreach (var item in contents)
            {
                if (string.IsNullOrEmpty(item.Value))
                {
                    throw new Exception($"No '{item.Key}' was found in your email message");
                }
            }

            if (_recepients.Count == 0)
            {
                throw new Exception("Your email message has no recepients");
            }
        }

        private void FailIfContentNotArrayForObjectPlaceholders()
        {
            if (_placeholdersObject == null)
            {
                return;
            }
            else
            {
                if (_bodyTemplateLines == null)
                {
                    throw new Exception("Cannot preprocess strings, only lists are allowed");
                }
            }
        }

        private void FailOnInvalidEmail(string emailAddress)
        {
            using (var validator = new DataValidator())
            {
                validator.EvaluateImmediate(EmailingValidations.IsInvalidEmail(emailAddress), $"Email address '{emailAddress}' does not appear to be a valid email address. Please correct");
            }
        }

        private string GetStringFromList(List<string> lines)
        {
            var stringBuilder = new StringBuilder();
            lines.ForEach(a => stringBuilder.Append(a));
            var str = stringBuilder.ToString();
            return str;
        }

        private void InjectRecepients(Transmission transmission)
        {
            foreach (var party in _recepients)
            {
                var recipient = new Recipient();
                recipient.Address.EMail = party;
                AddHeaderToValueIfPrimaryRecepientAvailable(recipient);
                transmission.Recipients.Add(recipient);
            }
        }

        private bool Matches(string recepient, string test)
        {
            if (string.IsNullOrEmpty(test))
            {
                return false;
            }
            else
            {
                return test.Equals(recepient, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        private void PreprocessForDevelopmentIfNeeded()
        {
            if (EmailingSettings.IsDevelopment)
            {
                var debugInfo = string.Empty;
                Action<string, string> appendLine = (key, value) => debugInfo += $"<b>{key}:</b> {value}<br/>";
                var counter = 1;
                appendLine("Mode", "Development");
                appendLine("Actual Recepients", string.Empty);
                foreach (var item in _recepients)
                {
                    var isPrimary = Matches(item, _primaryRecepient);
                    var isCC = false;
                    _ccList.ForEach(cc =>
                    {
                        isCC = isCC || Matches(item, cc);
                    });

                    var tag = isPrimary ? "Primary" : isCC ? "CC" : "BCC";

                    appendLine("Recepient " + counter + $"({tag})", item);
                    counter++;
                }

                _ccList.Clear();
                _recepients.Clear();
                _recepients.Add(EmailingSettings.DevelopmentEmail);
                _body = debugInfo + "<br/><br/>" + _body + "<br/><br/>";
            }
        }

        private void PreprocessObjectTemplatesIfRequired()
        {
            if (_placeholdersObject == null)
            {
                return;
            }

            FailIfContentNotArrayForObjectPlaceholders();
            var result = new LoopsPreprocessor(_placeholdersObject, _bodyTemplateLines, 1, string.Empty, null).PreProcess();
            AddBodyAsText(GetStringFromList(result.TemplateLines));
            _placeholders.AddRange(result.Placeholders);
        }

        private EmailBuilder QueueRecepient(string recepient)
        {
            FailOnInvalidEmail(recepient);
            _recepients.Add(recepient);
            return this;
        }

        private void SpecifyCCsIfAvailable(Transmission transmission)
        {
            var ccs = string.Empty;
            foreach (var cc in _ccList)
            {
                ccs += $"{cc},";
            }

            var hasCCs = !string.IsNullOrEmpty(ccs);

            if (hasCCs)
            {
                ccs = ccs.Substring(0, ccs.Length - 1);
                transmission.Content.Headers = new
                {
                    CC = ccs,
                };
            }
        }

        private void ThrowExceptionOnUnResolvedPlaceholder(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var startPos = content.IndexOf("{{", StringComparison.InvariantCultureIgnoreCase);
            var didntFindStarter = startPos < 0;
            if (didntFindStarter)
            {
                return;
            }
            else
            {
                var endPos = content.IndexOf("}}", startPos, StringComparison.InvariantCultureIgnoreCase);
                var foundEnder = endPos >= 0;
                if (foundEnder)
                {
                    var length = endPos - startPos + 2;
                    var placeholder = content.Substring(startPos, length);
                    using (var validator = new DataValidator())
                    {
                        validator.EvaluateImmediate(true, $"Unresolved place holder '{placeholder}'");
                    }
                }
            }
        }
    }
}