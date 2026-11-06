using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDCS.Model.Patient;
using BDCS.Data;
using BDCS.Model.Visit;
using BDCS.Model.VitalHistory;
using System.Transactions;
using BDCS.Model.RoleRights;
using BDCS.Model.Misc;

namespace BDCS.Bussiness
{
   public class UserBL
    {
        readonly UserDAL _userDAL;
        
        public UserBL()
        {
            _userDAL = new UserDAL();
        }

        public Users ValidateLogin(string loginId, string password)
            => _userDAL.ValidateLogin("ValidateLogin", "System", loginId, password);

        public List<RoleRight> GetRoleRightsAgainstRole(int roleId)
            => _userDAL.GetRoleRightsAgainstRole("GetRightsAgainstRole", "System", roleId);

        public List<RoleRight> GetRoleRightsAgainstRoles(List<int> roleIds)
            => _userDAL.GetRoleRightsAgainstRoles("GetRightsAgainstRoles", "System", string.Join(",", roleIds));

        public List<Users> GetUserAgainstUserName(string searchByUserName)
            => _userDAL.GetUserAgainstUserName("UserSearchByName", "System", searchByUserName);

        public Users GetUserById(int userId)
            => _userDAL.GetUserById("GetUserById", "System", userId);

        public List<Role> GetRoleList()
            => _userDAL.GetRoleList("GetRoleList", "System");
        public async Task<int> InsertUpdateUser(Users user) 
            => await _userDAL.InsertUpdateUser(user,"InsertUpdateUser", "System");

        public List<KeyAndValuePair> GetUserClinics(int userId)
            => _userDAL.GetUserClinics(userId);

        public List<Role> GetUserRoles(int userId)
            => _userDAL.GetUserRoles(userId);

        public async Task<int> InsertUpdateUserRoles(int userId, List<int> roleIds)
            => await _userDAL.InsertUpdateUserRoles(userId, string.Join(",", roleIds ?? new List<int>()));

        public async Task<int> InsertUpdateUserClinics(int userId, List<int> clinicIds)
            => await _userDAL.InsertUpdateUserClinics(userId, string.Join(",", clinicIds ?? new List<int>()));
    }
}

