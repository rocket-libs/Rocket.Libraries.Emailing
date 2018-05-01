using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Emailing.Models
{
    public class EmailingSettings
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public SecureSocketOptions SecureSocketOptions { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string TemplatesDirectory { get; set; }
        public string SenderName { get; set; }
    }
}
