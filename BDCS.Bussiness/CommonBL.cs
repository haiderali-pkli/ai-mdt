using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDCS.Data;
using BDCS.Model.Misc;

namespace BDCS.Bussiness
{
    public class CommonBL
    {
        CommonDAL _common;
        public CommonBL()
        {
            _common = new CommonDAL();
        }
        public async Task<List<KeyAndValuePair>> GetDefinationData(string procedureName, string schema, string paramters) => await _common.GetDefinationData(procedureName, schema, paramters);

        public async Task<int> InsertLookupData(string LookupValue, string ReturnValue, string Category) => await _common.InsertLookupData(LookupValue, ReturnValue, Category);


        public async Task<List<KeyAndValuePair>> GetUserWiseVisitData(DateTime date) => await _common.GetUserWiseVisitData(date);

        public List<KeyAndValuePair> GetMDTTypes(int userId)
            =>_common.GetMDTTypes(userId);
        public async Task<List<KeyAndValuePair>> GetDiseaseStatusById(string procedureName, string schema, string parameters)
            => await _common.GetDiseaseStatusById(procedureName, schema, parameters);

        public List<KeyAndValuePair> GetMDTTypesNameByUserId(int userId)
            => _common.GetMDTTypesNameByUserId(userId);
        public List<KeyAndValuePair> GetDiagnosisList()
            =>_common.GetDiagnosisList();
    }
}
