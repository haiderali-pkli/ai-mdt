using BDCS.Bussiness;
using BDCS.Constants;
using BDCS.Model.Helper;
using BDCS.Model.RoleRights;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BDCS.Controllers
{
    public class UserController : Controller
    {
        UserBL _ubl;

        public UserController()
        {
            _ubl = new UserBL();
        }
        // GET: User
        public ActionResult Index()
        {
       
            return View();
        }


        public ActionResult AddEditUser(int UserId=0)
       {
            Users Data = new Users();
            if (UserId>0)
            {
                Data = _ubl.GetUserById(UserId);
            }
            List<Role> RoleList = _ubl.GetRoleList();
            ViewBag.RoleListVB = RoleList.Select(a => new SelectListItem() { Text = a.RoleName, Value = a.RoleId.ToString() }).ToList();

            return View(Data);
        }

        public ActionResult SearchByUserName(string searchByUserName)
        {
            List<Users> Data = new List<Users>();
            if (searchByUserName != null)
            {
                Data = _ubl.GetUserAgainstUserName(searchByUserName);

            }
            ViewBag.SearchTextByUser = searchByUserName;
            ViewBag.GetUserList = Data;
            return View();
        }

        public async Task<ActionResult> InsertUpdateUser(Users user)
        {
            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (user == null)
                {
                    response = new AjaxResponse
                    {
                        Code = 200,
                        IsSuccess = false,
                        Message = "No Data",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning,
                        ResponseObject = result,
                    };
                }
                else
                {
                    result = await _ubl.InsertUpdateUser(user);
                    if (result > 0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            Message = "Saved",
                            MessageTitle = MessageConstants.SuccessfullInsertion,
                            MessageClass = MessageConstants.MessageClassSuccess,
                            DoFullRedirect = true,
                            RedirectUrl = Url.Action("SearchByUserName", "User"),
                            ResponseObject = result,
                        };
                    }
                    else
                    {
                        response = new AjaxResponse
                        {
                            Code = 404,
                            IsSuccess = false,
                            Message = "Insertion failed",
                            MessageTitle = MessageConstants.InsertionFailed,
                            MessageClass = MessageConstants.MessageClassWarning,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                response = new AjaxResponse
                {
                    Code = 505,
                    IsSuccess = false,
                    Message = "Internal error",
                    MessageTitle = MessageConstants.InternalError,
                    MessageClass = MessageConstants.MessageClassError,
                    ResponseObject = ex.Message,
                };

            }
            finally
            {

            }
            return Json(response);
        }
    }
}