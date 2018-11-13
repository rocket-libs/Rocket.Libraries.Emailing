using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furaha.Services.Logic.Legacy.Models.Reporting
{
    public class GroupedUninvoicedLoaded
    {
        public string Group { get; set; }
        public List<UninvoicedLoaded> Rows { get; set; }
    }
}