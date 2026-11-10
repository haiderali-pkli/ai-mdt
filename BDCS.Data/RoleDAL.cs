using BDCS.Data.Helpers;
using BDCS.Model.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BDCS.Model.Visit;
using BDCS.Model.VitalHistory;
using BDCS.Model.RoleRights;

namespace BDCS.Data
{
    public class RoleDAL
    {
        IDBManger _db;

        public RoleDAL()
        {
            _db = new SqlDbManager();
        }

        public List<RoleRight> GetMenuRoleRights(int roleId)
       {
            List<RoleRight> Data = new List<RoleRight>();
            string Query = QueryHelper.GetQueryString("GetSystemRights", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "RoleId", Type = DbType.Int32, Value = roleId }
 
            };
 
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new RoleRight()
                    {
                        MenuId = Convert.ToInt32(a["MenuId"]),
                        MenuGroupName = a["MenuGroupName"].ToString(),
                        MenuName = a["MenuName"].ToString(),
                        MenuParentId = Convert.ToInt32(a["MenuParentId"]),
                        CanView = a["CanView"] != DBNull.Value && Convert.ToBoolean(a["CanView"]),
                        CanAddEdit = a["CanAddEdit"] != DBNull.Value && Convert.ToBoolean(a["CanAddEdit"]),
                        CanDelete = a["CanDelete"] != DBNull.Value && Convert.ToBoolean(a["CanDelete"]),

                    }).ToList();
                    
                }
            }
            catch (Exception )
            {
                throw;
            }
            return Data;
        }

        public List<Role> GetRoleListByName(string searchText)
        {
            List<Role> Data = new List<Role>();
            string Query = QueryHelper.GetQueryString("GetRoleListAgainstSearchText", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "searchRoleByName", Type = DbType.String, Value = searchText ??"" }

            };

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Role()
                    {
                        RoleId = Convert.ToInt32(a["RoleId"]),
                        RoleName = a["RoleName"].ToString(),
                        Active = a["Active"] != DBNull.Value && Convert.ToBoolean(a["Active"]),
                    }).ToList();

                }
            }
            catch (Exception)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> InsertUpdateRoleRight(RoleRight roleRight, string procedureName,string schema)
        {

            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="RoleId",Type=DbType.Int16, Value=roleRight.RoleId },
                new DbParameters() {Name="MenuId",Type=DbType.String, Value=roleRight.MenuId },
                new DbParameters() {Name="CanView", Type=DbType.String,Value=roleRight.CanView },
                new DbParameters() {Name="CanEdit", Type=DbType.String,Value=roleRight.CanAddEdit },
                new DbParameters() {Name="CanDelete", Type=DbType.String,Value=roleRight.CanDelete },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }

        public Role GetRoleAgainstId(int roleId)
        {
            Role Data = new Role();
            string Query = QueryHelper.GetQueryString("GetRoleById", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "RoleId", Type = DbType.Int32, Value = roleId }

            };

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Role()
                    {
                        RoleId = Convert.ToInt32(a["RoleId"]),
                        RoleName = a["RoleName"].ToString(),
                        Active = a["Active"] != DBNull.Value && Convert.ToBoolean(a["Active"])

                    }).FirstOrDefault();

                }
            }
            catch (Exception)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> DeleteRoleRight(int roleId, int menuId)
        {

            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteRoleRightAgainstRole", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="RoleId",Type=DbType.Int16, Value=roleId},
                new DbParameters() {Name="MenuId",Type=DbType.String, Value=menuId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }

        public async Task<int> InsertUpdateRole(Role role)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InserUpdateRole", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="RoleId",Type=DbType.Int32, Value=role.RoleId },
                new DbParameters() {Name="RoleName",Type=DbType.String, Value=role.RoleName},
                new DbParameters() {Name="Active", Type=DbType.Boolean,Value=role.Active }
            };
            
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }
    }
}
