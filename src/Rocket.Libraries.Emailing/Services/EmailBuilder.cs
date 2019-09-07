namespace Rocket.Libraries.Emailing.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Rocket.Libraries.Emailing.Models;
    using Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing;
    using Rocket.Libraries.Validation.Services;
    using SparkPostDotNet;
    using SparkPostDotNet.Transmissions;
    using Options = Microsoft.Extensions.Options.Options;

    public class EmailBuilder
    {
        private List<string> _recepients = new List<string>();
        private string _subject;
        private string _body;
        private string _attachmentTemplate;
        private string _attachmentName;
        private Dictionary<string, string> _attachmentFiles = new Dictionary<string, string>();

        private EmailingSettings _emailingSettings;
        private IOptions<SparkPostOptions> _sparkPostOptions;
        private TemplateReader _templateReader;
        private PdfWriter _pdfWriter;
        private PlaceholderWriter _placeholderWriter;
        private List<TemplatePlaceholder> _placeholders = new List<TemplatePlaceholder>();
        private object _placeholdersObject;
        private List<string> _bodyTemplateLines;
        private SenderInformation _senderInformation;

        private EmailingSettings EmailingSettings
        {
            get
            {
                return _emailingSettings;
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

        public EmailBuilder()
        {
            SetConfiguration(new ConfigReader().ReadConfiguration());
            CleanUp();
        }

        public EmailBuilder AddPlaceholdersObject(object placeholdersObject)
        {
            this._placeholdersObject = placeholdersObject;
            return this;
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

        public EmailBuilder AddRecepient(string recepient)
        {
            FailOnInvalidEmail(recepient);
            _recepients.Add(recepient);
            return this;
        }

        public EmailBuilder AddSubject(string subject)
        {
            _subject = subject;
            return this;
        }

        public EmailBuilder AddBodyAsTemplate(string templateFile)
        {
            _bodyTemplateLines = TemplateReader.GetContentFromTemplate(templateFile);
            AddBodyAsText(GetStringFromList(_bodyTemplateLines));
            return this;
        }

        public EmailBuilder AddPlaceholders(List<TemplatePlaceholder> placeholders)
        {
            _placeholders = placeholders;
            return this;
        }

        public EmailBuilder AddBodyAsText(string body)
        {
            _body = body;
            return this;
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

        public async Task<EmailSendingResult> BuildAsync()
        {
            try
            {
                PreprocessObjectTemplatesIfRequired();
                FailIfContentMissing();
                PreprocessForDevelopmentIfNeeded();
                var sparkPostClient = new SparkPostClient(_sparkPostOptions);
                var transmission = new Transmission();
                transmission.Content.From.EMail = _senderInformation.SenderEmail;
                transmission.Content.From.Name = PlaceholderWriter.GetWithPlaceholdersReplaced(_senderInformation.SenderName, _placeholders);
                transmission.Content.Subject = PlaceholderWriter.GetWithPlaceholdersReplaced(_subject, _placeholders);
                transmission.Content.Html = PlaceholderWriter.GetWithPlaceholdersReplaced(_body, _placeholders);

                InjectRecepients(transmission);

                AppendAttachmentFromTemplateIfExists(transmission);
                AppendAttachmentFromFilesIfExists(transmission);

                await sparkPostClient.CreateTransmission(transmission);
                return new EmailSendingResult { Succeeded = true };
            }
            finally
            {
                CleanUp();
            }
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

        private void FailOnInvalidEmail(string emailAddress)
        {
            new DataValidator().EvaluateImmediate(EmailingValidations.IsInvalidEmail(emailAddress), $"Email address '{emailAddress}' does not appear to be a valid email address. Please correct");
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
                    appendLine("Recepient " + counter, item);
                    counter++;
                }

                _recepients.Clear();
                _recepients.Add(EmailingSettings.DevelopmentEmail);
                _body = debugInfo + "<br/><br/>" + _body + "<br/><br/>";
            }
        }

        private void InjectRecepients(Transmission transmission)
        {
            foreach (var bcc in _recepients)
            {
                var recipient = new Recipient();
                recipient.Address.EMail = bcc;
                transmission.Recipients.Add(recipient);
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

        private string GetStringFromList(List<string> lines)
        {
            var stringBuilder = new StringBuilder();
            lines.ForEach(a => stringBuilder.Append(a));
            var str = stringBuilder.ToString();
            return str;
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

        private void AppendAttachmentFromFilesIfExists(Transmission transmission)
        {
            foreach (var item in _attachmentFiles)
            {
                new DataValidator().EvaluateImmediate(!File.Exists(item.Value), $"Could not find attachment file at '{item.Value}'");
                var attachmentBytes = File.ReadAllBytes(item.Value);
                AppendAttachment(transmission, attachmentBytes, item.Key);
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

        private void CleanUp()
        {
            AddBodyAsText(string.Empty)
                .AddAttachmentAsTemplate(string.Empty, string.Empty)
                .AddSubject(string.Empty);
            _recepients = new List<string>();
            _senderInformation = new SenderInformation();
        }
    }
}