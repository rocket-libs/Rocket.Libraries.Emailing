namespace Rocket.Libraries.Emailing.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class NestedBlockDescription
    {
        public TagPair ParentTag { get; set; }

        public TagPair ChildTag { get; set; }
    }
}