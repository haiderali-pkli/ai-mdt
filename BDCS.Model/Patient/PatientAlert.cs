using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.Patient
{
    public class PatientAlert
    {
        public int PatientAlertId { get; set; }
        public int PatientId { get; set; }
        public string AlertTitle { get; set; }
        public string AlertDescription { get; set; }
        public DateTime? ActiveFrom { get; set; }
        public DateTime? ActiveTill { get; set; }
        public bool IsActive { get; set; }
        public int EnteredBy { get; set; }
        public DateTime EnteredOn { get; set; }
        
    }
}
