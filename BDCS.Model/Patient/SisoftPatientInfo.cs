using System;

namespace BDCS.Model.Patient
{
    public class SisoftPatientInfo
    {
        public string HospitalNumber { get; set; }
        public string Cnic { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string PlaceOfBirth { get; set; }
        public string MobileNumber { get; set; }
        public string ContactNumber { get; set; }
        public string EmailId { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string BloodGroup { get; set; }
        public string PhotoDataUri { get; set; }
    }
}
