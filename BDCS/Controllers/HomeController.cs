using BDCS.Bussiness;
using BDCS.Model.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BDCS.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Dashboard()
        {

            return View();
        }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        #region dashboard queries

        public async Task<ActionResult> UserWiseVisits(DateTime date)
        {
            CommonBL _common = new CommonBL();

            List<KeyAndValuePair> list = await _common.GetUserWiseVisitData(date);

            return Json(list);
        }

        public ActionResult Heartbeat()
        {
            //hopefuly this shsould work sir
           
            return Json(true);
        }

        #endregion
    }
}