using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDCS.Data.Helpers
{
    public class QueryHelper
    {
        public static string GetQueryString(string query,string schema )
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(schema))
            {
                return query;
            }
            return $"{schema}.{query}";
        }
    }
}
