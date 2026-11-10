using BDCS.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDCS.Model.Misc;
using System.Data;

namespace BDCS.Data
{
    public class CommonDAL
    {
        IDBManger _db;

        public CommonDAL()
        {
            _db = new SqlDbManager();
        }

        public async Task<List<KeyAndValuePair>> GetDefinationData(string procedureName, string schema, string parameters)
        {
            List<KeyAndValuePair> list = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>();
            //to gell all list we check parameters null then select all types of lookups
            if (procedureName == "GetCountryList")
            {
                pars.AddRange(new List<DbParameters>()
                {
                    new DbParameters() {Name="LookupType",Type=DbType.String, Value=parameters }
                });
            }
            if (!string.IsNullOrEmpty(parameters))
            {
                pars.AddRange(new List<DbParameters>()
                {
                    new DbParameters() {Name="LookupType",Type=DbType.String, Value=parameters }
                });
            }

            try
            {
                DataSet ds = await _db.LoadDataSetAsync(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair()
                    {
                        Id = a["Id"].ToString(),
                        Value = a["Value"].ToString(),
                        Description = a["Description"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return list;
        }

        public async Task<int> InsertLookupData(string LookupValue, string ReturnValue, string Category)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertLookUpValue", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="DisplayValue", Type=DbType.String,Value=LookupValue},
                new DbParameters() {Name="ReturnValue",Type=DbType.String,Value=ReturnValue },
                new DbParameters() {Name="LookupType",Type=DbType.String,Value=Category },

            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<List<KeyAndValuePair>> GetUserWiseVisitData(DateTime date)
        {
            List<KeyAndValuePair> list = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString("GetUserWiseVisitCount", "Clinical");
            List<DbParameters> pars = new List<DbParameters>();



            pars.AddRange(new List<DbParameters>()
                {
                    new DbParameters() {Name="Date",Type=DbType.Date, Value=date}
                });


            try
            {
                DataSet ds = await _db.LoadDataSetAsync(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair()
                    {
                        Id = a["UserFullName"].ToString(),
                        Value = a["VisitCount"].ToString()
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return list;
        }

        public List<KeyAndValuePair> GetMDTTypes(int userId)
        {
            List<KeyAndValuePair> list = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString("GetMDTTypes", "System");
            List<DbParameters> pars = new List<DbParameters>();

            pars.AddRange(new List<DbParameters>()
                {
                    new DbParameters() {Name="lookupType",Type=DbType.Date, Value="MDTType"},
                    new DbParameters() {Name="UserId",Type=DbType.Int32, Value=userId}
                });

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair()
                    {
                        Id = a["Id"].ToString(),
                        Value = a["Value"].ToString()
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return list;
        }

        public List<KeyAndValuePair> GetMDTTypesNameByUserId(int userId)
        {
            List<KeyAndValuePair> list = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString("GetMDTTypesNameByUserId", "System"); 
            List<DbParameters> pars = new List<DbParameters>();

            pars.AddRange(new List<DbParameters>()
                {
                    new DbParameters() {Name="UserId",Type=DbType.Int32, Value=userId}
                });

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair()
                    {
                        Id = a["Id"].ToString(),
                        Value = a["Value"].ToString()
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return list;
        }
        public async Task<List<KeyAndValuePair>> GetDiseaseStatusById(string procedureName, string schema, string parameters)
        {
            List<KeyAndValuePair> list = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>();
            
     
            if (!string.IsNullOrEmpty(parameters))
            {
                pars.AddRange(new List<DbParameters>()
                {
                    new DbParameters() {Name="lookupId",Type=DbType.Int32, Value=parameters }
                });
            }

            try
            {
                DataSet ds = await _db.LoadDataSetAsync(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair()
                    {
                        Id = a["Id"].ToString(),
                        Value = a["Value"].ToString(),
                        Description = a["Description"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return list;
        }

        public List<LookupItem> GetAllLookupsForAdmin()
        {
            List<LookupItem> list = new List<LookupItem>();
            string Query = QueryHelper.GetQueryString("GetAllLookupsForAdmin", "System");
            List<DbParameters> pars = new List<DbParameters>();
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new LookupItem()
                    {
                        LookupId = Convert.ToInt32(a["LookupId"]),
                        DisplayValue = a["DisplayValue"].ToString(),
                        ReturnValue = a["ReturnValue"] == DBNull.Value ? null : a["ReturnValue"].ToString(),
                        LookupType = a["LookupType"].ToString(),
                        IsActive = a["IsActive"] != DBNull.Value && Convert.ToBoolean(a["IsActive"]),
                    }).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return list;
        }

        public async Task<int> UpdateLookupValue(int lookupId, string displayValue, string returnValue, string lookupType)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("UpdateLookupValue", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LookupId", Type=DbType.Int32, Value=lookupId },
                new DbParameters() {Name="DisplayValue", Type=DbType.String, Value=displayValue },
                new DbParameters() {Name="ReturnValue", Type=DbType.String, Value=returnValue },
                new DbParameters() {Name="LookupType", Type=DbType.String, Value=lookupType },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public async Task<int> ToggleLookupActive(int lookupId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("ToggleLookupActive", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LookupId", Type=DbType.Int32, Value=lookupId },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception)
            {

                throw;
            }
            return result;
        }

        public List<KeyAndValuePair> GetDiagnosisList()
        {
            List<KeyAndValuePair> list = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString("GetDiagnosisList", "System");
            List<DbParameters> pars = new List<DbParameters>();


            pars.AddRange(new List<DbParameters>()
            {
                new DbParameters(){Name="DiagnosisType",Type =DbType.String, Value="DiagnosisType"}
            });

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    list = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair()
                    {
                        Id = a["Id"].ToString(),
                        Value = a["Value"].ToString(),
                        Description = a["Description"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return list;
        }

    }
}
