using System.Collections.Generic;
using System.Threading.Tasks;
using BDCS.Data;
using BDCS.Model.Misc;

namespace BDCS.Bussiness
{
    public class ReportConsultantBL
    {
        readonly ReportConsultantDAL _reportConsultantDAL;

        public ReportConsultantBL()
        {
            _reportConsultantDAL = new ReportConsultantDAL();
        }

        public List<ReportConsultant> GetReportConsultants() => _reportConsultantDAL.GetReportConsultants();
        public ReportConsultant GetReportConsultantById(int id) => _reportConsultantDAL.GetReportConsultantById(id);
        public async Task<int> InsertUpdateReportConsultant(ReportConsultant consultant) => await _reportConsultantDAL.InsertUpdateReportConsultant(consultant);
        public async Task<int> ToggleReportConsultantActive(int id) => await _reportConsultantDAL.ToggleReportConsultantActive(id);
        public async Task<int> DeleteReportConsultant(int id) => await _reportConsultantDAL.DeleteReportConsultant(id);
    }
}
