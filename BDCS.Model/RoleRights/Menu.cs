using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.RoleRights
{
    public class Menu
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string MenuGroupName { get; set; }
        public int MenuParentId { get; set; }
        public string URL { get; set; }
        public bool Active { get; set; }

    }
}
