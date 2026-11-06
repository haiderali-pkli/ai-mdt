using BDCS.Data.Helpers;
using BDCS.Model.RoleRights;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BDCS.Data
{
    public class ClinicDAL
    {
        IDBManger _db;

        public ClinicDAL()
        {
            _db = new SqlDbManager();
        }

        public List<Clinic> GetClinicList()
        {
            List<Clinic> Data = new List<Clinic>();
            string Query = QueryHelper.GetQueryString("GetClinicList", "System");
            List<DbParameters> pars = new List<DbParameters>();
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Clinic
                    {
                        ClinicId = Convert.ToInt32(a["ClinicId"]),
                        ClinicName = a["ClinicName"].ToString(),
                        ContactPerson = a["ContactPerson"] == DBNull.Value ? null : a["ContactPerson"].ToString(),
                        ContactNumber = a["ContactNumber"] == DBNull.Value ? null : a["ContactNumber"].ToString(),
                        IsActive = a["IsActive"] != DBNull.Value && Convert.ToBoolean(a["IsActive"]),
                    }).ToList();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public Clinic GetClinicById(int clinicId)
        {
            Clinic Data = new Clinic();
            string Query = QueryHelper.GetQueryString("GetClinicById", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="ClinicId", Type=DbType.Int32, Value=clinicId }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Clinic
                    {
                        ClinicId = Convert.ToInt32(a["ClinicId"]),
                        ClinicName = a["ClinicName"].ToString(),
                        ContactPerson = a["ContactPerson"] == DBNull.Value ? null : a["ContactPerson"].ToString(),
                        ContactNumber = a["ContactNumber"] == DBNull.Value ? null : a["ContactNumber"].ToString(),
                        IsActive = a["IsActive"] != DBNull.Value && Convert.ToBoolean(a["IsActive"]),
                    }).ToList().FirstOrDefault();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public async Task<int> InsertUpdateClinic(Clinic clinic)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertUpdateClinic", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="ClinicId", Type=DbType.Int32, Value=clinic.ClinicId },
                new DbParameters() {Name="ClinicName", Type=DbType.String, Value=clinic.ClinicName },
                new DbParameters() {Name="ContactPerson", Type=DbType.String, Value=clinic.ContactPerson },
                new DbParameters() {Name="ContactNumber", Type=DbType.String, Value=clinic.ContactNumber },
                new DbParameters() {Name="IsActive", Type=DbType.Boolean, Value=clinic.IsActive },
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
    }
}
