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
                emailMessage.Body = new TextPart("html") { Text = _body };
            
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

    }
}