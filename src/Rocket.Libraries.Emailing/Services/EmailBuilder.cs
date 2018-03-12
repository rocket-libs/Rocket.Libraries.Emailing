using System;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Rocket.Libraries.Emailing.Models;

namespace Rocket.Libraries.Emailing.Services
{
    internal class EmailBuilder
    {
        private string _recepient;
        private string _subject;
        private string _body;

        private string _senderName;
        private string _sendEmail;
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
            .AddSubject(string.Empty)
            .AddSender(string.Empty,string.Empty);
        }

        

        public EmailBuilder AddSender(string emailAddress,string name = null)
        {
            _sendEmail = emailAddress;
            _senderName = !string.IsNullOrEmpty(name) ? name : emailAddress;
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
    
                emailMessage.From.Add(new MailboxAddress(_senderName, _sendEmail));
                emailMessage.To.Add(new MailboxAddress("", _recepient));
                emailMessage.Subject = _subject;
                emailMessage.Body = new TextPart("html") { Text = _body };
            
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls).ConfigureAwait(false);
                    await client.AuthenticateAsync("rocket.documents1@gmail.com", "pentium1.2");
                    await client.SendAsync(emailMessage).ConfigureAwait(false);
                    await client.DisconnectAsync(true).ConfigureAwait(false);
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

    }
}