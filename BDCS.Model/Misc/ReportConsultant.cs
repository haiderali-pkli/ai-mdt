namespace BDCS.Model.Misc
{
    // Backs System.ReportConsultant - the consultant entries printed in the footer of
    // both the Liver MDT (Patient.rdlc) and Bone Marrow MDT (PatientMaster.rdlc) visit
    // reports. One shared list feeds both reports' footers (see
    // PatientController.BuildConsultantFooterParameters) - department is free text, and
    // consultants sharing the same Department within the same ColumnNumber are grouped
    // under one heading in print. ColumnNumber (1-4) is which of the 4 fixed footer boxes
    // this entry prints in - the admin picks it explicitly, there's no auto-assignment.
    // DisplayOrder is this entry's row position within that column (ascending, nulls last).
    public class ReportConsultant
    {
        public int ReportConsultantId { get; set; }
        public string Department { get; set; }
        public string ConsultantName { get; set; }
        public string Designation { get; set; }
        public string Qualifications { get; set; }
        public int ColumnNumber { get; set; }
        public int? DisplayOrder { get; set; }
        public bool IsActive { get; set; }
        public bool BoldDepartment { get; set; }
        public bool BoldConsultantName { get; set; }
        public bool BoldDesignation { get; set; }
        public bool BoldQualifications { get; set; }
    }
}
