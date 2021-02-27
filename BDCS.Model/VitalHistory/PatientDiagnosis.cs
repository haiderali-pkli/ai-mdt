using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.VitalHistory
{
    public class PatientDiagnosis
    {
        public int PatientDiagnosisId { get; set; }
        public int VisitId { get; set; }
        public string Diagnosis { get; set; }
        public bool IsActive { get; set; }
        public DateTime InactiveDate { get; set; }
        public int EnteredBy { get; set; }
        public DateTime EnteredOn { get; set; }


    }
    
}
