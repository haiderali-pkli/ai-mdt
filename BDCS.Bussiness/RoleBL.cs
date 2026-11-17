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

                            //every row is submitted now (checked or not) so an unchecked right
                            //can actually be revoked; a row with nothing granted is just deleted
                            //instead of being re-inserted as a no-op all-false row
                            await _role.DeleteRoleRight(RoleId, rr.MenuId);
                            if (rr.CanView || rr.CanAddEdit || rr.CanDelete)
                            {
                                await _role.InsertUpdateRoleRight(rr, "InsertRoleRight", "System");
                            }
                        }

                        //success is "the role itself saved", not "the last menu right insert
                        //returned a row" - a role saved with every right revoked is still valid
                        Result = RoleId;
                    }

                    scope.Complete();
                }
            }


            return Result;
        }
    }
}

