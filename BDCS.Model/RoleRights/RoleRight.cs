using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.RoleRights
{
    public class RoleRight
    {
        public int RightIdt { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string MenuName { get; set; }
        public string MenuGroupName { get; set; }
        public string Url { get; set; }
        public int MenuParentId { get; set; }
        public int MenuId { get; set; }
        public bool CanView { get; set; }
        public bool CanAddEdit { get; set; }
        public bool CanDelete { get; set; }

    }
}
