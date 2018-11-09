namespace Rocket.Libraries.Emailing.Models
{
    using System;

    public class EmailSendingResult
    {
        public bool Succeeded { get; set; }
        public Exception Exception { get; set; }
    }
}