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
    }
}
