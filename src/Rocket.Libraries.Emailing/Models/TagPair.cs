using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Emailing.Models
{
    public class TagPair
    {
        public string RawTag { get; set; }
        public string OpeningTag => $"<{RawTag}>";
        public string ClosingTag => $"</{RawTag}>";
    }
}