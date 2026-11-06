using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.VitalHistory
{
    public class LabReportsBMT
    {
        public int LabReportIdBMT { get; set; }
        public int PreviousReportId { get; set; }
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public int TestId { get; set; }
        public string TestName { get; set; }
        public DateTime? TestDate { get; set; }
        public DateTime? VisitDateTime { get; set; }
        public string TestResult { get; set; }
        public string TestUnit { get; set; }
        public string TestResultLabel { get; set; }
    }
}
