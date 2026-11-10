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
    public class LocationController : Controller
    {
        LocationBL _locationBL = new LocationBL();

        public ActionResult AddEditLocation(int LocationId = 0)
        {
            Location Data = new Location();
            if (LocationId > 0)
            {
                Data = _locationBL.GetLocationById(LocationId);
            }
            return View(Data);
        }

        public ActionResult GetLocationList()
        {
            List<Location> Data = _locationBL.GetLocationList();
            return View(Data);
        }

        public async Task<ActionResult> InsertUpdateLocation(Location location)
        {
            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (location == null)
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
                    result = await _locationBL.InsertUpdateLocation(location);
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
                            RedirectUrl = Url.Action("GetLocationList", "Location"),
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
