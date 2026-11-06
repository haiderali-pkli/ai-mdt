using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.Visit
{
    public class Visit
    {
        public int VisitId { get; set; }
        public int PatientId { get; set; }
        public DateTime? VisitDate { get; set; }
        public string Reason { get; set; }
        public int ClinicId { get; set; }
        //View Model
        public string PatientName { get; set; }
        public string MDTType { get; set; }

    }
}
