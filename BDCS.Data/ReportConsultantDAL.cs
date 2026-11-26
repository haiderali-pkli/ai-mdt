using BDCS.Data.Helpers;
using BDCS.Model.Misc;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BDCS.Data
{
    public class ReportConsultantDAL
    {
        IDBManger _db;

        public ReportConsultantDAL()
        {
            _db = new SqlDbManager();
        }

        List<ReportConsultant> MapRows(DataSet ds)
        {
            List<ReportConsultant> Data = new List<ReportConsultant>();
            if (ds?.Tables.Count > 0)
            {
                Data = ds.Tables[0].AsEnumerable().Select(a => new ReportConsultant
                {
                    ReportConsultantId = Convert.ToInt32(a["ReportConsultantId"]),
                    Department = a["Department"] == DBNull.Value ? "" : a["Department"].ToString(),
                    ConsultantName = a["ConsultantName"] == DBNull.Value ? "" : a["ConsultantName"].ToString(),
                    Designation = a["Designation"] == DBNull.Value ? "" : a["Designation"].ToString(),
                    Qualifications = a["Qualifications"] == DBNull.Value ? "" : a["Qualifications"].ToString(),
                    ColumnNumber = a["ColumnNumber"] == DBNull.Value ? 1 : Convert.ToInt32(a["ColumnNumber"]),
                    DisplayOrder = a["DisplayOrder"] == DBNull.Value ? (int?)null : Convert.ToInt32(a["DisplayOrder"]),
                    IsActive = a["IsActive"] != DBNull.Value && Convert.ToBoolean(a["IsActive"]),
                    BoldDepartment = a["BoldDepartment"] != DBNull.Value && Convert.ToBoolean(a["BoldDepartment"]),
                    BoldConsultantName = a["BoldConsultantName"] != DBNull.Value && Convert.ToBoolean(a["BoldConsultantName"]),
                    BoldDesignation = a["BoldDesignation"] != DBNull.Value && Convert.ToBoolean(a["BoldDesignation"]),
                    BoldQualifications = a["BoldQualifications"] != DBNull.Value && Convert.ToBoolean(a["BoldQualifications"]),
                }).ToList();
            }
            return Data;
        }

        public List<ReportConsultant> GetReportConsultants()
        {
            DataSet ds = _db.LoadDataSet(QueryHelper.GetQueryString("GetReportConsultants", "System"), CommandType.StoredProcedure, new List<DbParameters>());
            return MapRows(ds);
        }

        public ReportConsultant GetReportConsultantById(int id)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "ReportConsultantId", Type = DbType.Int32, Value = id } };
            DataSet ds = _db.LoadDataSet(QueryHelper.GetQueryString("GetReportConsultantById", "System"), CommandType.StoredProcedure, pars);
            return MapRows(ds).FirstOrDefault();
        }

        public async Task<int> InsertUpdateReportConsultant(ReportConsultant consultant)
        {
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "ReportConsultantId", Type = DbType.Int32, Value = consultant.ReportConsultantId },
                new DbParameters() { Name = "Department", Type = DbType.String, Value = consultant.Department },
                new DbParameters() { Name = "ConsultantName", Type = DbType.String, Value = consultant.ConsultantName },
                new DbParameters() { Name = "Designation", Type = DbType.String, Value = (object)consultant.Designation ?? DBNull.Value },
                new DbParameters() { Name = "Qualifications", Type = DbType.String, Value = (object)consultant.Qualifications ?? DBNull.Value },
                new DbParameters() { Name = "ColumnNumber", Type = DbType.Int32, Value = consultant.ColumnNumber },
                new DbParameters() { Name = "DisplayOrder", Type = DbType.Int32, Value = (object)consultant.DisplayOrder ?? DBNull.Value },
                new DbParameters() { Name = "IsActive", Type = DbType.Boolean, Value = consultant.IsActive },
                new DbParameters() { Name = "BoldDepartment", Type = DbType.Boolean, Value = consultant.BoldDepartment },
                new DbParameters() { Name = "BoldConsultantName", Type = DbType.Boolean, Value = consultant.BoldConsultantName },
                new DbParameters() { Name = "BoldDesignation", Type = DbType.Boolean, Value = consultant.BoldDesignation },
                new DbParameters() { Name = "BoldQualifications", Type = DbType.Boolean, Value = consultant.BoldQualifications },
            };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("InsertUpdateReportConsultant", "System"), CommandType.StoredProcedure, pars));
        }

        public async Task<int> ToggleReportConsultantActive(int id)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "ReportConsultantId", Type = DbType.Int32, Value = id } };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("ToggleReportConsultantActive", "System"), CommandType.StoredProcedure, pars));
        }

        public async Task<int> DeleteReportConsultant(int id)
        {
            List<DbParameters> pars = new List<DbParameters>() { new DbParameters() { Name = "ReportConsultantId", Type = DbType.Int32, Value = id } };
            return Convert.ToInt32(await _db.ExecuteScalarAsync(QueryHelper.GetQueryString("DeleteReportConsultant", "System"), CommandType.StoredProcedure, pars));
        }
    }
}
