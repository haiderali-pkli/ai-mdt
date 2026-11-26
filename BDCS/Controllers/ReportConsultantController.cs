using BDCS.Bussiness;
using BDCS.Constants;
using BDCS.Model.Helper;
using BDCS.Model.Misc;
using BDCS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace BDCS.Controllers
{
    // Admin screen for System.ReportConsultant - the consultant entries printed in the
    // footer of both the Liver MDT (Patient.rdlc) and Bone Marrow MDT (PatientMaster.rdlc)
    // visit reports. One shared list feeds both reports (see
    // PatientController.BuildConsultantFooterColumns), so there is only ever one screen,
    // not a separate one per MDT type.
    public class ReportConsultantController : Controller
    {
        ReportConsultantBL _reportConsultantBL = new ReportConsultantBL();

        [CheckUserRights(MenuName = "Report Consultants", Order = 1, UserRightType = RightType.View)]
        public ActionResult GetReportConsultantList(string searchText)
        {
            List<ReportConsultant> Data = _reportConsultantBL.GetReportConsultants();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                Data = Data.Where(a =>
                    (a.ConsultantName != null && a.ConsultantName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (a.Department != null && a.Department.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();
            }
            ViewBag.SearchText = searchText;
            ViewBag.DepartmentsVB = _reportConsultantBL.GetReportConsultants()
                .Select(a => a.Department).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().OrderBy(d => d).ToList();
            return View(Data);
        }

        [CheckUserRights(MenuName = "Report Consultants", Order = 1, UserRightType = RightType.View)]
        public ActionResult AddEditReportConsultant(int ReportConsultantId = 0)
        {
            ReportConsultant Data = ReportConsultantId > 0 ? _reportConsultantBL.GetReportConsultantById(ReportConsultantId) : new ReportConsultant { IsActive = true, BoldDepartment = true, ColumnNumber = 1 };
            ViewBag.DepartmentsVB = _reportConsultantBL.GetReportConsultants()
                .Select(a => a.Department).Where(d => !string.IsNullOrWhiteSpace(d)).Distinct().OrderBy(d => d).ToList();
            return View(Data ?? new ReportConsultant { IsActive = true, BoldDepartment = true, ColumnNumber = 1 });
        }

        [CheckUserRights(MenuName = "Report Consultants", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> InsertUpdateReportConsultant(ReportConsultant consultant)
        {
            AjaxResponse response;
            try
            {
                if (consultant == null || string.IsNullOrWhiteSpace(consultant.Department) || string.IsNullOrWhiteSpace(consultant.ConsultantName))
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess = false,
                        Message = "Department and Consultant Name are required",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning,
                    };
                }
                else
                {
                    int result = await _reportConsultantBL.InsertUpdateReportConsultant(consultant);
                    response = result > 0
                        ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Saved", MessageTitle = MessageConstants.SuccessfullInsertion, MessageClass = MessageConstants.MessageClassSuccess, DoFullRedirect = true, RedirectUrl = Url.Action("GetReportConsultantList", "ReportConsultant"), ResponseObject = result }
                        : new AjaxResponse { Code = 404, IsSuccess = false, Message = "Save failed", MessageTitle = MessageConstants.InsertionFailed, MessageClass = MessageConstants.MessageClassWarning };
                }
            }
            catch (Exception ex)
            {
                response = new AjaxResponse { Code = 505, IsSuccess = false, Message = "Internal error", MessageTitle = MessageConstants.InternalError, MessageClass = MessageConstants.MessageClassError, ResponseObject = ex.Message };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Report Consultants", Order = 1, UserRightType = RightType.Delete)]
        public async Task<ActionResult> ToggleReportConsultantActive(int ReportConsultantId)
        {
            AjaxResponse response;
            try
            {
                int result = await _reportConsultantBL.ToggleReportConsultantActive(ReportConsultantId);
                response = result > 0
                    ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Status updated", MessageTitle = MessageConstants.MessageClassSuccess, MessageClass = MessageConstants.MessageClassSuccess, ResponseObject = result }
                    : new AjaxResponse { Code = 404, IsSuccess = false, Message = "Update failed", MessageTitle = MessageConstants.InsertionFailed, MessageClass = MessageConstants.MessageClassWarning };
            }
            catch (Exception ex)
            {
                response = new AjaxResponse { Code = 505, IsSuccess = false, Message = "Internal error", MessageTitle = MessageConstants.InternalError, MessageClass = MessageConstants.MessageClassError, ResponseObject = ex.Message };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Report Consultants", Order = 1, UserRightType = RightType.Delete)]
        public async Task<ActionResult> DeleteReportConsultant(int ReportConsultantId)
        {
            AjaxResponse response;
            try
            {
                int result = await _reportConsultantBL.DeleteReportConsultant(ReportConsultantId);
                response = result > 0
                    ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Deleted", MessageTitle = MessageConstants.SuccessfullDeletion, MessageClass = MessageConstants.MessageClassSuccess, ResponseObject = result }
                    : new AjaxResponse { Code = 404, IsSuccess = false, Message = "Delete failed", MessageTitle = MessageConstants.InsertionFailed, MessageClass = MessageConstants.MessageClassWarning };
            }
            catch (Exception ex)
            {
                response = new AjaxResponse { Code = 505, IsSuccess = false, Message = "Internal error", MessageTitle = MessageConstants.InternalError, MessageClass = MessageConstants.MessageClassError, ResponseObject = ex.Message };
            }
            return Json(response);
        }
    }
}
