using BDCS.Bussiness;
using BDCS.Constants;
using BDCS.Model.Helper;
using BDCS.Model.Misc;
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
                Data.RoleIds = _ubl.GetUserRoles(UserId).Select(a => a.RoleId).ToList();
                Data.ClinicIds = _ubl.GetUserClinics(UserId).Select(a => Convert.ToInt32(a.Id)).ToList();
            }
            List<Role> RoleList = _ubl.GetRoleList();
            ViewBag.RoleList = RoleList;

            List<Clinic> ClinicList = new ClinicBL().GetClinicList();
            ViewBag.ClinicList = ClinicList;

            return View(Data);
        }

        public ActionResult SearchByUserName(string searchByUserName)
        {
            List<Users> Data = new List<Users>();
            if (searchByUserName != null)
            {
                Data = _ubl.GetUserAgainstUserName(searchByUserName);
            }

            Dictionary<int, List<Role>> RolesByUser = new Dictionary<int, List<Role>>();
            Dictionary<int, List<KeyAndValuePair>> ClinicsByUser = new Dictionary<int, List<KeyAndValuePair>>();
            foreach (Users u in Data)
            {
                RolesByUser[u.UserId] = _ubl.GetUserRoles(u.UserId);
                ClinicsByUser[u.UserId] = _ubl.GetUserClinics(u.UserId);
            }

            ViewBag.SearchTextByUser = searchByUserName;
            ViewBag.GetUserList = Data;
            ViewBag.RolesByUser = RolesByUser;
            ViewBag.ClinicsByUser = ClinicsByUser;
            return View();
        }

        public async Task<ActionResult> InsertUpdateUser(Users user, List<int> RoleIds, List<int> ClinicIds)
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
                else if (RoleIds == null || !RoleIds.Any())
                {
                    response = new AjaxResponse
                    {
                        Code = 200,
                        IsSuccess = false,
                        Message = "Select at least one role",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning,
                    };
                }
                else
                {
                    if (user.UserId > 0 && string.IsNullOrEmpty(user.Password))
                    {
                        Users existing = _ubl.GetUserById(user.UserId);
                        user.Password = existing?.Password;
                    }

                    user.RoleId = RoleIds.Min();
                    result = await _ubl.InsertUpdateUser(user);
                    if (result > 0)
                    {
                        await _ubl.InsertUpdateUserRoles(result, RoleIds);
                        await _ubl.InsertUpdateUserClinics(result, ClinicIds);

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