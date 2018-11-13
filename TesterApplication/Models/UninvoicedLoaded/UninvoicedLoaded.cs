using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Furaha.Services.Logic.Legacy.Models.Reporting
{
    public class UninvoicedLoaded
    {
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string RegNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Status { get; set; }
        public string Vessel { get; set; }
        public string PortOfLoading { get; set; }
        public string DischargePort { get; set; }
        public DateTime Ets { get; set; }
        public string BookingNumber { get; set; }
        public DateTime ClosingDate { get; set; }

        public string Customer => string.IsNullOrEmpty(CompanyName) ? $"{FirstName} {LastName}" : CompanyName;
    }
}