    using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.Patient
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get { return FirstName + " " + LastName; } }
        public string MrNumber { get; set; }
        public string HospitalNumber { get; set; }
        public string Relationship { get; set; }
        public string FatherSpouseName { get; set; }
        public int GenderId { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public double Age { get; set; }
        public string AgeYearMonth { get { return CalculateYourAge(DOB.HasValue ? DOB.Value : DateTime.MinValue); } }
        public string AgeUnit { get; set; }
        public int BloodGroupId { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public int ProvinceId { get; set; }
        public string Province { get; set; }
        public int DistrictId { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string EmailId { get; set; }
        public string Ethnicity { get; set; }
        public string MobileNumber { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string ReferredBy { get; set; }
        public int DiagnosisId { get; set; }
        public bool IsDiabeties { get; set; }
        public string Comments { get; set; }
        public string ImageURL { get; set; }
        public bool IsActive { get; set; }
        public int EnteredBy { get; set; }
        public DateTime? EnteredOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int VisitId { get; set; }
        public string Reason { get; set; }
        public DateTime? VisitDateTime { get; set; }

        public string CameraPicture { get; set; }
        public string DiagnosisName { get; set; }

        //ViewModel
        public int ClinicId { get; set; }

        public string Rel { get; set; }
        public bool DiscussionRequired { get; set; }
        public string BloodGroupName { get; set; }

        string CalculateYourAge(DateTime Dob)
        {
            if (Dob == null || Dob.Date == DateTime.MinValue.Date)
            {
                return "";
            }
            DateTime Now = DateTime.Now;
            int Years = new DateTime(DateTime.Now.Subtract(Dob).Ticks).Year - 1;
            DateTime PastYearDate = Dob.AddYears(Years);
            int Months = 0;
            for (int i = 1; i <= 12; i++)
            {
                if (PastYearDate.AddMonths(i) == Now)
                {
                    Months = i;
                    break;
                }
                else if (PastYearDate.AddMonths(i) >= Now)
                {
                    Months = i - 1;
                    break;
                }
            }
            int Days = Now.Subtract(PastYearDate.AddMonths(Months)).Days;
            int Hours = Now.Subtract(PastYearDate).Hours;
            int Minutes = Now.Subtract(PastYearDate).Minutes;
            int Seconds = Now.Subtract(PastYearDate).Seconds;
            return String.Format("Age: {0} Year(s) {1} Month(s) {2} Day(s)",
            Years, Months, Days);
        }
    }
}
