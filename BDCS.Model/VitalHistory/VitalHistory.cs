using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.VitalHistory
{
    public class VitalHistory
    {
        public int PatientDetailId { get; set; }
        public int PatientId { get; set; }
        public int VisitId { get; set; }
        public DateTime? VisitDateTime { get; set; }
        public string VisitType { get; set; }
        public string CIC { get; set; }
        public string PastHxNAT { get; set; }
        public string PresentHx { get; set; }
        public string PastHx { get; set; }
        public string FamilyHx { get; set; }
        public string GynaeObsHx { get; set; }
        public string SecHx { get; set; }
        public string DrugHx { get; set; }
        public string TransfusionHx { get; set; }
        public string Comorbidites { get; set; }
        public string SystemicCVS { get; set; }
        public string SystemicRes { get; set; }
        public string SystemicLiver { get; set; }
        public string SystemicSpleen { get; set; }
        public string Weight { get; set; }
        public string WeightUnit { get; set; }
        public string WeightUnitName { get; set; }
        public string Height { get; set; }
        public string HeightUnit { get; set; }
        public string HeightUnitName { get; set; }
        public string Temp { get; set; }
        public bool GPEJ { get; set; }
        public bool GPEA { get; set; }
        public bool GPEL { get; set; }
        public bool GPEC { get; set; }
        public bool GPEK { get; set; }
        public bool GPEE { get; set; }
        public bool GPET { get; set; }
        public string GPEComments { get; set; }
        public string BPSystolic { get; set; }
        public string BPDistolic { get; set; }
        public string NextFollowup { get; set; }
        public DateTime? NextFollowupDate { get; set; }
        public int EnteredBy { get; set; }
        public DateTime? EnteredOn { get; set; }
        public int ClinicId { get; set; }

        public string DiagnosisName { get; set; }
        public string DiagnosisDetails { get; set; }

        public List<LabReports> PreviousLab { get; set; } = new List<LabReports>();
        public List<LabReports> labreports { get; set; } = new List<LabReports>();

        public List<Protocols> protocols { get; set; } = new List<Protocols>();
        public List<PatientDiagnosis> PatientDiagnosis { get; set; } = new List<PatientDiagnosis>();


        public string ChildStatus { get; set; }
        public string MeldStatus { get; set; }
        public string BCLC { get; set; }
        public string ECOGStatus { get; set; }
        public string HepStatus { get; set; }
        public string SVRStatus { get; set; }
        public string AscitiesStatus { get; set; }
        public bool LiverCirrhosis { get; set; }

        public bool PortalHypertension { get; set; }
        public List<string> OtherComorbities { get; set; }
        public string LargestLesionSize { get; set; }
        public string NumberOfLesion { get; set; }
        public bool Angioinvasion { get; set; }
        public string SiteOfAngioinvasionVB { get; set; }
        public bool ExtraHepaticDisease { get; set; }
        public string SiteOfHepaticDisease { get; set; }
        public bool PreviousTreatment { get; set; }
        public string MDTOutCome { get; set; }
        public string TreatmentType { get; set; }
        public string TreatmentResponse { get; set; }
        public bool MeetingMilan { get; set; }
        public bool PtWillingForTransplant { get; set; }
        public string ReasonOfRefusal { get; set; }
        public string MDTDiscussion { get; set; }
        public bool Varices { get; set; }
    }
}
