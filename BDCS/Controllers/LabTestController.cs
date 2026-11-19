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
    public class LabTestController : Controller
    {
        LabTestBL _labTestBL = new LabTestBL();

        // ===== Liver (System.LabTests) =====

        [CheckUserRights(MenuName = "Lab Tests", Order = 1, UserRightType = RightType.View)]
        public ActionResult GetLabTestList(string searchText)
        {
            List<LabTestDefinition> Data = _labTestBL.GetLabTestList();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                Data = Data.Where(a => a.TestName != null && a.TestName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            ViewBag.SearchText = searchText;
            return View(Data);
        }

        [CheckUserRights(MenuName = "Lab Tests", Order = 1, UserRightType = RightType.View)]
        public ActionResult AddEditLabTest(int TestId = 0)
        {
            LabTestDefinition Data = TestId > 0 ? _labTestBL.GetLabTestById(TestId) : new LabTestDefinition();
            return View(Data ?? new LabTestDefinition());
        }

        [CheckUserRights(MenuName = "Lab Tests", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> InsertUpdateLabTest(LabTestDefinition test)
        {
            AjaxResponse response;
            try
            {
                if (test == null || string.IsNullOrWhiteSpace(test.TestName))
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess = false,
                        Message = "Test name is required",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning,
                    };
                }
                else
                {
                    int result = await _labTestBL.InsertUpdateLabTest(test);
                    response = result > 0
                        ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Saved", MessageTitle = MessageConstants.SuccessfullInsertion, MessageClass = MessageConstants.MessageClassSuccess, DoFullRedirect = true, RedirectUrl = Url.Action("GetLabTestList", "LabTest"), ResponseObject = result }
                        : new AjaxResponse { Code = 404, IsSuccess = false, Message = "Save failed", MessageTitle = MessageConstants.InsertionFailed, MessageClass = MessageConstants.MessageClassWarning };
                }
            }
            catch (Exception ex)
            {
                response = new AjaxResponse { Code = 505, IsSuccess = false, Message = "Internal error", MessageTitle = MessageConstants.InternalError, MessageClass = MessageConstants.MessageClassError, ResponseObject = ex.Message };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Lab Tests", Order = 1, UserRightType = RightType.Delete)]
        public async Task<ActionResult> DeleteLabTest(int TestId)
        {
            AjaxResponse response;
            try
            {
                int result = await _labTestBL.DeleteLabTest(TestId);
                response = result > 0
                    ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Deleted", MessageTitle = MessageConstants.MessageClassSuccess, MessageClass = MessageConstants.MessageClassSuccess, ResponseObject = result }
                    : new AjaxResponse { Code = 404, IsSuccess = false, Message = "Delete failed", MessageTitle = MessageConstants.InsertionFailed, MessageClass = MessageConstants.MessageClassWarning };
            }
            catch (Exception ex)
            {
                response = new AjaxResponse { Code = 505, IsSuccess = false, Message = "Internal error", MessageTitle = MessageConstants.InternalError, MessageClass = MessageConstants.MessageClassError, ResponseObject = ex.Message };
            }
            return Json(response);
        }

        // ===== Bone Marrow (System.LabTestsBMT) =====

        [CheckUserRights(MenuName = "Lab Tests BMT", Order = 1, UserRightType = RightType.View)]
        public ActionResult GetLabTestListBMT(string searchText)
        {
            List<LabTestDefinition> Data = _labTestBL.GetLabTestListBMT();
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                Data = Data.Where(a => a.TestName != null && a.TestName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }
            ViewBag.SearchText = searchText;
            return View(Data);
        }

        [CheckUserRights(MenuName = "Lab Tests BMT", Order = 1, UserRightType = RightType.View)]
        public ActionResult AddEditLabTestBMT(int TestId = 0)
        {
            LabTestDefinition Data = TestId > 0 ? _labTestBL.GetLabTestByIdBMT(TestId) : new LabTestDefinition();
            return View(Data ?? new LabTestDefinition());
        }

        [CheckUserRights(MenuName = "Lab Tests BMT", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> InsertUpdateLabTestBMT(LabTestDefinition test)
        {
            AjaxResponse response;
            try
            {
                if (test == null || string.IsNullOrWhiteSpace(test.TestName))
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess = false,
                        Message = "Test name is required",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning,
                    };
                }
                else
                {
                    int result = await _labTestBL.InsertUpdateLabTestBMT(test);
                    response = result > 0
                        ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Saved", MessageTitle = MessageConstants.SuccessfullInsertion, MessageClass = MessageConstants.MessageClassSuccess, DoFullRedirect = true, RedirectUrl = Url.Action("GetLabTestListBMT", "LabTest"), ResponseObject = result }
                        : new AjaxResponse { Code = 404, IsSuccess = false, Message = "Save failed", MessageTitle = MessageConstants.InsertionFailed, MessageClass = MessageConstants.MessageClassWarning };
                }
            }
            catch (Exception ex)
            {
                response = new AjaxResponse { Code = 505, IsSuccess = false, Message = "Internal error", MessageTitle = MessageConstants.InternalError, MessageClass = MessageConstants.MessageClassError, ResponseObject = ex.Message };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Lab Tests BMT", Order = 1, UserRightType = RightType.Delete)]
        public async Task<ActionResult> DeleteLabTestBMT(int TestId)
        {
            AjaxResponse response;
            try
            {
                int result = await _labTestBL.DeleteLabTestBMT(TestId);
                response = result > 0
                    ? new AjaxResponse { Code = 200, IsSuccess = true, Message = "Deleted", MessageTitle = MessageConstants.MessageClassSuccess, MessageClass = MessageConstants.MessageClassSuccess, ResponseObject = result }
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
