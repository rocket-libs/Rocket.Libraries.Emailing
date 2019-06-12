namespace Rocket.Libraries.Emailing.Models.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using MailKit.Security;

    public class ImapSettings
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public bool UseSsl { get; set; }
    }
}