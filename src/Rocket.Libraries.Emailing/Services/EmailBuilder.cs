using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DinkToPdf;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using Rocket.Libraries.Emailing.Models;

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
            return this;
        }

        public async Task<EmailSendingResult> BuildAsync()
        {
            try
            {

                var emailMessage = new MimeMessage();
    
                emailMessage.From.Add(new MailboxAddress(_emailingSettings.SenderName, _emailingSettings.User));
                emailMessage.To.Add(new MailboxAddress("", _recepient));
                emailMessage.Subject = _subject;



                AddMultipart(emailMessage);

                using (var client = new SmtpClient())
                {
                    var secureSocketOptions = (SecureSocketOptions)_emailingSettings.SecureSocketOptions;
                    await client.ConnectAsync(_emailingSettings.Server, _emailingSettings.Port, secureSocketOptions);
                    await client.AuthenticateAsync(_emailingSettings.User, _emailingSettings.Password);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }

                return new EmailSendingResult { Succeeded = true };
                
            }
            catch(Exception e)
            {
                return new EmailSendingResult 
                {
                    Succeeded = false,
                    Exception = e
                };
            }
            finally
            {
                CleanUp();
            }
        }


        private void AddMultipart(MimeMessage emailMessage)
        {
            var byteArray = Encoding.UTF8.GetBytes(_attachment);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            using (var stream = new MemoryStream(GetPdfStream()))
            {

                var attachment = new MimePart("document", "pdf")
                {
                    Content = new MimeContent(stream),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = $"{_attachmentName}.pdf"
                };


                var body = new TextPart(TextFormat.Html) { Text = _body };
                var multipart = new Multipart("mixed");
                multipart.Add(body);
                multipart.Add(attachment);
                emailMessage.Body = multipart;
            }

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
                        HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                    }
                }
            };

            var converter = new SynchronizedConverter(new PdfTools());
            return converter.Convert(doc);
        }

    }
}