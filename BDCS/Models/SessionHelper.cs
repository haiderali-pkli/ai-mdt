using BDCS.Model.RoleRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }
}