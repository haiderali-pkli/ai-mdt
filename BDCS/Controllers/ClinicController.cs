using BDCS.Bussiness;
using BDCS.Constants;
using BDCS.Model.Helper;
using BDCS.Model.RoleRights;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BDCS.Controllers
{
    public class ClinicController : Controller
    {
        ClinicBL _clinicBL = new ClinicBL();

        public ActionResult AddEditClinic(int ClinicId = 0)
        {
            Clinic Data = new Clinic();
            if (ClinicId > 0)
            {
                Data = _clinicBL.GetClinicById(ClinicId);
            }
            return View(Data);
        }

        public ActionResult GetClinicList(string searchText)
        {
            List<Clinic> Data = _clinicBL.GetClinicList();
            ViewBag.SearchText = searchText;
            return View(Data);
        }

        public async Task<ActionResult> InsertUpdateClinic(Clinic clinic)
        {
            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (clinic == null)
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
                    result = await _clinicBL.InsertUpdateClinic(clinic);
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
                            RedirectUrl = Url.Action("GetClinicList", "Clinic"),
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
            return Json(response);
        }
    }
}
