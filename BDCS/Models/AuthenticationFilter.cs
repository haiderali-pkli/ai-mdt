using BDCS.Constants;
using BDCS.Model.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BDCS.Models
{
    public class CheckUserRights : ActionFilterAttribute
    {
        public string MenuName { get; set; }
        public RightType UserRightType { get; set; }
        //onlogin i keep the user and roles in sessions and then from controller we pass the filter prameters and filter paramter depndent on session helper checks rights overmenun
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!SessionHelper.IsUserLoggedIn())
            {
                filterContext.Result = SessionHelper.BuildNotLoggedInResult(filterContext);
            }

            else
            {
                if (!SessionHelper.IsUserAuthorized(MenuName, UserRightType))
                {
                    if (filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        UrlHelper helper = new UrlHelper(filterContext.RequestContext);
                        AjaxResponse res = new AjaxResponse()
                        {
                            Code = 403,
                            IsSuccess = false,
                            Message = MessageConstants.UserNotAuthorized ,
                            MessageClass = MessageConstants.MessageClassWarning,
                            MessageTitle = "Not Authorized",

                        };

                        filterContext.Result = new JsonResult() { Data = res, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "application/json;" };
                    }

                    else
                    {
                        filterContext.Result =
                        new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary() {
                            {"controller", "Home" },
                            {"action","Index" }
                        });
                    }


                }
            }

            base.OnActionExecuting(filterContext);
        }
    }

    // Marker for actions/controllers that must stay reachable without a session: the login
    // page itself, login/logout endpoints, the heartbeat ping, and the separate ASP.NET
    // Identity controllers (AccountController/ManageController) which already protect
    // themselves with the framework's own [Authorize].
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AllowAnonymousSessionAttribute : Attribute
    {
    }

    // Registered globally (see FilterConfig) so every action - not just the ones that
    // remembered to add [CheckUserRights] - gets a clean "please log in again" redirect
    // when Session["User"] has expired or was never set. Before this, actions like
    // VitalHistory, AddEditPatientDetail, and ToggleDiscussionStatus read Session members
    // with no null check at all and would throw a raw NullReferenceException instead.
    public class RequireSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool allowAnonymous = filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousSessionAttribute), true)
                || filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousSessionAttribute), true);

            if (!allowAnonymous && !SessionHelper.IsUserLoggedIn())
            {
                filterContext.Result = SessionHelper.BuildNotLoggedInResult(filterContext);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}