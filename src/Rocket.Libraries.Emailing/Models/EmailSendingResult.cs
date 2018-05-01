using System;

namespace Rocket.Libraries.Emailing.Models
{
    public class EmailSendingResult
    {
        public bool Succeeded {get; set;}
        public Exception Exception {get; set;}
    }
}