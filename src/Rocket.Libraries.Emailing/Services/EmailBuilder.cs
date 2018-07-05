using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Rocket.Libraries.Emailing.Models;
using SparkPostDotNet;
using SparkPostDotNet.Transmissions;
using Options = Microsoft.Extensions.Options.Options;

namespace Rocket.Libraries.Emailing.Services
{
    internal class EmailBuilder
    {
        private string _recepient;
        private string _subject;
        private string _body;
        private string _attachment;
        private string _attachmentName;

        
        private EmailingSettings _emailingSettings;
        private IOptions<SparkPostOptions> _sparkPostOptions;

        public EmailingSettings EmailingSettings
        {
            get
            {
                return _emailingSettings;
            }
        }

        public EmailBuilder()
        {
            CleanUp();
        }

        private void CleanUp()
        {
            AddBody(string.Empty)
                .AddAttachment(string.Empty, string.Empty)
                .AddRecepient(string.Empty)
                .AddSubject(string.Empty);
        }

        
        public EmailBuilder AddAttachment(string html, string attachmentName)
        {
            _attachment = html;
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

        public EmailBuilder AddBody(string body)
        {
            _body = body;
            return this;
        }

        public EmailBuilder SetConfiguration(IConfiguration configuration)
        {
            _emailingSettings = configuration.GetSection("Emailing").Get<EmailingSettings>();
            _sparkPostOptions =  Options.Create(configuration.GetSection("SparkPost").Get<SparkPostOptions>());
            return this;
        }

        public async Task<EmailSendingResult> BuildAsync()
        {
            try
            {
                var sparkPostClient = new SparkPostClient(_sparkPostOptions);
                var transmission = new Transmission();
                transmission.Content.From.EMail = "noreply@rocketdocuments.com";
                transmission.Content.From.Name = _emailingSettings.SenderName;
                transmission.Content.Subject = _subject;
                transmission.Content.Html = _body;
                
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


        private void AddAttachmentIfExists(Transmission transmission)
        {
            var hasAttachment = !string.IsNullOrEmpty(_attachment);
            if (hasAttachment)
            {
                LoadLib();
                var attachment = new Attachment();
                attachment.Data = GetPdfBytes();
                attachment.Name = $"{_attachmentName}.pdf";
                attachment.Type = "application/pdf";
                transmission.Content.Attachments.Add(attachment);
            }
            else
            {
                return;
            }

        }

        private byte[] GetPdfBytes()
        {
            var doc = new HtmlToPdfDocument
            {
                 GlobalSettings =
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                },
                Objects =
                {
                    new ObjectSettings()
                    {
                        PagesCount = true,
                        HtmlContent = _attachment,
                        WebSettings = { DefaultEncoding = "utf-8" },
                        FooterSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 },
                        
                    }
                }
            };

            var converter = new BasicConverter(new PdfTools());
            return converter.Convert(doc);
        }


        private void LoadLib()
        {
            var architectureFolder = (IntPtr.Size == 8) ? "64bit" : "32bit";
            var wkHtmlToPdfPath = Path.Combine(AppContext.BaseDirectory, $"libs/{architectureFolder}/");
            foreach (var file in Directory.GetFiles(wkHtmlToPdfPath))
            {
                var targetFile = $"{AppContext.BaseDirectory}/{Path.GetFileName(file)}";
                if (!File.Exists(targetFile))
                {
                    File.Copy(file, targetFile);
                }
            }
            /*var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(wkHtmlToPdfPath);*/
        }

    }
}