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
using BDCS.Model.Misc;

namespace BDCS.Data
{
    public class UserDAL
    {
        IDBManger _db;

        public UserDAL()
        {
            _db = new SqlDbManager();
        }
        //Get the RoleId such as Admin/User
        public Users ValidateLogin(string procedureName, string schema, string loginId,string password)
        {
            Users Data = new Users();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LoginId",Type=DbType.String,Value=loginId },
                new DbParameters() {Name="Password",Type=DbType.String,Value=password },
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Users
                    {
                        LoginId = a["LoginId"].ToString(),
                        UserId=Convert.ToInt32(a["UserId"]),
                        UserFullName=a["UserFullName"].ToString(),
                        RoleId=Convert.ToInt32(a["RoleId"]),
                        Active =Convert.ToBoolean(a["Active"]),
                        //this is because that we dont need a list only firsr row required therefore .Tolist(); will not use
                    }).FirstOrDefault();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }
        //Gets Menus against RoleId and get rights over the menus aginst RoleId
        public List<RoleRight> GetRoleRightsAgainstRole(string procedureName, string schema, int roleId)
        {
            List<RoleRight> Data = new List<RoleRight>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="RoleId",Type=DbType.Int32,Value=roleId}
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new RoleRight
                    {
                        RoleId  = Convert.ToInt32(a["RoleId"]),
                        RoleName = a["RoleName"].ToString(),
                        MenuId  = Convert.ToInt32(a["MenuId"]),
                        MenuName = a["MenuName"].ToString(),//Patient--Add Patient
                        MenuGroupName = a["MenuGroupName"].ToString(),//Setting, Patient
                        MenuParentId = Convert.ToInt32(a["MenuParentId"]),
                        CanView  = Convert.ToBoolean(a["CanView"]),
                        CanAddEdit = Convert.ToBoolean(a["CanAddEdit"]),
                        CanDelete = Convert.ToBoolean(a["CanDelete"])


                    }).ToList();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public List<Role> GetRoleList(string procedureName, string schema)
        {
            List<Role> Data = new List<Role>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
               
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Role
                    {
                        RoleId = Convert.ToInt32(a["RoleId"]),
                        RoleName = a["RoleName"].ToString(),
                    }).ToList();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public List<Users> GetUserAgainstUserName(string procedureName,string schema,string parameter)
        {
            List<Users> Data = new List<Users>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="SearchUserByName", Type=DbType.String, Value=parameter }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Users
                    {
                        UserId = Convert.ToInt32(a["UserId"]),
                        RoleId = Convert.ToInt32(a["RoleId"]),
                        LoginId = a["LoginId"].ToString(),
                        UserFullName = a["UserFullName"].ToString(),
                        Password = a["Password"].ToString(),
                        Active=Convert.ToBoolean(a["Active"])
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Data;
        }

        public Users GetUserById(string procedureName, string schema, int parameter)
        {
            Users Data = new Users();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="UserId", Type=DbType.Int32, Value=parameter }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Users
                    {
                        UserId = Convert.ToInt32(a["UserId"]),
                        RoleId = Convert.ToInt32(a["RoleId"]),
                        LoginId = a["LoginId"].ToString(),
                        UserFullName = a["UserFullName"].ToString(),
                        Password = a["Password"].ToString(),
                        Active = Convert.ToBoolean(a["Active"])
                    }).ToList().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Data;
        }

        
        public async Task<int> InsertUpdateUser(Users user, string procedureName,string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="UserId", Type=DbType.String,Value=user.UserId },
                new DbParameters() {Name="LoginId", Type=DbType.String,Value=user.LoginId },
                new DbParameters() {Name="Password", Type=DbType.String,Value=user.Password },
                new DbParameters() {Name="RoleId", Type=DbType.Int32,Value=user.RoleId },
                new DbParameters() {Name="UserFullName", Type=DbType.String,Value=user.UserFullName },
                new DbParameters() {Name="LastLoginTime", Type=DbType.DateTime,Value=user.LastLoginTime },
                new DbParameters() {Name="Active", Type=DbType.Boolean,Value=user.Active },
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

        public List<KeyAndValuePair> GetUserClinics(int userId)
        {
            List<KeyAndValuePair> Data = new List<KeyAndValuePair>();
            string Query = QueryHelper.GetQueryString("GetUserClinics", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="UserId", Type=DbType.Int32, Value=userId}
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);

                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new KeyAndValuePair
                    {
                        Id = a["ClinicId"].ToString(),
                        Value = a["ClinicName"].ToString()
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Data;
        }
    }

    

}
