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
        public string ChildStatus { get; set; }
        public string MeldStatus { get; set; }
        public string BCLC { get; set; }
        public string ECOGStatus { get; set; }
        public string HepStatus { get; set; }
        public string AscitiesStatus { get; set; }
        public List<string> OtherComorbities { get; set; }
        public string CTReportConclusion { get; set; }
        public bool Angioinvasion { get; set; }
        public List<string> SiteOfAngioinvasionVB { get; set; }
        public bool ExtraHepaticDisease { get; set; }
        public string SiteOfHepaticDiseaseString { get; set; }
        //public string SiteOfHepaticDisease { get; set; }

        public List<string> SiteOfHepaticDisease { get; set; }

        public bool LiverCirrhosis { get; set; }
        public bool PortalHypertension { get; set; }
        public bool Varices { get; set; }
        public string NumberOfLesion { get; set; }
        public string LargestLesionSize { get; set; }
        public string SVRStatus { get; set; }
        public bool PreviousTreatment { get; set; }
        public List<string> TreatmentType { get; set; }
        public string TreatmentTypeString { get; set; }
        public string TreatmentResponse { get; set; }
 

        public string DiagnosisName { get; set; }
        public string DiagnosisDetails { get; set; }
        public bool MeetingMilan { get; set; }
        public bool UCSF { get; set; }
        public bool ASAN { get; set; }
        public bool other { get; set; }
        public string otherDetail { get; set; }
        public bool PtWillingForTransplant { get; set; }
        public string ReasonOfRefusal { get; set; }
        public string MDTDiscussion { get; set; }
        public List<string> MDTOutCome { get; set; }
        public int EnteredBy { get; set; }
        public DateTime EnteredOn { get; set; }
        public int ClinicId { get; set; }

        public string Temp { get; set; }
      
        public List<LabReports> PreviousLab { get; set; } = new List<LabReports>();
        public List<LabReports> labreports { get; set; } = new List<LabReports>();

        public List<Protocols> protocols { get; set; } = new List<Protocols>();
        public List<PatientDiagnosis> PatientDiagnosis { get; set; } = new List<PatientDiagnosis>();
        public string PatientName { get; set; }
        public string SiteOfAngioinvasion { get; set; }
        public string OtherComorbitiesString { get; set; }
        public string ChildStatusValue { get; set; }
        public string MeldStatusValue { get; set; }
        public string BCLCValue { get; set; }
        public string ECOGStatusValue { get; set; }
        public string TreatmentResponseValue { get; set; }
        public string HepStatusValue { get; set; }
        public string AscitiesStatusValue { get; set; }
        public string SVRStatusValue { get; set; }
        public string MDTOutComeString { get; set; }
        public string HospitalNumber { get; set; }
        public string NumberOfLesionValue { get; set; }
        public string MDTOutComeOther { get; set; }
        public bool TransplantDiscussion { get; set; }
        public string TransplantDiscussionDetail { get; set; }
        public DateTime? ImagingDate { get; set; }
    }
}
