using BDCS.Constants;
using BDCS.Model.Helper;
using BDCS.Model.RoleRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BDCS.Models
{
    public static class SessionHelper
    {
        // all the sesions object is validated here from Authenticataion model
        public static Users GetUserObject()=>
            HttpContext.Current.Session["User"] as Users;

        public static int GetUserId()
        {
            int UserId = 0;

            if (HttpContext.Current.Session["User"] != null)
            {
                UserId = (HttpContext.Current.Session["User"] as Users).UserId;
            }

            return UserId;
        }


        public static bool IsUserLoggedIn() =>
            (HttpContext.Current.Session["User"] as Users)?.UserId > 0;

        public static bool IsUserAuthorized(string menuName, RightType type)
        {
            bool IsAuthorized = false;

            List<RoleRight> UserRights = HttpContext.Current.Session["Rights"] as List<RoleRight>;

            if (UserRights?.Count > 0)
            {
                switch (type)
                {
                    case RightType.AddEdit:
                        IsAuthorized = UserRights.Where(a => a.MenuName == menuName && a.CanAddEdit).Count() > 0;
                        break;
                    case RightType.View:
                        IsAuthorized = UserRights.Where(a => a.MenuName == menuName && a.CanView).Count() > 0;
                        break;
                    case RightType.Delete:
                        IsAuthorized = UserRights.Where(a => a.MenuName == menuName && a.CanDelete).Count() > 0;
                        break;
                    default:
                        break;
                }
            }

            return IsAuthorized;
        }

        // Shared by CheckUserRights and the global RequireSession filter so an expired/missing
        // session always gets the same clean "please log in again" response - an AJAX-aware
        // JSON redirect for loadPage() calls, a plain redirect otherwise - instead of whatever
        // NullReferenceException the action body would have hit trying to read Session["User"].
        public static ActionResult BuildNotLoggedInResult(System.Web.Mvc.ControllerContext filterContext)
        {
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                UrlHelper helper = new UrlHelper(filterContext.RequestContext);
                AjaxResponse res = new AjaxResponse()
                {
                    Code = 403,
                    DoFullRedirect = true,
                    IsSuccess = false,
                    Message = "User Not Logged In",
                    MessageClass = MessageConstants.MessageClassError,
                    MessageTitle = "User Logged Off",
                    RedirectUrl = helper.Action("Login", "Home"),
                };

                return new JsonResult() { Data = res, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "application/json;" };
            }

            return new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary() {
                {"controller","Home" },
                {"action","Login" }
            });
        }
    }
}