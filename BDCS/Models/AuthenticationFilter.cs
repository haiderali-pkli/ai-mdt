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
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    UrlHelper helper = new UrlHelper(filterContext.RequestContext);
                    AjaxResponse res = new AjaxResponse() {
                        Code = 403,
                        DoFullRedirect = true,
                        IsSuccess = false,
                        Message = "User Not Logged In",
                        MessageClass = MessageConstants.MessageClassError,
                        MessageTitle = "User Logged Off",
                        RedirectUrl = helper.Action("Login","Home"),
                     
                    };

                    filterContext.Result = new JsonResult() { Data = res, JsonRequestBehavior = JsonRequestBehavior.AllowGet, ContentType = "application/json;" };
                }

                else
                {
                    filterContext.Result =
                        new RedirectToRouteResult(new System.Web.Routing.RouteValueDictionary() {
                            {"controller","Home" },
                            {"action","Login" }
                        });
                }
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
}