using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.VitalHistory
{
    public class VitalHistoryBMT
    {
        public int PatientDetailId { get; set; }
        public int PatientId { get; set; }
        public int VisitId { get; set; }
        public DateTime? VisitDateTime { get; set; }
        public string VisitType { get; set; }
        public string DiagnosisName { get; set; }
        public string DiagnosisDetails { get; set; }
        public string CIC { get; set; }
        public string DiseaseStatus { get; set; }
        public string Lansky { get; set; }
        public string HCTCI { get; set; }
        public string HepBStatus { get; set; }
        public string HepCStatus { get; set; }
        public string HIVStatus { get; set; }
        public string ECOGStatus { get; set; }
        public List<string> OtherComorbitiesBMT { get; set; }
        public string Summary { get; set; }
        public string Stage { get; set; }
        public string RiskScore { get; set; }
        public string Biopsy { get; set; }
        public string PETCT { get; set; }
        public string Chelation { get; set; }
        public string Liver { get; set; }
        public string Spleen { get; set; }
        public bool Lymphonodes { get; set; }
        public List<string> SiteOfLymphnodes { get; set; }
        public string SiteOfLymphNodesVB { get; set; }
        //public List<string> SiteOfLymphNodesVB { get; set; }
        public string LiverBiopsy { get; set; }
        public bool PreviousTreatment { get; set; }
        public List<string> PreviousTreatmentBMT { get; set; }
        public string PreviousTreatmentBMTSring { get; set; }
        public string TreatmentType { get; set; }
        public string BMTTreatmentType { get; set; }
        public string TreatmentResponse { get; set; }
        public string TreatmentResponseValue { get; set; }
        public bool PtWillingForTransplant { get; set; }
        public string ReasonOfRefusal { get; set; }
        public string MDTDiscussion { get; set; }
        public List<string> MDTOutCome { get; set; }

        public string MDTOutComeOnlyText { get; set; }


        public int EnteredBy { get; set; }
        public DateTime EnteredOn { get; set; }
        public int ClinicId { get; set; }

        public string DiseaseStatusValReport { get; set; }
        public string LanskyValReport { get; set; }
        public string HCTCIValReport { get; set; }
        public string ECOGStatusValReport { get; set; }
        public string HepBStatusValReport { get; set; }
        public string HepCStatusValReport { get; set; }
        public string HIVStatusValReport { get; set; }

      
        //public string TreatmentTypeString { get; set; }
        //public string TreatmentResponse { get; set; }



        //public bool MeetingMilan { get; set; }
        //public bool UCSF { get; set; }
        //public bool ASAN { get; set; }
        //public bool other { get; set; }
        //public string otherDetail { get; set; }


        //public string Temp { get; set; }

        //public List<LabReports> PreviousLab { get; set; } = new List<LabReports>();
        public List<LabReportsBMT> labreports { get; set; } = new List<LabReportsBMT>();

        //public List<Protocols> protocols { get; set; } = new List<Protocols>();
        public List<PatientDiagnosisBMT> PatientDiagnosis { get; set; } = new List<PatientDiagnosisBMT>();
        //public string PatientName { get; set; }
        //public string SiteOfAngioinvasion { get; set; }
        public string OtherComorbitiesString { get; set; }
public string SiteOfLymphnodesString { get; set; }
        
        //public string ChildStatusValue { get; set; }
        //public string MeldStatusValue { get; set; }
        //public string BCLCValue { get; set; }
        //public string ECOGStatusValue { get; set; }
        //public string TreatmentResponseValue { get; set; }
        //public string HepStatusValue { get; set; }
        //public string AscitiesStatusValue { get; set; }
        //public string SVRStatusValue { get; set; }
        public string MDTOutComeString { get; set; }
        //public string HospitalNumber { get; set; }
        //public string NumberOfLesionValue { get; set; }
        public string MDTOutComeOther { get; set; }
        public bool TransplantDiscussion { get; set; }
        public string TransplantDiscussionDetail { get; set; }
        public DateTime? MDTDate { get; set; }

        public string PatientName { get; set; }

        public string HospitalNumber { get; set; }
        

    }
}
