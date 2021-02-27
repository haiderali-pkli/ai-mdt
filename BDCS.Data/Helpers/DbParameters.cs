using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Data.Helpers
{
    public class DbParameters
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public DbType Type { get; set; }

        public DbParameters()
        {
            Name = "";
            Value = null;
            Type = DbType.String;
        }

        public DbParameters(string name,object value,DbType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
    }    
}
