namespace BDCS.Model.Misc
{
    // Shared shape for System.LabTests and System.LabTestsBMT - the catalogs that
    // drive the default lab-entry grid on a visit (see Clinical.GetLabOrders /
    // GetLabOrdersBMT). TestGroup isn't exposed here - each screen's controller
    // action always targets the matching fixed-TestGroup catalog instead.
    public class LabTestDefinition
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public string TestUnit { get; set; }
        public int? Order { get; set; }
        public string CptCode { get; set; }
        public bool IsMandatory { get; set; }
    }
}
