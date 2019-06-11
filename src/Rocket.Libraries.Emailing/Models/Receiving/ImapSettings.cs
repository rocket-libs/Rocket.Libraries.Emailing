using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using MailKit.Security;

namespace Rocket.Libraries.Emailing.Models.Receiving
{
    public class ImapSettings
    {
        public string Server { get; set; }

        public int Port { get; set; } = 993;

        public string User { get; set; }

        public string Password { get; set; }
    }
}