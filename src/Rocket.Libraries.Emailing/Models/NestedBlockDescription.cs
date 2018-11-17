using System;
using System.Collections.Generic;
using System.Text;

namespace Rocket.Libraries.Emailing.Models
{
    public class NestedBlockDescription
    {
        public TagPair ParentTag { get; set; }

        public TagPair ChildTag { get; set; }
    }
}