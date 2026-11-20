using System;

namespace BDCS.Model.Patient
{
    public class SisoftLabResult
    {
        public string CptCode { get; set; }
        public string Param { get; set; }
        public string Result { get; set; }
        public string Unit { get; set; }
        public DateTime? ResultDate { get; set; }
    }
}
