using BDCS.Data.Helpers;
using BDCS.Model.Misc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BDCS.Data
{
    public class LabTestDAL
    {
        IDBManger _db;

        public LabTestDAL()
        {
            _db = new SqlDbManager();
        }

        List<LabTestDefinition> MapRows(DataSet ds)
        {
            List<LabTestDefinition> Data = new List<LabTestDefinition>();
            if (ds?.Tables.Count > 0)
            {
                Data = ds.Tables[0].AsEnumerable().Select(a => new LabTestDefinition
                {
                    TestId = Convert.ToInt32(a["TestId"]),
                    TestName = a["TestName"] == DBNull.Value ? "" : a["TestName"].ToString(),
                    TestUnit = a["TestUnit"] == DBNull.Value ? "" : a["TestUnit"].ToString(),
                    Order = a["Order"] == DBNull.Value ? (int?)null : Convert.ToInt32(a["Order"]),
                }).ToList();
            }
            return Data;
        }

        public List<LabTestDefinition> GetLabTestList()
        {
            DataSet ds = _db.LoadDataSet(QueryHelper.GetQueryString("GetLabTests", "System"), CommandType.StoredProcedure, new List<DbParameters>());
            return MapRows(ds);
        }

        public LabTestDefinition GetLabTestById(int testId)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "TestId", Type = DbType.Int32, Value = testId } };
            DataSet ds = _db.LoadDataSet(QueryHelper.GetQueryString("GetLabTestById", "System"), CommandType.StoredProcedure, pars);
            return MapRows(ds).FirstOrDefault();
        }

        public async Task<int> InsertUpdateLabTest(LabTestDefinition test)
        {
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "TestId", Type = DbType.Int32, Value = test.TestId },
                new DbParameters() { Name = "TestName", Type = DbType.String, Value = test.TestName },
                new DbParameters() { Name = "TestUnit", Type = DbType.String, Value = (object)test.TestUnit ?? DBNull.Value },
                new DbParameters() { Name = "Order", Type = DbType.Int32, Value = (object)test.Order ?? DBNull.Value },
            };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("InsertUpdateLabTest", "System"), CommandType.StoredProcedure, pars));
        }

        public async Task<int> DeleteLabTest(int testId)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "TestId", Type = DbType.Int32, Value = testId } };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("DeleteLabTest", "System"), CommandType.StoredProcedure, pars));
        }

        public List<LabTestDefinition> GetLabTestListBMT()
        {
            DataSet ds = _db.LoadDataSet(QueryHelper.GetQueryString("GetLabTestsBMT", "System"), CommandType.StoredProcedure, new List<DbParameters>());
            return MapRows(ds);
        }

        public LabTestDefinition GetLabTestByIdBMT(int testId)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "TestId", Type = DbType.Int32, Value = testId } };
            DataSet ds = _db.LoadDataSet(QueryHelper.GetQueryString("GetLabTestByIdBMT", "System"), CommandType.StoredProcedure, pars);
            return MapRows(ds).FirstOrDefault();
        }

        public async Task<int> InsertUpdateLabTestBMT(LabTestDefinition test)
        {
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "TestId", Type = DbType.Int32, Value = test.TestId },
                new DbParameters() { Name = "TestName", Type = DbType.String, Value = test.TestName },
                new DbParameters() { Name = "TestUnit", Type = DbType.String, Value = (object)test.TestUnit ?? DBNull.Value },
                new DbParameters() { Name = "Order", Type = DbType.Int32, Value = (object)test.Order ?? DBNull.Value },
            };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("InsertUpdateLabTestBMT", "System"), CommandType.StoredProcedure, pars));
        }

        public async Task<int> DeleteLabTestBMT(int testId)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "TestId", Type = DbType.Int32, Value = testId } };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("DeleteLabTestBMT", "System"), CommandType.StoredProcedure, pars));
        }
    }
}
