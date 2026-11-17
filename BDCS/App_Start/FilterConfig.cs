using BDCS.Models;
using System.Web;
using System.Web.Mvc;

namespace BDCS
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new RequireSessionAttribute());
        }
    }
}
