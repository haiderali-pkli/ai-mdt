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

namespace BDCS.Bussiness
{
   public class RoleBL
    {
        readonly RoleDAL _role;
        
        public RoleBL()
        {
            _role = new RoleDAL();
        }

        public List<RoleRight> GetMenuRoleRights(int roleId)
            => _role.GetMenuRoleRights(roleId);
        public List<Role> GetRoleListByName(string searchText)
            => _role.GetRoleListByName(searchText);
        
        //public async Task<int> InsertUpdateRoleRight(RoleRight roleRight)
        //    => await _role.InsertUpdateRoleRight(roleRight, "InsertRoleRight", "System");

        public Role GetRoleAgainstId(int roleId)
            => _role.GetRoleAgainstId(roleId);

        public async Task<int> InsertUpdateRoleAndRights(Role role, List<RoleRight> rights)
        {
            int Result = 0, RoleId = 0;

            if (role != null && rights != null && rights.Any())
            {

                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    RoleId = await _role.InsertUpdateRole(role);

                    if (RoleId > 0 )
                    {
                        foreach (RoleRight rr in rights)
                        {
                            rr.RoleId = RoleId;

                            //TODO: call delete method and pass roleid and menuid
                            await _role.DeleteRoleRight(RoleId, rr.MenuId);
                            Result = await _role.InsertUpdateRoleRight(rr,"InsertRoleRight", "System");
                        }
                    }

                    scope.Complete();
                }
            }


            return Result;
        }
    }
}

