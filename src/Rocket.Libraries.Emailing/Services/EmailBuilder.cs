using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DinkToPdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Rocket.Libraries.Emailing.Models;
using SparkPostDotNet;
using SparkPostDotNet.Transmissions;
using Options = Microsoft.Extensions.Options.Options;

namespace Rocket.Libraries.Emailing.Services
{
    public class EmailBuilder
    {
        private string _recepient;
        private string _subject;
        private string _body;
        private string _attachmentFile;
        private string _attachmentName;

        
        private EmailingSettings _emailingSettings;
        private IOptions<SparkPostOptions> _sparkPostOptions;
        private TemplateReader _templateReader;
        private PdfWriter _pdfWriter;
        private PlaceholderWriter _placeholderWriter;
        private List<TemplatePlaceholder> _placeholders;

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
                if(_templateReader == null)
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
                if(_pdfWriter == null)
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
                if(_placeholderWriter == null)
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

        public EmailBuilder AddAttachment(string attachementFile, string attachmentName)
        {
            _attachmentFile = attachementFile;
            _attachmentName = attachmentName;
            return this;
        }

        public EmailBuilder AddRecepient(string recepient)
        {
            _recepient = recepient;
            return this;
        }

        public EmailBuilder AddSubject(string subject)
        {
            _subject = subject;
            return this;
        }

        public EmailBuilder AddBodyAsTemplate(string templateFile)
        {
            var body = TemplateReader.GetContentFromTemplate(templateFile);
            AddBodyAsText(body);
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
            _sparkPostOptions =  Options.Create(configuration.GetSection("SparkPost").Get<SparkPostOptions>());
            return this;
        }

        public async Task<EmailSendingResult> BuildAsync()
        {
            try
            {
                FailIfContentMissing();
                var sparkPostClient = new SparkPostClient(_sparkPostOptions);
                var transmission = new Transmission();
                transmission.Content.From.EMail = "noreply@rocketdocuments.com";
                transmission.Content.From.Name = PlaceholderWriter.GetWithPlaceholdersReplaced(_emailingSettings.SenderName,_placeholders);
                transmission.Content.Subject = PlaceholderWriter.GetWithPlaceholdersReplaced(_subject,_placeholders);
                transmission.Content.Html = PlaceholderWriter.GetWithPlaceholdersReplaced(_body,_placeholders);
                
                var recipient = new Recipient();
                recipient.Address.EMail = _recepient;
                transmission.Recipients.Add(recipient);

                AddAttachmentIfExists(transmission);
                

                await sparkPostClient.CreateTransmission(transmission);
                return new EmailSendingResult { Succeeded = true };
                
            }
            catch(Exception e)
            {
                throw e;
            }
            finally
            {
                CleanUp();
            }
        }


        private void FailIfContentMissing()
        {
            var contents = new Dictionary<string, string>
            {
                {"Subject", _subject },
                {"Body", _body },
                {"Recipient", _recepient }
            };
            foreach (var item in contents)
            {
                if(string.IsNullOrEmpty(item.Value))
                {
                    throw new Exception($"No '{item.Key}' was found in your email message");
                }
            }
        }

        private void AddAttachmentIfExists(Transmission transmission)
        {
            var hasAttachment = !string.IsNullOrEmpty(_attachmentFile);
            if (hasAttachment)
            {
                var attachment = new Attachment();
                var attachmentContent = TemplateReader.GetContentFromTemplate(_attachmentFile);
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