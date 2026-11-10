using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.RoleRights
{
    public class Users
    {
        public int UserId { get; set; }
        public string LoginId { get; set; }
        public string Password { get; set; }
        public int MyProperty { get; set; }
        public int RoleId { get; set; }
        public string UserFullName { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public bool Active { get; set; }
        public string mdtType { get; set; }
        public List<int> RoleIds { get; set; } = new List<int>();
        public List<int> ClinicIds { get; set; } = new List<int>();
        public List<int> LocationIds { get; set; } = new List<int>();
    }
}
