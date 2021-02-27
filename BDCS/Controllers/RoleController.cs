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
    public class RoleController : Controller
    {
        RoleBL _roleBL = new RoleBL();
        public ActionResult AddEditRole(int id =0)
        {
            Role Role = new Role();
            List<RoleRight> Rights = new List<RoleRight>();

            if (id > 0)
            {

                Role = new RoleBL().GetRoleAgainstId(id);

            } 

            Rights = new RoleBL().GetMenuRoleRights(id);

            ViewBag.Rights = Rights;


            return View(Role);
        }

       
        public async Task<ActionResult> InsertUpdateRoleRight(Role Role,  List<RoleRight> Rights)
        {
            AjaxResponse res = new AjaxResponse();
            int result = 0;
            try
            {
                if (Role == null)
                {
                    res = new AjaxResponse
                    {
                        Code = 200,
                        Message = "No Data",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning,
                        ResponseObject = result,
                    };
                }
                else
                {
                    result = await _roleBL.InsertUpdateRoleAndRights(Role,Rights);
                    if (result > 0)
                    {
                        res = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            Message = "Saved",
                            MessageTitle = MessageConstants.SuccessfullInsertion,
                            MessageClass = MessageConstants.MessageClassSuccess,
                            DoFullRedirect = true,
                            RedirectUrl = Url.Action("GetRoleList", "Role"),
                            ResponseObject = result,
                        };
                    }
                    else
                    {

                        res = new AjaxResponse
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

                res = new AjaxResponse
                {
                    Code = 505,
                    IsSuccess = false,
                    Message = "Internal error",
                    MessageTitle = MessageConstants.InternalError,
                    MessageClass = MessageConstants.MessageClassError,
                    ResponseObject = ex.Message,
                };
                        
            }
            finally { }
            return Json(res);
        }

        public async Task<ActionResult> GetRoleList(string searchText)
        {
            List<Role> Data = new List<Role>();
            if (searchText != null)
            {

                await Task.Run(() =>
               {
                   Data = _roleBL.GetRoleListByName(searchText);
               });
            }
            ViewBag.SearchTextbyRole = searchText;
            return View(Data);
        }
    }
}