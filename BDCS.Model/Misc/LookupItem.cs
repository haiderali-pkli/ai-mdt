using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Model.Misc
{
    public class LookupItem
    {
        public int LookupId { get; set; }
        public string DisplayValue { get; set; }
        public string ReturnValue { get; set; }
        public string LookupType { get; set; }
        public bool IsActive { get; set; }
    }
}
