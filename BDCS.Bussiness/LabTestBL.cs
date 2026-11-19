using System.Collections.Generic;
using System.Threading.Tasks;
using BDCS.Data;
using BDCS.Model.Misc;

namespace BDCS.Bussiness
{
    public class LabTestBL
    {
        readonly LabTestDAL _labTestDAL;

        public LabTestBL()
        {
            _labTestDAL = new LabTestDAL();
        }

        public List<LabTestDefinition> GetLabTestList() => _labTestDAL.GetLabTestList();
        public LabTestDefinition GetLabTestById(int testId) => _labTestDAL.GetLabTestById(testId);
        public async Task<int> InsertUpdateLabTest(LabTestDefinition test) => await _labTestDAL.InsertUpdateLabTest(test);
        public async Task<int> DeleteLabTest(int testId) => await _labTestDAL.DeleteLabTest(testId);

        public List<LabTestDefinition> GetLabTestListBMT() => _labTestDAL.GetLabTestListBMT();
        public LabTestDefinition GetLabTestByIdBMT(int testId) => _labTestDAL.GetLabTestByIdBMT(testId);
        public async Task<int> InsertUpdateLabTestBMT(LabTestDefinition test) => await _labTestDAL.InsertUpdateLabTestBMT(test);
        public async Task<int> DeleteLabTestBMT(int testId) => await _labTestDAL.DeleteLabTestBMT(testId);
    }
}
