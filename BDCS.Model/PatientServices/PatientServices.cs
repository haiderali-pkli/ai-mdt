using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.PatientServices
{
    public class PatientServices
    {
        public int ChargeServiceId { get; set; }
        public int ServiceId { get; set; }
        public int PatientId { get; set; }
        public int VisitId { get; set; }
        public double Quantity { get; set; }
        public double Amount { get; set; }
        public string ServiceName { get; set; }
        public string CptCode { get; set; }
        public double Price { get; set; }
    }
}
