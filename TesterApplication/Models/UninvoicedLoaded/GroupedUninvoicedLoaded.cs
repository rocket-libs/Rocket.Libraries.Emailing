using System.Collections.Generic;

namespace Furaha.Services.Logic.Legacy.Models.Reporting
{
    public class GroupedUninvoicedLoaded
    {
        public string Group { get; set; }
        public List<UninvoicedLoaded> Rows { get; set; }
    }
}