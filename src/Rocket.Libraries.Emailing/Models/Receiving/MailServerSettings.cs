namespace Rocket.Libraries.Emailing.Models.Receiving.Imap
{
    public class MailServerSettings
    {
        public string Server { get; set; }

        public int Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public bool UseSsl { get; set; }
    }
}