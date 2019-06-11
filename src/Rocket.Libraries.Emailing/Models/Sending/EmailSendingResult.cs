namespace Rocket.Libraries.Emailing.Models.Sending
{
    using System;

    public class EmailSendingResult
    {
        public bool Succeeded { get; set; }

        public Exception Exception { get; set; }
    }
}