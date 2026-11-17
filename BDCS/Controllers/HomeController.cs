using BDCS.Bussiness;
using BDCS.Model.Misc;
using BDCS.Models;
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

        [AllowAnonymousSession]
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

        [AllowAnonymousSession]
        public ActionResult Login()
        {
            //hitting the login page (or the "/" root, which routes here by default) while
            //already logged in should land you back where you were, not re-show the login
            //form - the session was fine, this page just never checked
            if (SessionHelper.IsUserLoggedIn())
            {
                return RedirectToAction("GetPatientList", "Patient");
            }

            return View();
        }

        #region dashboard queries

        public async Task<ActionResult> UserWiseVisits(DateTime date)
        {
            CommonBL _common = new CommonBL();

            List<KeyAndValuePair> list = await _common.GetUserWiseVisitData(date);

            return Json(list);
        }

        [AllowAnonymousSession]
        public ActionResult Heartbeat()
        {
            //hopefuly this shsould work sir
           
            return Json(true);
        }

        #endregion
    }
}