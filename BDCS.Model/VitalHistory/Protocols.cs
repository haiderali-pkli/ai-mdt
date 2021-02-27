using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.VitalHistory
{
    public class Protocols
    {
        public int VisitId { get; set; }
        public int ProtocolDetailId { get; set; }
        public int PatientId { get; set; }
        public int ProtocolId { get; set; }
        public string MedicineType { get; set; }
        public string ProtocoName { get; set; }
        public string Medicine { get; set; }
        public string Dose { get; set; }
        public string DoseUnit { get; set; }
        public string Route { get; set; }
        public string Frequency { get; set; }
        public string Duration { get; set; }
        public bool IsFavorite { get; set; }

        public bool IsDiscontinued { get; set; }

        public int EnteredBy { get; set; }
    }
}
