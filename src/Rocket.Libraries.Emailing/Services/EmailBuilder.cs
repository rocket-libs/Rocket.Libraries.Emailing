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
                transmission.Content.From.EMail = _emailingSettings.SenderName;
                transmission.Content.From.Name = _emailingSettings.User;
                transmission.Content.Subject = _subject;
                transmission.Content.Html = _body;
                var recipient = new Recipient();
                recipient.Address.EMail = _recepient;
                transmission.Recipients.Add(recipient);
                
                await sparkPostClient.CreateTransmission(transmission);
                return new EmailSendingResult { Succeeded = true };

                /*var emailMessage = new MimeMessage();
    
                emailMessage.From.Add(new MailboxAddress(_emailingSettings.SenderName, _emailingSettings.User));
                emailMessage.To.Add(new MailboxAddress("", _recepient));
                emailMessage.Subject = _subject;

                LoadLib();

                using (var stream = new MemoryStream(GetPdfStream()))
                {
                    AddMultipart(emailMessage, stream);

                    using (var client = new SmtpClient())
                    {
                        var secureSocketOptions = (SecureSocketOptions)_emailingSettings.SecureSocketOptions;
                        client.Connect(_emailingSettings.Server, _emailingSettings.Port, secureSocketOptions);
                        client.Authenticate(_emailingSettings.User, _emailingSettings.Password);
                        client.Send(emailMessage);
                        client.Disconnect(true);
                    }

                    return new EmailSendingResult { Succeeded = true };
                }*/
                
            }
            finally
            {
                CleanUp();
            }
        }


        private void AddMultipart(MimeMessage emailMessage, MemoryStream stream)
        {
            var byteArray = Encoding.UTF8.GetBytes(_attachment);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            
            var attachment = new MimePart("document", "pdf")
            {
                Content = new MimeContent(stream),
                ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                ContentTransferEncoding = ContentEncoding.Base64,
                FileName = $"{_attachmentName}.pdf"
            };


            var body = new TextPart(TextFormat.Html) { Text = _body };
            var multipart = new Multipart("mixed")
            {
                body,
                attachment
            };
            emailMessage.Body = multipart;
        }

        private byte[] GetPdfStream()
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