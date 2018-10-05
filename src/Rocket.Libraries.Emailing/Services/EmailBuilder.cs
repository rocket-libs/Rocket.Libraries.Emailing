using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rocket.Libraries.Emailing.Models;
using Rocket.Libraries.Emailing.Services.TemplatePreprocessing.LoopsPreprocessing;
using SparkPostDotNet;
using SparkPostDotNet.Transmissions;
using Options = Microsoft.Extensions.Options.Options;

namespace Rocket.Libraries.Emailing.Services
{
    public class EmailBuilder
    {
        private List<string> _recepients = new List<string>();
        private string _subject;
        private string _body;
        private string _attachmentFile;
        private string _attachmentName;

        private EmailingSettings _emailingSettings;
        private IOptions<SparkPostOptions> _sparkPostOptions;
        private TemplateReader _templateReader;
        private PdfWriter _pdfWriter;
        private PlaceholderWriter _placeholderWriter;
        private List<TemplatePlaceholder> _placeholders = new List<TemplatePlaceholder>();
        private object _placeholdersObject;
        private List<string> _bodyTemplateLines;

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

        public EmailBuilder AddAttachment(string attachementFile, string attachmentName)
        {
            _attachmentFile = attachementFile;
            _attachmentName = attachmentName;
            return this;
        }

        public EmailBuilder AddRecepient(string recepient)
        {
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

        public async Task<EmailSendingResult> BuildAsync()
        {
            try
            {
                PreprocessObjectTemplatesIfRequired();
                FailIfContentMissing();
                var sparkPostClient = new SparkPostClient(_sparkPostOptions);
                var transmission = new Transmission();
                transmission.Content.From.EMail = "noreply@rocketdocuments.com";
                transmission.Content.From.Name = PlaceholderWriter.GetWithPlaceholdersReplaced(_emailingSettings.SenderName, _placeholders);
                transmission.Content.Subject = PlaceholderWriter.GetWithPlaceholdersReplaced(_subject, _placeholders);
                transmission.Content.Html = PlaceholderWriter.GetWithPlaceholdersReplaced(_body, _placeholders);

                InjectRecepients(transmission);

                AddAttachmentIfExists(transmission);

                await sparkPostClient.CreateTransmission(transmission);
                return new EmailSendingResult { Succeeded = true };
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                CleanUp();
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
            var result = new LoopsPreprocessor(_placeholdersObject, _bodyTemplateLines).PreProcess();
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
                {"Body", _body }
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
            return stringBuilder.ToString();
        }

        private void AddAttachmentIfExists(Transmission transmission)
        {
            var hasAttachment = !string.IsNullOrEmpty(_attachmentFile);
            if (hasAttachment)
            {
                var attachment = new Attachment();
                var attachmentLines = TemplateReader.GetContentFromTemplate(_attachmentFile);
                var attachmentContent = GetStringFromList(attachmentLines);
                attachment.Data = PdfWriter.GetPdfBytes(PlaceholderWriter.GetWithPlaceholdersReplaced(attachmentContent, _placeholders));
                attachment.Name = $"{_attachmentName}.pdf";
                attachment.Type = "application/pdf";
                transmission.Content.Attachments.Add(attachment);
            }
            else
            {
                return;
            }
        }

        private void CleanUp()
        {
            AddBodyAsText(string.Empty)
                .AddAttachment(string.Empty, string.Empty)
                .AddRecepient(string.Empty)
                .AddSubject(string.Empty);
        }
    }
}