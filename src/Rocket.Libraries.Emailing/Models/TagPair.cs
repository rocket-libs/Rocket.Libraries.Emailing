﻿namespace Rocket.Libraries.Emailing.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class TagPair
    {
        public TagPair(string prefix, string rawTag)
        {
            Prefix = prefix;
            RawTag = rawTag;
        }

        public string OpeningTag => $"<{RawTag}>";

        public string ClosingTag => $"</{RawTag}>";

        public string UnPrefixedTag => RawTag.Substring(Prefix.Length);

        public string Prefix { get; }

        public string RawTag { get; }
    }
}