using System.Collections.Generic;

namespace TesterApplication.Models
{
    internal class TelexRequestInformation
    {
        public List<Recepient> Recepients { get; set; }
        public List<Mbl> Mbls { get; set; }
    }
}