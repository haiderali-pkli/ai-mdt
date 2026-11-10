using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BDCS.Bussiness;
using System.Threading.Tasks;
using BDCS.Model.Misc;
using BDCS.Model.Patient;
using BDCS.Model.Helper;
using BDCS.Model.Visit;
using BDCS.Constants;
using BDCS.Model.VitalHistory;
using BDCS.Model.RoleRights;
using BDCS.Models;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using BDCS.Model.PatientServices;
using Microsoft.Reporting.WebForms;
using System.Data;
using System.IO;
using System.Reflection;
using Microsoft.Ajax.Utilities;
using DocumentFormat.OpenXml.Office2010.Excel;
using static Antlr.Runtime.Tree.TreeWizard;

namespace BDCS.Controllers
{
    public class PatientController : Controller
    {
        CommonBL _CommonBL;
        PatientBL _PatientBL;

        // GET: Patient
        #region [Constructor]
        public PatientController()
        {
            _CommonBL = new CommonBL();
            _PatientBL = new PatientBL();

        }
        #endregion

        // Comma-separated ids of the Locations assigned to the logged-in user (System.UserLocation),
        // cached in Session at login. Empty means "no Locations assigned" -> unrestricted (legacy behaviour).
        private string GetUserLocationIdsCsv()
        {
            List<int> ids = Session["LocationIds"] as List<int>;
            return ids != null && ids.Count > 0 ? string.Join(",", ids) : string.Empty;
        }

        #region [AddEditPatient]
        public async Task<ActionResult> AddEditPatient(int patId = 0)
        {
            Patient pat = new Patient();
            if (patId > 0)
            {
                pat = _PatientBL.GetPatientById(patId, GetUserLocationIdsCsv());
                if (pat.PatientId <= 0)
                {
                    // Either the patient doesn't exist, or it's tagged with a Location this user isn't assigned to.
                    return RedirectToAction("GetPatientList", "Patient");
                }
                if (pat.DOB.HasValue && pat.DOB.Value.Date != DateTime.MinValue.Date)
                {
                    int PatientAgeDays = Convert.ToInt32((DateTime.Now.Date - pat.DOB.Value.Date).TotalDays);

                    if (PatientAgeDays >= 365)
                    {
                        pat.Age = Math.Round((PatientAgeDays *1.0) / 365,2);
                        pat.AgeUnit = "Years";

                    }
                    else if (PatientAgeDays >= 30 && PatientAgeDays < 365)
                    {
                        pat.Age = PatientAgeDays / 30;
                        pat.AgeUnit = "Months";

                    }
                    else if (PatientAgeDays < 30)
                    {
                        pat.Age = PatientAgeDays;
                        pat.AgeUnit = "Days";

                    }
                }

            }
            else
            {
                pat.Age = 1;
            }


            List<KeyAndValuePair> Diagnosis = await _CommonBL.GetDefinationData("lookups", "System", "DiagnosisType");
            List<KeyAndValuePair> RelationShip = await _CommonBL.GetDefinationData("lookups", "System", "RelationType");
            List<KeyAndValuePair> Gender = await _CommonBL.GetDefinationData("lookups", "System", "GenderType");
            List<KeyAndValuePair> Country = await _CommonBL.GetDefinationData("GetCountryList", "System", "");
            List<KeyAndValuePair> Referredby=await _CommonBL.GetDefinationData("lookups", "System", "ReferredBy");
            List<KeyAndValuePair> Ethnicity = await _CommonBL.GetDefinationData("lookups", "System", "Ethnicity");

            List<KeyAndValuePair> BloodGroup = await _CommonBL.GetDefinationData("lookups", "System", "BloodGroup");

            List<KeyAndValuePair> AgeUnit = new List<KeyAndValuePair>() {
                new KeyAndValuePair() { Id = "Years", Value = "Years" },
                new KeyAndValuePair() { Id = "Days", Value = "Days" },
                new KeyAndValuePair() { Id = "Months", Value = "Months" },
              
            };

            
            ViewBag.DiagnosisVB = Diagnosis.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.RelationshipVB = RelationShip.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.GenderVB = Gender.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.CountryVB = Country.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.ReferredbyVB = Referredby.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.AgeUnit = AgeUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.EthnicityVB = Ethnicity.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            ViewBag.BloodGroupVB = BloodGroup.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            // Only the Locations this user is assigned to - a patient can only be registered against one of them.
            Users currentUser = Session["User"] as Users;
            List<KeyAndValuePair> userLocations = currentUser != null ? new UserBL().GetUserLocations(currentUser.UserId) : new List<KeyAndValuePair>();
            ViewBag.LocationVB = userLocations.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            return View(pat);
        }

        public async Task<ActionResult> GetProvinceListById(int Id = 0)
        {
            List<KeyAndValuePair> Province = await _CommonBL.GetDefinationData("GetProvinceList", "System", Id.ToString());
            return Json(Province, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetDistrictListById(int Id = 0)
        {
            List<KeyAndValuePair> District = await _CommonBL.GetDefinationData("GetDistrictList", "System", Id.ToString());
            return Json(District, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Home");
        }
        //LoginView Submit--> and call the -->ValidageLoginId and Rights-->PatientDAL-->AuthenticationFilter(check session values userId and Rigts)-->SessionHelper(check userobject and rights exists in session)-->
        #region [GetPatientList]
        [CheckUserRights(MenuName  = "Search Patient", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> GetPatientList(string searchText)
         {
            List<Patient> pat = new List<Patient>();
            int clinicId = Convert.ToInt32(Session["ClinicId"]);

            searchText = searchText ?? string.Empty;
            Users user = Session["user"] as Users;
            int userId = user.UserId;
            if (user==null)
            {
                return RedirectToAction("Home", "Login");
            }
            if (userId>0)
            {
                if (searchText !=null)
                {
                    await Task.Run(() =>
                    {
                        pat = _PatientBL.GetPatientList(searchText, clinicId, GetUserLocationIdsCsv());
                    });
                }
                ViewBag.SearchText = searchText;
                List<KeyAndValuePair> visit = await _CommonBL.GetDefinationData("lookups", "System", "VisitType");
                ViewBag.VisitType = visit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();



                List<KeyAndValuePair> data = new List<KeyAndValuePair>();
                //Users user = Session["user"] as Users;
                //int userId = user.UserId;
                data = _CommonBL.GetMDTTypes(userId);
                ViewBag.MDTTypes = data.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

                List<KeyAndValuePair> mdtTypesName = new List<KeyAndValuePair>();
                mdtTypesName = _CommonBL.GetMDTTypesNameByUserId(userId);
                ViewBag.mdtTypesNameVb = mdtTypesName.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            }
            else
            {
                return RedirectToAction("Home", "Login");
            }
            

            return View(pat);
        }

        [CheckUserRights(MenuName = "Add Patient", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> InsertUpdatePatient(Patient pat, HttpPostedFileBase PatientImage)
        {
            //Patient patient = new Patient();

            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (pat==null)
                {
                    response = new AjaxResponse
                    {
                        Code=403,
                        IsSuccess=false,
                        Message="No Data",
                        MessageTitle=MessageConstants.NoDataReceived,
                        MessageClass=MessageConstants.MessageClassWarning
                    };
                }
                else
                {

                    if (await _PatientBL.DoesPatientHospitalNumberExists(pat.HospitalNumber, pat.PatientId) > 0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 403,
                            IsSuccess = false,
                            Message = "Patient Hospital Number already present",
                            MessageTitle = MessageConstants.NoDataReceived,
                            MessageClass = MessageConstants.MessageClassWarning,
                            ResponseObject = 0
                        };

                        return Json(response);
                    }

                    if (pat.PatientId < 1)
                    {
                        // assume its a new patient

                        if (pat.Age > 0 && !string.IsNullOrEmpty(pat.AgeUnit))
                        {
                            switch (pat.AgeUnit.ToLower())
                            {

                                case "days":
                                    pat.DOB = DateTime.Now.AddDays(pat.Age * -1);
                                    break;

                                case "months":
                                    pat.DOB = DateTime.Now.AddMonths(Convert.ToInt32(pat.Age) * -1);
                                    break;

                                case "years":
                                    if (Math.Floor(pat.Age) < pat.Age )
                                    {
                                        int AgeDays = (int) ( (Math.Floor(pat.Age) * 365) + ((pat.Age - Math.Floor(pat.Age) ) * 365 ));
                                        pat.DOB = pat.DOB = DateTime.Now.AddDays(AgeDays * -1);
                                    }
                                    else
                                    {
                                        pat.DOB = DateTime.Now.AddYears(Convert.ToInt32(pat.Age) * -1);
                                    }
                                    
                                    break;
                                    

                                default:
                                    break;
                            }
                        }
                    }

                    


                    if (PatientImage != null && PatientImage.ContentLength > 0)
                    {
                        string FileName = pat.FirstName + "_" + pat.LastName + "_" + PatientImage.FileName;

                        if (!System.IO.Directory.Exists(Server.MapPath("~/Uploads")))
                        {
                            System.IO.Directory.CreateDirectory(Server.MapPath("~/Uploads"));
                        }
                        
                        PatientImage.SaveAs(Server.MapPath("~/Uploads") + "/"+ FileName);
                        pat.ImageURL = "~/Uploads/" + FileName;
                    }

                    if (pat.CameraPicture!=null)
                    {
                        var base64Data = Regex.Match(pat.CameraPicture, @"data:image/(?<type>.+?),(?<data>.+)").Groups["data"].Value;
                        var binData = Convert.FromBase64String(base64Data);


                        string FileName = pat.FirstName + "_" + pat.LastName + "_" + DateTime.Now.ToString("ddMMyyhhmm") + ".jpeg";

                        if (!System.IO.Directory.Exists(Server.MapPath("~/Uploads")))
                        {
                            System.IO.Directory.CreateDirectory(Server.MapPath("~/Uploads"));
                        }
                        if (!System.IO.File.Exists(Server.MapPath("~/Uploads/") + FileName))
                        {
                            //using (var stream = new MemoryStream(binData))
                            //{
                            System.IO.File.WriteAllBytes(Server.MapPath("~/Uploads/") + FileName, binData);
                            //}
                        }
                        
                        pat.ImageURL = "~/Uploads/" + FileName;

                       


                    }

                    Users u = Session["User"] as Users;
                    pat.EnteredBy = u.UserId;
                    pat.ClinicId = Convert.ToInt32(Session["ClinicId"]);

                    // Stamp the patient with a Location this user is actually assigned to - never trust the
                    // posted LocationId blindly. A null LocationId on an update is a no-op (the proc keeps
                    // whatever the patient already had), so this only ever narrows access, never un-assigns
                    // a patient someone else correctly tagged earlier.
                    List<int> allowedLocationIds = (Session["LocationIds"] as List<int>) ?? new List<int>();
                    bool postedLocationIsAllowed = pat.LocationId.HasValue && allowedLocationIds.Contains(pat.LocationId.Value);
                    if (pat.PatientId <= 0 && allowedLocationIds.Count == 1)
                    {
                        // New patient, operator has exactly one Location -> use it automatically.
                        pat.LocationId = allowedLocationIds[0];
                    }
                    else if (!postedLocationIsAllowed)
                    {
                        pat.LocationId = null;
                    }

                    result = await _PatientBL.InsertUpdatePatient(pat);
                    if (result>0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            MessageTitle = MessageConstants.SuccessfullInsertion,
                            MessageClass = MessageConstants.MessageClassSuccess,
                            DoFullRedirect=true,
                            //RedirectUrl = Url.Action("GetPatientList"),
                            ResponseObject = result,
                           // PopupUrl = Url.Action("PatientCard", new {PatientId = result })
                        };
                    }
                    else
                    {
                        response = new AjaxResponse
                        {
                            Code=404,
                            IsSuccess=false,
                            MessageTitle=MessageConstants.InsertionFailed,
                            MessageClass=MessageConstants.MessageClassWarning
                        };
                    }
                }

            }
            catch (Exception e)
            {
                response = new AjaxResponse
                {
                    Code=500,
                    IsSuccess=false,
                    Message=MessageConstants.InternalError,
                    MessageTitle=MessageConstants.MessageClassError,
                    ResponseObject=e.Message
                };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Add Patient", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> ToggleDiscussionStatus(int patientId)
        {
            //Patient patient = new Patient();

            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (patientId <= 0)
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess = false,
                        Message = "No Data",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning
                    };
                }
                else
                {


                    result = await _PatientBL.ToggleDiscussionStatus(patientId);
                    if (result > 0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            MessageTitle = MessageConstants.SuccessfullInsertion,
                            MessageClass = MessageConstants.MessageClassSuccess,
                            DoFullRedirect = true,
                            //RedirectUrl = Url.Action("GetPatientList"),
                            ResponseObject = result,
                            // PopupUrl = Url.Action("PatientCard", new {PatientId = result })
                        };
                    }
                    else
                    {
                        response = new AjaxResponse
                        {
                            Code = 404,
                            IsSuccess = false,
                            MessageTitle = MessageConstants.InsertionFailed,
                            MessageClass = MessageConstants.MessageClassWarning
                        };
                    }
                }

            }
            catch (Exception e)
            {
                response = new AjaxResponse
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = MessageConstants.InternalError,
                    MessageTitle = MessageConstants.MessageClassError,
                    ResponseObject = e.Message
                };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Add Patient", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> DoesPatientHospitalNumberExists(string hospitalNumber, int patientId)
        {
            //Patient patient = new Patient();

            AjaxResponse response = new AjaxResponse();
           
            try
            {
                if (await _PatientBL.DoesPatientHospitalNumberExists(hospitalNumber, patientId) > 0)
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess = false,
                        Message = "Patient Hospital Number already present.",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass = MessageConstants.MessageClassWarning
                    };
                }
                
            }
            catch (Exception e)
            {
                response = new AjaxResponse
                {
                    Code = 500,
                    IsSuccess = false,
                    Message = MessageConstants.InternalError,
                    MessageTitle = MessageConstants.MessageClassError,
                    ResponseObject = e.Message
                };
            }
            return Json(response);
        }

        [CheckUserRights(MenuName = "Create Visit", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> CreatePatientVisit(int patId,string reason,string mdttype= "")
        {
            Visit visit = new Visit();
            AjaxResponse response = new AjaxResponse();
            visit.PatientId = patId;
            visit.Reason = reason;
            visit.VisitDate = DateTime.Now;
            visit.MDTType = mdttype?.ToString();
            int result = 0;
            try
            {
                if (patId == 0)
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess=false,
                        Message = "No Data Received",
                        MessageTitle = "No Data",
                        MessageClass = MessageConstants.MessageClassWarning,
                        ResponseObject = result
                    };
                }
                else
                {
                    visit.ClinicId = Convert.ToInt32(Session["ClinicId"]);
                    result = await _PatientBL.CreateVisit(visit);
                    if (result>0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            Message = "Saved",
                            MessageTitle = MessageConstants.MessageClassSuccess,
                            ResponseObject = result

                        };
                    }
                    else
                    {
                        response = new AjaxResponse
                        {
                            Code = 404,
                            IsSuccess = false,
                            Message = "failed",
                            MessageTitle = MessageConstants.InsertionFailed,
                            MessageClass=MessageConstants.MessageClassWarning
                        };
                    }
                }
            }
            catch (Exception e)
            {
                response = new AjaxResponse
                {
                    Code=505,
                    IsSuccess=false,
                    Message="Internal Error",
                    MessageTitle=MessageConstants.InternalError,
                    MessageClass=MessageConstants.MessageClassError,
                    ResponseObject=e.Message
                };
            }
            return Json(response);
        }

        public ActionResult ViewFollowupPatientList(DateTime followupDate)
        {

            List<Patient> FollowupPatients = new List<Patient>();


            FollowupPatients = new PatientBL().GetFollowupPatientsForDate(followupDate);


            return View(FollowupPatients);

        }

        #endregion


        #region [AddEditPatientDetail--BoneMarrow]
        public async Task<ActionResult> AddEditPatientDetailBoneMarrow(int patId, string visitDateTime, int visitId = 0, string reason = "")
        {
            Patient Data = new Patient();
            List<PatientAlert> alert = new List<PatientAlert>();

            if (patId != 0 && visitId != 0)
            {
                await Task.Run(() =>
                    Data = _PatientBL.GetPatientById(patId, GetUserLocationIdsCsv())
               );
            }

            alert = _PatientBL.GetPatientActiveAlerts(patId);

            if (Data.DOB.HasValue && Data.DOB.Value.Date != DateTime.MinValue.Date)
            {
                int PatientAgeDays = Convert.ToInt32((DateTime.Now.Date - Data.DOB.Value.Date).TotalDays);

                if (PatientAgeDays >= 365)
                {
                    Data.Age = Math.Round((PatientAgeDays * 1.0) / 365, 2);
                    Data.AgeUnit = "Years";

                }
                else if (PatientAgeDays >= 30 && PatientAgeDays < 365)
                {
                    Data.Age = PatientAgeDays / 30;
                    Data.AgeUnit = "Months";

                }
                else if (PatientAgeDays < 30)
                {
                    Data.Age = PatientAgeDays;
                    Data.AgeUnit = "Days";

                }
            }

            TempData["VisitIdVB"] = visitId;
            TempData["patientIdVB"] = patId;
            TempData["reasonVB"] = reason;
            TempData["VisitDateTimeVB"] = Convert.ToDateTime(visitDateTime ?? DateTime.Now.ToString()).ToString("dd-MMM-yyyy");
            ViewBag.Alerts = alert;

            List<KeyAndValuePair> labTest = await _CommonBL.GetDefinationData("lookups", "System", "Lab");
            ViewBag.LabTestVB = labTest.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id.ToString() }).ToList();

            return View(Data);
        }
        #endregion


        #region [AddEditPatientDetail]
        public async Task<ActionResult> AddEditPatientDetail(int patId, string MDTType,string visitDateTime, int visitId = 0, string reason = "")
        {
            Patient Data = new Patient();
            List<PatientAlert> alert = new List<PatientAlert>();

            if (patId!=0 && visitId!=0)
            {
                await Task.Run(() =>
                    Data = _PatientBL.GetPatientById(patId, GetUserLocationIdsCsv())
               );
            }

            //alert = _PatientBL.GetPatientActiveAlerts(patId);

            if (Data.DOB.HasValue && Data.DOB.Value.Date != DateTime.MinValue.Date)
            {
                int PatientAgeDays = Convert.ToInt32((DateTime.Now.Date - Data.DOB.Value.Date).TotalDays);

                if (PatientAgeDays >= 365)
                {
                    Data.Age = Math.Round((PatientAgeDays * 1.0) / 365, 2);
                    Data.AgeUnit = "Years";

                }
                else if (PatientAgeDays >= 30 && PatientAgeDays < 365)
                {
                    Data.Age = PatientAgeDays / 30;
                    Data.AgeUnit = "Months";
                }
                else if (PatientAgeDays < 30)
                {
                    Data.Age = PatientAgeDays;
                    Data.AgeUnit = "Days";
                }
            }

            TempData["VisitIdVB"] = visitId;
            TempData["patientIdVB"] = patId;
            TempData["reasonVB"] = reason;
            TempData["VisitDateTimeVB"] = Convert.ToDateTime(visitDateTime ?? DateTime.Now.ToString()).ToString("dd-MMM-yyyy");
            TempData["MDTType"] = MDTType;
            //ViewBag.Alerts = alert;

            if(MDTType== "BoneMarrow")
            {
                List<KeyAndValuePair> labTest = await _CommonBL.GetDefinationData("lookups", "System", "Labbmt");
                ViewBag.LabTestVB = labTest.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id.ToString() }).ToList();
            }
            else if (MDTType == "Liver")
            {
                List<KeyAndValuePair> labTest = await _CommonBL.GetDefinationData("lookups", "System", "Lab");
                ViewBag.LabTestVB = labTest.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id.ToString() }).ToList();
            }
            return View(Data);
        }

        public ActionResult PatientHeader(int patId)
        {
            Patient Data = new Patient();
            List<PatientAlert> alert = new List<PatientAlert>();

            if (patId != 0)
            {
                Data = _PatientBL.GetPatientById(patId, GetUserLocationIdsCsv());

            }
            
            if (Data.DOB.HasValue && Data.DOB.Value.Date != DateTime.MinValue.Date)
            {
                int PatientAgeDays = Convert.ToInt32((DateTime.Now.Date - Data.DOB.Value.Date).TotalDays);

                if (PatientAgeDays >= 365)
                {
                    Data.Age = Math.Round((PatientAgeDays * 1.0) / 365, 2);
                    Data.AgeUnit = "Years";

                }
                else if (PatientAgeDays >= 30 && PatientAgeDays < 365)
                {
                    Data.Age = PatientAgeDays / 30;
                    Data.AgeUnit = "Months";

                }
                else if (PatientAgeDays < 30)
                {
                    Data.Age = PatientAgeDays;
                    Data.AgeUnit = "Days";

                }
            }


            TempData["patientIdVB"] = patId;


            return View(Data);
            
        }

        #endregion

        public async Task<ActionResult> GetPatientDetail(int visitId,int patId)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            await Task.Run(() =>
                Data = _PatientBL.GetPatientDetail(visitId, patId)
            );

            List<KeyAndValuePair> Diagnosis = await _CommonBL.GetDefinationData("lookups", "System", "DiagnosisType");
            ViewBag.DiagnosisVB = Diagnosis.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            return View("VitalHistory", Data);
        }


        #region [VitalHistory]
        public async Task <ActionResult> VitalHistory(int visitId,string reason,int patId, string mdtype = "")
        {
            ViewBag.VisitIdVB = visitId;
            ViewBag.VisitTypeVB = reason;
            //ViewBag.VisitDateTimeVB = VisitDateTime;
            List<KeyAndValuePair> CVS = await _CommonBL.GetDefinationData("Lookups", "System", "CVS");
            //select list item MVC internally manage it for dropdown id and text. we no need to make a manual list and convert it in list form
            ViewBag.cvsVB = CVS.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> WeightUnit = await _CommonBL.GetDefinationData("Lookups", "System", "WeightUnit");
            ViewBag.WeightUnitVB = WeightUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> HeightUnit = await _CommonBL.GetDefinationData("Lookups", "System", "HeightUnit");
            ViewBag.HeightUnitVB = HeightUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PresenttHx = await _CommonBL.GetDefinationData("Lookups", "System", "PresentHx");
            ViewBag.PresenttHxVB = PresenttHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PastHx = await _CommonBL.GetDefinationData("Lookups", "System", "PastHx");
            ViewBag.PastHxVB = PastHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> SReponse = await _CommonBL.GetDefinationData("Lookups", "System", "SResponse");
            ViewBag.sresponseVB = SReponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> SLiver = await _CommonBL.GetDefinationData("Lookups", "System", "SLiver");
            ViewBag.sliverVB = SLiver.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> Sspleen = await _CommonBL.GetDefinationData("Lookups", "System", "SSpleen");
            ViewBag.sspleenVB = Sspleen.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PresentHistory = await _CommonBL.GetDefinationData("Lookups", "System", "PHx");
            ViewBag.presentHistoryVB = PresentHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> FamilyHistory = await _CommonBL.GetDefinationData("Lookups", "System", "FHx");
            ViewBag.FamilyHistoryVB = FamilyHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> DrugHistory = await _CommonBL.GetDefinationData("Lookups", "System", "DrugHx");
            ViewBag.DrugHistoryVB = DrugHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SecHistory = await _CommonBL.GetDefinationData("Lookups", "System", "SECHx");
            ViewBag.SecHistoryVB = SecHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> Comorbidities = await _CommonBL.GetDefinationData("Lookups", "System", "Comorbidities");
            ViewBag.ComorbiditiesVB = Comorbidities.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> TransfusionHx = await _CommonBL.GetDefinationData("Lookups", "System", "TransfusionHx");
            ViewBag.TransfusionHxVB = TransfusionHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> CIC = await _CommonBL.GetDefinationData("Lookups", "System", "CIC");
            ViewBag.CICVBB = CIC.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> TestGroup = await _CommonBL.GetDefinationData("Lookups", "System", "TestGroup");
            ViewBag.TestGroup = TestGroup.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> LabTestList = await _CommonBL.GetDefinationData("Lookups", "System", "LabTestList");
            ViewBag.LabTestListVB = LabTestList.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ResultLabel = await _CommonBL.GetDefinationData("Lookups", "System", "ResultLabel");
            ViewBag.ResultLabelVB = ResultLabel.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> Diagnosis = await _CommonBL.GetDefinationData("lookups", "System", "DiagnosisType");
            ViewBag.DiagnosisVB = Diagnosis.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> ChildStatus = await _CommonBL.GetDefinationData("Lookups", "System", "ChildStatus");
            ViewBag.ChildStatusVB = ChildStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> MeldStatus = await _CommonBL.GetDefinationData("Lookups", "System", "MeldStatus");
            ViewBag.MeldStatusVB = MeldStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> BCLC = await _CommonBL.GetDefinationData("Lookups", "System", "BCLCStatus");
            ViewBag.BCLCVB = BCLC.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> ECOGStatus = await _CommonBL.GetDefinationData("Lookups", "System", "ECOGStatus");
            ViewBag.ECOGStatusVB = ECOGStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> HepStatus = await _CommonBL.GetDefinationData("Lookups", "System", "HepStatus");
            ViewBag.HepStatusVB = HepStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> AscitiesStatus = await _CommonBL.GetDefinationData("Lookups", "System", "AscitiesStatus");
            ViewBag.AscitiesStatusVB = AscitiesStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> LargestLesionSize = await _CommonBL.GetDefinationData("Lookups", "System", "LargestLesionSize");
            ViewBag.LargestLesionSizeVB = LargestLesionSize.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> MDTOutCome = await _CommonBL.GetDefinationData("Lookups", "System", "MDTOutCome");
            ViewBag.MDTOutComeVB = MDTOutCome.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();


            List<KeyAndValuePair> SiteOfAngioinvasion = await _CommonBL.GetDefinationData("Lookups", "System", "SiteOfAngioinvasion");
            ViewBag.SiteOfAngioinvasionDropdown = SiteOfAngioinvasion.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> TreatmentResponse = await _CommonBL.GetDefinationData("Lookups", "System", "TreatmentResponse");
            ViewBag.TreatmentResponseVB = TreatmentResponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> RefusalReason = await _CommonBL.GetDefinationData("Lookups", "System", "RefusalReason");
            ViewBag.RefusalReasonVB = RefusalReason.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SitesOfExtraHepaticDisease = await _CommonBL.GetDefinationData("Lookups", "System", "SitesOfExtraHepaticDisease");
            ViewBag.SitesOfExtraHepaticDiseaseVB = SitesOfExtraHepaticDisease.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            //List<KeyAndValuePair> SitesOfExtraHepaticDisease = await _CommonBL.GetDefinationData("Lookups", "System", "SitesOfExtraHepaticDisease");
            //ViewBag.SitesOfExtraHepaticDiseaseVB = SitesOfExtraHepaticDisease.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            
            List<KeyAndValuePair> SVRStatus = await _CommonBL.GetDefinationData("Lookups", "System", "SVRStatus");
            ViewBag.SVRStatusVB = SVRStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<VitalHistory> Data = new List<VitalHistory>();

            //if (Data != null && Data.Count > 0)
            //{
                await Task.Run(() =>
                    {
                        Data = _PatientBL.GetPatientDetail(visitId, patId);
                        if (Data.Count>0)
                        {
                            //Data[0].labreports = _PatientBL.GetLabOrders(visitId);
                            //Data[0].PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
                            Data[0].PatientDiagnosis = _PatientBL.GetPatientDiagnosisAgainstVisit(visitId);
                        }
                    }
                );
            //}
            ViewBag.VisitIdVB = visitId;
            ViewBag.patientIdVB = patId;
            ViewBag.reasonVB = TempData["reasonVB"];
            ViewBag.VisitDateTimeVB = TempData["VisitDateTimeVB"];// Convert.ToDateTime(visitDateTime ?? DateTime.Now.ToString()).ToString("dd -MMM-yyyy");

            return View(Data.FirstOrDefault() ?? new VitalHistory());
        }
        public async Task<ActionResult> GetLabOrders(int visitId = 0)
        {
            List<LabReports> labreports = new List<LabReports>();
            List<KeyAndValuePair> LabTestList = await _CommonBL.GetDefinationData("Lookups", "System", "LabTestList");
            ViewBag.LabTestListVB = LabTestList.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> ResultLabel = await _CommonBL.GetDefinationData("Lookups", "System", "ResultLabel");
            ViewBag.ResultLabelVB = ResultLabel.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ResultUnit = await _CommonBL.GetDefinationData("Lookups", "System", "ResultUnit");
            ViewBag.ResultUnitVB = ResultUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ProtocoName = await _CommonBL.GetDefinationData("Lookups", "System", "ResultUnit");
            ViewBag.ProtocoNameVB = ProtocoName.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            if (visitId > 0)
            {
                await Task.Run(() =>
                {
                    labreports = _PatientBL.GetLabOrders(visitId);
                });
            }
            return View(labreports);
        }

        //public async Task<ActionResult> GetPreviousLabReport(int visitId = 0, int patId=0)
        //{
        //    List<LabReports> labreports = new List<LabReports>();

           

        //    if (visitId > 0)
        //    {
        //        await Task.Run(() =>
        //        {
        //            labreports = _PatientBL.GetPreviousLabReport(visitId);
        //        });
        //    }
        //    return View(labreports.FirstOrDefault());
        //}

        public async Task<ActionResult> GetProtocols(int patId= 0, int visitId = 0)
        {
            //List<Protocols> protocols = new List<Protocols>();
            //if (visitId > 0)
            //{
            //    await Task.Run(() =>
            //    {
            //        protocols = _PatientBL.getProtocols(visitId);
            //    });
            //}
            //return View(protocols);
            
            List<KeyAndValuePair> MedicineType = await _CommonBL.GetDefinationData("Lookups", "System", "MedicineType");
            ViewBag.MedicineTypeVB = MedicineType.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            //List<KeyAndValuePair> DoseUnit = await _CommonBL.GetDefinationData("Lookups", "System", "ResultUnit");
            //ViewBag.DoseUnitVB = DoseUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> Protocol = await _CommonBL.GetDefinationData("Lookups", "System", "Protocols");
            ViewBag.ProtocolVB = Protocol.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> Route = await _CommonBL.GetDefinationData("Lookups", "System", "DouseRoute");
            ViewBag.RouteVB = Route.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> Frequency = await _CommonBL.GetDefinationData("Lookups", "System", "Frequency");
            ViewBag.FrequencyVB = Frequency.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> Duration = await _CommonBL.GetDefinationData("Lookups", "System", "Duration");
            ViewBag.DurationVB = Duration.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> DouseUnit = await _CommonBL.GetDefinationData("Lookups", "System", "DoseUnit");
            ViewBag.DoseUnitVB = DouseUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            ViewBag.Favorites = _PatientBL.GetFavoriteMedicineForUser(SessionHelper.GetUserId());

            List<Protocols> protocols = new List<Protocols>();
            if (visitId > 0)
            {
                await Task.Run(() =>
                {
                    protocols = _PatientBL.GetPatientProtocolDetail(visitId);
                    ViewBag.PreviousProtocols = _PatientBL.GetPatientPreviousProtocols(patId, visitId);
                });
            }
            return View(protocols);
        }
        //ProtocolHistoryAgainstPatient
        public async Task<ActionResult> GetProtocol(string term)
        {

            List<Protocols> Data = await new PatientBL().GetProtocolByName(term);
            //List<KeyAndValuePair> Data = await _CommonBL.GetDefinationData("Lookups", "System", term);
            ////return Json(Data.Select(a => new { value = a.ProtocoName, id = a.ProtocolId.ToString() }), JsonRequestBehavior.AllowGet);

            return Json(Data.Select(a => new { value = a.ProtocoName }), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetService(string term)
        {

            List<PatientServices> Data = await new PatientBL().GetServiceByName(term);


            return Json(Data.Select(a => new { value = a.ServiceName }), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetServiceDetails(string name)
        {
            List<PatientServices> Data = new List<PatientServices>();
            await Task.Run(() =>
            {
                Data = _PatientBL.GetServiceDetails(name);
            });

            return Json(Data.Select(a => new {
                //This is anynomus variable does not required type, this variable must be same on html for mapping (etc ProtocolId,ProtocolName etc)
                ServiceId = Convert.ToDouble(a.ServiceId),
                PatientId = Convert.ToDouble(a.PatientId),
                VisitId = Convert.ToDouble(a.VisitId),
                ServiceName = a.ServiceName.ToString(),
                Price = a.Price.ToString(),
                Quantity = Convert.ToDouble(a.Quantity),
                Amount = Convert.ToDouble(a.Amount),
              

            }), JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> GetProtocolDetail(string name)
        {
            List<Protocols> Data = new List<Protocols>();
            await Task.Run(() =>
            {
                Data = _PatientBL.GetProtocolDetail(name);
            });

            return Json(Data.Select(a=>new {
                //This is anynomus variable does not required type, this variable must be same on html for mapping (etc ProtocolId,ProtocolName etc)
                ProtocolId = a.ProtocolId,
                ProtocolName = a.ProtocoName.ToString(),
                MedicineType = a.MedicineType.ToString(),
                Medicine = a.Medicine.ToString(),
                Dose = a.Dose.ToString(),
                DoseUnit = a.DoseUnit.ToString(),
                Route = a.Route.ToString(),
                Frequency = a.Frequency.ToString(),
                Duration = a.Duration.ToString()

            }),JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> savePatientDetail1(VitalHistory vitalHistory)
        {
            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (vitalHistory == null)
                {
                    response = new AjaxResponse
                    {
                        Code = 200,
                        IsSuccess = false,
                        Message = "No Data",
                        MessageTitle = MessageConstants.NoDataReceived,
                        MessageClass=MessageConstants.MessageClassWarning,
                        ResponseObject = result,
                    };       
                }
                else
                {
                    Users u = Session["User"] as Users;
                    vitalHistory.EnteredBy = u.UserId;
                    vitalHistory.EnteredOn = DateTime.Now;
                    vitalHistory.ClinicId = Convert.ToInt32(Session["ClinicId"]);
                    result = await _PatientBL.insertUpdatePatientDetail(vitalHistory);
                    if (result>0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            Message = "Saved",
                            MessageTitle = MessageConstants.SuccessfullInsertion,
                            MessageClass = MessageConstants.MessageClassSuccess,
                            DoFullRedirect = true,
                            RedirectUrl= Url.Action("GetPatientList", "Patient"),
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
                    Code=505,
                    IsSuccess=false,
                    Message="Internal error",
                    MessageTitle=MessageConstants.InternalError,
                    MessageClass=MessageConstants.MessageClassError,
                    ResponseObject=ex.Message,
                };
                
            }
            finally
            {

            }        
            return Json(response);
        }

        //public ActionResult GetPreviousVisitLabTests(int patientId, int visitId)
        //{
        //    List<LabReports> reports = _PatientBL.GetPreviousVisitLabTests(patientId, visitId);
        //    return View(reports);
        //}

        #endregion

        #region [PatientVisitHisotry]
        [CheckUserRights(MenuName = "Visit History", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> GetVisitHistory(int patId, string patName,string mdtType)
        {
            List<Visit> data = new List<Visit>();
            if (patId>0)
            {
                await Task.Run(() =>
                {
                    data =  _PatientBL.GetVisitHistory(patId,mdtType);
                });
            }
            ViewBag.PatientName = patName;
            return View(data);
        }
        public async Task<ActionResult> ValidateLogin(string loginId,string password)
        {
            Users data = new Users();
            UserBL _user = new UserBL();
            //First initialze with default values
            AjaxResponse res = new AjaxResponse() {
                Code = 403,
                IsSuccess = true,
                Message = "Invalid Username/Password",
                MessageClass = MessageConstants.MessageClassWarning,
                MessageTitle = "Error", 
            };

            if (loginId!=null && password !=null)
            {
                await Task.Run(() =>
                {
                    data = _user.ValidateLogin(loginId, password);
                });
            }
            //if (data!=null) below is short hand to check null and greater value
            if (data?.UserId > 0)
            {
                //!data active mean false value
                if (!data.Active)
                {
                    res.Message = "User is Not Active";
                }
                else
                {
                    Session["User"] = data;
                    //a user can now hold multiple roles (System.UserRole); rights are the union of all assigned roles.
                    //fall back to the legacy single-role lookup if no UserRole rows exist yet for this user.
                    List<int> roleIds = _user.GetUserRoles(data.UserId).Select(a => a.RoleId).ToList();
                    List<RoleRight> Rights = roleIds.Any()
                        ? _user.GetRoleRightsAgainstRoles(roleIds)
                        : _user.GetRoleRightsAgainstRole(data.RoleId);
                    Session["Rights"] = Rights;
                    // Locations (hospital sites) assigned to this user - drives which patients they can see/open.
                    // Empty list means no restriction has been configured for them yet (see GetUserLocationIdsCsv).
                    Session["LocationIds"] = _user.GetUserLocations(data.UserId).Select(a => Convert.ToInt32(a.Id)).ToList();
                    res.Code = 200;
                    res.DoFullRedirect = true;
                    res.IsSuccess = true;
                    res.Message = "User Login Succcessfull";
                    res.MessageClass = MessageConstants.MessageClassSuccess;
                    res.RedirectUrl = Url.Action("GetPatientList", "Patient");
                    res.ResponseObject = data.UserId;
                }
            }
            return Json(res, JsonRequestBehavior.DenyGet);
        }
        public ActionResult UserMenu()
        {
            List<RoleRight> Rights = new List<RoleRight>();
            if (Session["Rights"] != null)
            {
                Rights = Session["Rights"] as List<RoleRight>;
            }
            return View("_UserMenu",Rights);
        }
        #endregion
        [CheckUserRights(MenuName = "Visit History", Order = 1, UserRightType = RightType.View)]
        public async  Task<ActionResult> PatientVisitReport(int patId, int visitId, string reason="")
        {
            Patient Patient = new Patient();
            if (patId != 0 && visitId != 0)
            {
                await Task.Run(() =>
                    Patient = _PatientBL.GetPatientById(patId, GetUserLocationIdsCsv())
               );
            }
            ViewBag.Patient = Patient;

            ViewBag.VisitIdVB = visitId;

            //ViewBag.VisitDateTimeVB = VisitDateTime;
            List<KeyAndValuePair> CVS = await _CommonBL.GetDefinationData("Lookups", "System", "CVS");
            //select list item MVC internally manage it for dropdown id and text. we no need to make a manual list and convert it in list form
            ViewBag.cvsVB = CVS.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> WeightUnit = await _CommonBL.GetDefinationData("Lookups", "System", "WeightUnit");
            ViewBag.WeightUnitVB = WeightUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> HeightUnit = await _CommonBL.GetDefinationData("Lookups", "System", "HeightUnit");
            ViewBag.HeightUnitVB = HeightUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PastHx = await _CommonBL.GetDefinationData("Lookups", "System", "PastHx");
            ViewBag.PastHxVB = PastHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> SReponse = await _CommonBL.GetDefinationData("Lookups", "System", "SResponse");
            ViewBag.sresponseVB = SReponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> SLiver = await _CommonBL.GetDefinationData("Lookups", "System", "SLiver");
            ViewBag.sliverVB = SLiver.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> Sspleen = await _CommonBL.GetDefinationData("Lookups", "System", "SSpleen");
            ViewBag.sspleenVB = Sspleen.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PresentHx = await _CommonBL.GetDefinationData("Lookups", "System", "PHx");
            ViewBag.presentHistoryVB = PresentHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            VitalHistory Data = new VitalHistory();

            //if (Data != null && Data.Count > 0)
            //{
            await Task.Run(() =>
            {
                Data = _PatientBL.GetPatientDetail(visitId, patId).FirstOrDefault();
                if (Data != null)
                {
                    Data.labreports = _PatientBL.GetLabOrders(visitId);

                    Data.PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
                    Data.protocols = _PatientBL.GetPatientProtocolDetail(visitId);
                }
                }
            );
            //}

            ViewBag.VisitIdVB = visitId;
            ViewBag.patientIdVB = patId;
            ViewBag.reasonVB = reason;//  TempData["reasonVB"];

            ViewBag.VisitDateTimeVB = TempData["VisitDateTimeVB"];// Convert.ToDateTime(visitDateTime ?? DateTime.Now.ToString()).ToString("dd -MMM-yyyy");


            List<LabReports> labreports = new List<LabReports>();

            List<KeyAndValuePair> ResultLabel = await _CommonBL.GetDefinationData("Lookups", "System", "ResultLabel");
            ViewBag.ResultLabelVB = ResultLabel.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ResultUnit = await _CommonBL.GetDefinationData("Lookups", "System", "ResultUnit");
            ViewBag.ResultUnitVB = ResultUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            
            //if (visitId > 0)
            //{
            //    await Task.Run(() =>
            //    {
            //        labreports = _PatientBL.GetLabOrders(visitId);
            //    });
               
            //    Data.labreports = labreports;
            //}

            return View(Data);
        }

        private string _mdtType;

        [CheckUserRights(MenuName = "Visit History", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> PatientVisitReportRdlc(int patId, int visitId, string reason = "", string mdtType="")
        {
            globalPatiId = patId;
            globalVisitId = visitId;

            _mdtType = mdtType;
            Warning[] warnings;
            string mimeType;
            string[] streamids;
            string encoding;
            string filenameExtension;

            List<Patient> Patient = new List<Patient>();
            
            List<VitalHistory> Data = new List<VitalHistory>();
            List<VitalHistoryBMT> DataBMT = new List<VitalHistoryBMT>();

            if (patId != 0 && visitId != 0)
            {
                await Task.Run(() =>
                    Patient = _PatientBL.GetPatientByIdForRpt(patId)
               );
            }

            //if (Data != null && Data.Count > 0)
            //{
            await Task.Run(() =>
            {
                if (mdtType == "Liver")
                {
                    Data = _PatientBL.GetPatientDetail(visitId, patId);
                    if (Data != null && Data.Count > 0)
                    {
                        //Data[0].PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
                        //Data[0].labreports = _PatientBL.GetLabOrders(visitId);
                        //Data[0].protocols = _PatientBL.GetPatientProtocolDetail(visitId);
                    }
                }
                else if (mdtType == "BoneMarrow")
                {
                    DataBMT = _PatientBL.GetBMTPatientDetail(visitId, patId);
                    if (DataBMT != null && DataBMT.Count > 0)
                    {
                        //DataBMT[0].PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
                        DataBMT[0].labreports = _PatientBL.GetLabOrdersBMT(visitId);
                        //DataBMT[0].protocols = _PatientBL.GetPatientProtocolDetail(visitId);
                    }
                }

              }
            );

            if (mdtType == "Liver")
            {
                if (Data == null || Data.Count < 1)
                {
                    return RedirectToAction("NoData", new { message = "No Report data found.." });
                }
            }
            else if (mdtType == "BoneMarrow")
            {
                if (DataBMT == null || DataBMT.Count < 1)
                {
                    return RedirectToAction("NoData", new { message = "No Report data found.." });
                }
            }
            LocalReport Report = new LocalReport();
            Report.EnableExternalImages = true;
            
            if (mdtType == "Liver")
            {
        
                Report.ReportPath = Server.MapPath("~/Reports/Patient.rdlc");
                Report.DataSources.Add(new ReportDataSource("PreviousLabReports", Data[0].PreviousLab));
                Report.DataSources.Add(new ReportDataSource("CurrentLabs", Data[0].labreports));
                Report.DataSources.Add(new ReportDataSource("Protocols", Data[0].protocols));
                Report.DataSources.Add(new ReportDataSource("Patient", Patient));
                Report.DataSources.Add(new ReportDataSource("PatientDetail", Data));
                Report.SetParameters(new ReportParameter("PatientId", patId.ToString()));
                Report.SetParameters(new ReportParameter("VisitId", visitId.ToString()));
                Report.SetParameters(new ReportParameter("Reason", reason));
                string imagePath = new Uri(Server.MapPath(Patient[0].ImageURL)).AbsoluteUri;
                ReportParameter p = new ReportParameter("ImagePath", imagePath);

                Report.SetParameters(p);
                Report.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
            }
            else if (mdtType == "BoneMarrow")
            {
                Report.ReportPath = Server.MapPath("~/Reports/PatientMaster.rdlc");
                Report.DataSources.Add(new ReportDataSource("dsPatientMaster", Patient));
                Report.DataSources.Add(new ReportDataSource("dsPatientChild", DataBMT));

                Report.SetParameters(new ReportParameter("PatientId", patId.ToString()));
                Report.SetParameters(new ReportParameter("VisitId", visitId.ToString()));

                //string imagePath = new Uri(Server.MapPath(Patient[0].ImageURL)).AbsoluteUri;
                //ReportParameter p = new ReportParameter("ImagePath", imagePath);
                // Report.SetParameters(p);
                Report.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
            }
            //Report.Refresh();
            
            var bytes = Report.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
            return new FileContentResult(bytes, mimeType);
        }
        private int globalPatiId;
        private int globalVisitId;
        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {
            
            if (_mdtType == "Liver")
            {
                int PatientId = Convert.ToInt32(e.Parameters["PatientId"].Values[0]);
                int VisitId = Convert.ToInt32(e.Parameters["VisitId"].Values[0]);

                string dataSourceName = e.DataSourceNames[0];

                List<VitalHistory> Data = new List<VitalHistory>();
                Data = new PatientBL().GetPatientDetail(VisitId, PatientId);

                List<PatientDiagnosis> Diagnosis = _PatientBL.GetPatientDiagnosisAgainstVisit(VisitId);
                if (Diagnosis != null && Diagnosis.Any())
                {
                    Diagnosis = Diagnosis.Where(a => a.IsActive == true).ToList();
                }

                List<PatientAlert> Alerts = new List<PatientAlert>();// _PatientBL.GetPatientActiveAlerts(PatientId);

                e.DataSources.Add(new ReportDataSource("PatientDetail", Data));
                e.DataSources.Add(new ReportDataSource("PreviousLabReports", new PatientBL().GetPreviousLabReport(VisitId)));
                e.DataSources.Add(new ReportDataSource("CurrentLabs", new PatientBL().GetLabOrders(VisitId)));
                e.DataSources.Add(new ReportDataSource("PreviousVisitsLabs", new List<LabReports>()));
                //new PatientBL().GetLabOrdersAgainstPreviousVisits(VisitId).Where(a => a.VisitId == VisitId).ToList()));) ;
                e.DataSources.Add(new ReportDataSource("Protocols", new List<Protocols>()));// new PatientBL().GetPatientProtocolDetail(VisitId)));
                e.DataSources.Add(new ReportDataSource("Patient", new PatientBL().GetPatientByIdForRpt(PatientId)));
                e.DataSources.Add(new ReportDataSource("PatientDiagnosis", Diagnosis));
                e.DataSources.Add(new ReportDataSource("PatientAlerts", Alerts));
            }
            else if (_mdtType == "BoneMarrow")
            {
                //int PatientId = 14326;//

                //int VisitId = 18442;//


                int PatientId = Convert.ToInt32(e.Parameters["PatientId"].Values[0]);
                int VisitId = Convert.ToInt32(e.Parameters["VisitId"].Values[0]);

                //int globalPatId = patId;
                //int globalVisitId = visitId;


                string dataSourceName = e.DataSourceNames[0];

                List<VitalHistoryBMT> DataBMT = new List<VitalHistoryBMT>();
                DataBMT = new PatientBL().GetBMTPatientDetail(VisitId, PatientId);

                List<PatientDiagnosisBMT> DiagnosisBMT = _PatientBL.GetPatientDiagnosisAgainstVisitBMT(VisitId);
                if (DiagnosisBMT != null && DiagnosisBMT.Any())
                {
                    DiagnosisBMT = DiagnosisBMT.Where(a => a.IsActive == true).ToList();
                }
                //List<PatientAlert> Alerts = _PatientBL.GetPatientActiveAlerts(PatientId);
                if (DataBMT!=null && DataBMT.Count > 0) 
                {
                    DataBMT.ForEach(a => a.OtherComorbitiesString = string.Join(",", a.OtherComorbitiesBMT));
                        e.DataSources.Add(new ReportDataSource("dsPatientChild", DataBMT));

                    DataBMT.ForEach(a => a.SiteOfLymphNodesVB = string.Join(",", a.SiteOfLymphnodes));
                        e.DataSources.Add(new ReportDataSource("dsPatientChild", DataBMT));

                    DataBMT.ForEach(a => a.PreviousTreatmentBMTSring = string.Join(",", a.PreviousTreatmentBMT)); 
                    e.DataSources.Add(new ReportDataSource("dsPatientChild", DataBMT));

                    //foreach (var item in DataBMT)
                    //{
                    //    if (item.PreviousTreatmentBMT!=null)
                    //    {
                    //        item.PreviousTreatmentBMTSring = string.Join(",", item.PreviousTreatmentBMT);

                    //    }
                    //    e.DataSources.Add(new ReportDataSource("dsPatientChild", DataBMT));
                    //}
                    DataBMT.ForEach(a => a.MDTOutComeString = string.Join(",", a.MDTOutCome));
                    e.DataSources.Add(new ReportDataSource("dsPatientChild", DataBMT));
                }
                //e.DataSources.Add(new ReportDataSource("PreviousLabReports", new PatientBL().GetPreviousLabReport(VisitId)));
                e.DataSources.Add(new ReportDataSource("dsLabResults", new PatientBL().GetLabOrdersBMT(VisitId)));
                //e.DataSources.Add(new ReportDataSource("PreviousVisitsLabs", new PatientBL().GetLabOrdersAgainstPreviousVisits(VisitId).Where(a => a.VisitId == VisitId).ToList()));
                //e.DataSources.Add(new ReportDataSource("Protocols", new PatientBL().GetPatientProtocolDetail(VisitId)));
                //e.DataSources.Add(new ReportDataSource("Patient", new PatientBL().GetPatientByIdForRpt(PatientId)));
                e.DataSources.Add(new ReportDataSource("dsPatientDiagnosis", DiagnosisBMT));
                //e.DataSources.Add(new ReportDataSource("PatientAlerts", Alerts));
            }
        }


        void PreviousResult_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {

            int PatientId = Convert.ToInt32(e.Parameters["PatientId"].Values[0]);
            int VisitId = Convert.ToInt32(e.Parameters["VisitId"].Values[0]);
            string dataSourceName = e.DataSourceNames[0];
            List<VitalHistoryBMT> DataBMT=new List<VitalHistoryBMT>();
            //DataBMT = new PatientBL().GetBMTPatientDetail(VisitId, PatientId);
            //List<VitalHistory> Data = new List<VitalHistory>();
            //Data = new PatientBL().GetPatientDetail(VisitId, PatientId);
            //DataBMT = new PatientBL().GetBMTPatientDetail(VisitId, PatientId);
            //e.DataSources.Add(new ReportDataSource("PreviousLabReports", new PatientBL().GetPreviousLabReport(VisitId)));
        }

        [CheckUserRights(MenuName = "Add Patient", Order = 1, UserRightType = RightType.View)]
        public ActionResult PatientCard(int PatientId)
        {
            Patient pat = new Patient();
            if (PatientId> 0)
            {
                pat = _PatientBL.GetPatientById(PatientId, GetUserLocationIdsCsv());
            }

            return View(pat);
        }
        [CheckUserRights(MenuName = "Create Visit", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> CopyVisit(int patientId, int oldVisitId,string mdtType)
        {
            Visit visit = new Visit();
            AjaxResponse response = new AjaxResponse();
            visit.PatientId = patientId;
            visit.Reason = "Followup";
            visit.MDTType = mdtType;
            visit.VisitDate = DateTime.Now;
            int result = 0;

            result = await _PatientBL.CreateVisit(visit);

            if (result > 0)
            {

                int a = await _PatientBL.CopyVisitData(oldVisitId, result);

                response = new AjaxResponse
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "Saved",
                    MessageTitle = MessageConstants.MessageClassSuccess,
                    ResponseObject = result

                };
            }
            else
            {
                response = new AjaxResponse
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = "failed",
                    MessageTitle = MessageConstants.InsertionFailed,
                    MessageClass = MessageConstants.MessageClassWarning
                };
            }

            return Json(response);
        }
        [HttpPost]
        public async Task<ActionResult> CopyVisitBMT(int patientId, int oldVisitId,string mdtType)
        {
            Visit visit = new Visit();
            AjaxResponse response = new AjaxResponse();
            visit.PatientId = patientId;
            visit.Reason = "Followup";
            visit.VisitDate = DateTime.Now;
            visit.MDTType = mdtType;
            int result = 0;

            result = await _PatientBL.CreateVisit(visit);

            if (result > 0)
            {

                int a = await _PatientBL.CopyVisitDataBMT(oldVisitId, result);

                response = new AjaxResponse
                {
                    Code = 200,
                    IsSuccess = true,
                    Message = "Saved",
                    MessageTitle = MessageConstants.MessageClassSuccess,
                    ResponseObject = result

                };
            }
            else
            {
                response = new AjaxResponse
                {
                    Code = 404,
                    IsSuccess = false,
                    Message = "failed",
                    MessageTitle = MessageConstants.InsertionFailed,
                    MessageClass = MessageConstants.MessageClassWarning
                };
            }

            return Json(response);
        }
        #region SMS
        [CheckUserRights(MenuName = "SMS", Order = 1, UserRightType = RightType.AddEdit)]
        public ActionResult SendSMSForFollowUp(DateTime followupDate)
        {
            List<Patient> FollowupPatients = new List<Patient>();


            FollowupPatients = new PatientBL().GetFollowupPatientsForDate(followupDate);


            if (FollowupPatients != null && FollowupPatients.Any())
            {
                foreach (Patient pat in FollowupPatients)
                {
                    string Message = $"Dear {pat.FullName}, This is to remind you about the follow up visit to our Clinic on {followupDate.ToString("dd-MMM")}. Kindly come on time. Thank you..";

                    SMSHelper.SendSmsMessage(Message, pat.MobileNumber, "");
                }
            }

            return View();

        }

        [HttpPost]
        public ActionResult UploadVaccinationSMS(DateTime VaccinationDueDate, HttpPostedFileBase smsFile)
        {
            //Create a new DataTable.
            //DataTable dt = new DataTable();
            int RowCount = 0;

            if (smsFile != null && smsFile.ContentLength > 0)
            {
                using (XLWorkbook workBook = new XLWorkbook(smsFile.InputStream))
                {
                    //Read the first Sheet from Excel file.
                    IXLWorksheet workSheet = workBook.Worksheet(1);

                  
                    //Loop through the Worksheet rows.
                    bool firstRow = true;
                    foreach (IXLRow row in workSheet.Rows())
                    {
                        //Use the first row to add columns to DataTable.
                        if (firstRow)
                        {
                            
                            firstRow = false;
                        }
                        else
                        {

                            if (string.IsNullOrEmpty(row.Cell(1).Value.ToString()))
                            {
                                continue;
                            }

                            //Insert record in DB 
                            //_dal.InsertSMSDataForVaccination(row.Cell(1).Value.ToString(), row.Cell(2).Value.ToString(), row.Cell(3).Value.ToString(), VaccinationDueDate);
                            RowCount++;
                            //Add rows to DataTable.
                            //dt.Rows.Add();
                            //int i = 0;
                            //foreach (IXLCell cell in row.Cells())
                            //{

                            //    dt.Rows[dt.Rows.Count - 1][i] = cell.Value.ToString();
                            //    i++;
                            //}
                        }
                    }
                }
            }



            ViewBag.RowCount = RowCount;

            return RedirectToAction("VaccinationSMS");
        }

        public ActionResult GetTestsAgainstGroupName(string groupName)
        {
            List<LabReports> Reports = _PatientBL.GetLabTestForGroupName(groupName);

            return Json(Reports, JsonRequestBehavior.DenyGet);
        }

        [CheckUserRights(MenuName = "Add Lookup", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> AddEditLookupData()
        {
            List<KeyAndValuePair> LookupData = await _CommonBL.GetDefinationData("GetAllLookups", "System", "");
            
            return View(LookupData);
        }

        [CheckUserRights(MenuName = "Add Lookup", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> InsertLookupData(string LookupValue, string ReturnValue, string Category)
        {

            AjaxResponse response = new AjaxResponse();

            int result = 0;
            try
            {
                if (string.IsNullOrEmpty(LookupValue) || string.IsNullOrEmpty(Category))
                {
                    response = new AjaxResponse
                    {
                        Code = 403,
                        IsSuccess = false,
                        Message = "No Data Received",
                        MessageTitle = "No Data",
                        MessageClass = MessageConstants.MessageClassWarning,
                        ResponseObject = result
                    };
                }
                else
                {
                    result = await _CommonBL.InsertLookupData(LookupValue, ReturnValue, Category);

                    if (result > 0)
                    {
                        response = new AjaxResponse
                        {
                            Code = 200,
                            IsSuccess = true,
                            Message = "Saved",
                            MessageTitle = MessageConstants.MessageClassSuccess,
                            ResponseObject = result

                        };
                    }
                    else
                    {
                        response = new AjaxResponse
                        {
                            Code = 404,
                            IsSuccess = false,
                            Message = "failed",
                            MessageTitle = MessageConstants.InsertionFailed,
                            MessageClass = MessageConstants.MessageClassWarning
                        };
                    }
                }
            }
            catch (Exception e)
            {
                response = new AjaxResponse
                {
                    Code = 505,
                    IsSuccess = false,
                    Message = "Internal Error",
                    MessageTitle = MessageConstants.InternalError,
                    MessageClass = MessageConstants.MessageClassError,
                    ResponseObject = e.Message
                };
            }

            return Json(response);
        }

        public ActionResult AddPatientServices(int patientId, int visitId)
        {

            ViewBag.PatientId = patientId;
            ViewBag.VisitId = visitId;

            Patient Data = new Patient();
            List<PatientServices> list = new List<PatientServices>();

            if (patientId!= 0 && visitId != 0)
            {
                Data = _PatientBL.GetPatientById(patientId, GetUserLocationIdsCsv());
                list = _PatientBL.GetPatientServiceAgainstVisit(visitId);
            }

            ViewBag.Patient = Data;
            
            return View(list);
        }

        [CheckUserRights(MenuName = "Add Patient Services", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> savePatientServiceDetail(List<PatientServices> patientServices)
        {
            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (patientServices == null)
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
                    result = await _PatientBL.insertUpdatePatientServiceDetail(patientServices);
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
                            RedirectUrl = Url.Action("GetPatientList", "Patient"),
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
        #endregion
        public ActionResult GetUserClinics(int userId)
        {
            List<KeyAndValuePair> list = new UserBL().GetUserClinics(userId);
            return Json(list, JsonRequestBehavior.DenyGet);
        }
        public ActionResult SaveClinicInSession(int clinicId)
        {

            Session["ClinicId"] = clinicId;
            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Saved",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
                RedirectUrl= Url.Action("GetPatientList", "Patient", new { PreloadList = "Yes"}),
            };

            
            return Json(res, JsonRequestBehavior.DenyGet);
        }
        public async Task<ActionResult> DeleteProtocolDetail(int id)
        {
            int Deleted = await _PatientBL.DeleteProtocolDetail(id);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Deleted",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };


            return Json(res, JsonRequestBehavior.DenyGet);
        }
        public ActionResult GetMedicineAndFavorite(string term)
        {
            int UserId = SessionHelper.GetUserId();
            List<Protocols> Data = new PatientBL().SearchMedicineAndFavorites(term,UserId);

            return Json(Data, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> AddMedicineToFavorite(Protocols protocol)
        {
            int UserId = SessionHelper.GetUserId();
            protocol.EnteredBy = UserId;
            int Result = await _PatientBL.InsertMedicineFavorite(protocol);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Added to Favorite",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };

            return Json(res, JsonRequestBehavior.DenyGet);

        }
        public async Task<ActionResult> DeleteFavoriteMedicine( string medicineName, string medicineType)
        {
            int UserId = SessionHelper.GetUserId();

            int Result = await _PatientBL.DeleteFavoritemMedicine(UserId, medicineName, medicineType);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Medicine removed from Favorite",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };

            return Json(res, JsonRequestBehavior.DenyGet);

        }
        [HttpPost]
        public async Task<ActionResult> MarkPatientDiagnosisAsInactive(int patientDiagnosisId)
        {
            
            int Result = await _PatientBL.MarkPatientDiagnosisAsInactive(patientDiagnosisId);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Diagnosis Marked as Inactive",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };

            return Json(res, JsonRequestBehavior.DenyGet);

        }
        public async Task<ActionResult> AddPatientAlert(PatientAlert alert)
        {
            int UserId = SessionHelper.GetUserId();
            alert.EnteredBy = UserId;
            alert.IsActive = true;
            int Result = await _PatientBL.InsertPatientAlert(alert);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Patient Alert Added",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };

            return Json(res, JsonRequestBehavior.DenyGet);
        }
        [HttpPost]
        public async Task<ActionResult> MarkPatientAlertAsInactive(int patientAlertId)
        {

            int Result = await _PatientBL.MarkPatientAlertAsInactive(patientAlertId);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Alert Marked as Inactive",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };

            return Json(res, JsonRequestBehavior.DenyGet);

        }
        [HttpPost]
        public async Task<ActionResult> DiscontinueMedicine(int protocolDetailId)
        {

            int Result = await _PatientBL.DiscontinueMedicine(protocolDetailId);

            //First initialze with default values
            AjaxResponse res = new AjaxResponse()
            {
                Code = 200,
                IsSuccess = true,
                Message = "Medicine Marked as Discontinued",
                MessageClass = MessageConstants.MessageClassSuccess,
                MessageTitle = "Success",
            };

            return Json(res, JsonRequestBehavior.DenyGet);

        }
        public async Task<ActionResult> PatientVisitAdvanceSearch()
        {

    
            List<KeyAndValuePair> Diagnosis = await _CommonBL.GetDefinationData("lookups", "System", "DiagnosisType");
            ViewBag.DiagnosisVB = Diagnosis.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> ChildStatus = await _CommonBL.GetDefinationData("Lookups", "System", "ChildStatus");
            ViewBag.ChildStatusVB = ChildStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> MeldStatus = await _CommonBL.GetDefinationData("Lookups", "System", "MeldStatus");
            ViewBag.MeldStatusVB = MeldStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> BCLC = await _CommonBL.GetDefinationData("Lookups", "System", "BCLCStatus");
            ViewBag.BCLCVB = BCLC.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ECOGStatus = await _CommonBL.GetDefinationData("Lookups", "System", "ECOGStatus");
            ViewBag.ECOGStatusVB = ECOGStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> HepStatus = await _CommonBL.GetDefinationData("Lookups", "System", "HepStatus");
            ViewBag.HepStatusVB = HepStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> AscitiesStatus = await _CommonBL.GetDefinationData("Lookups", "System", "AscitiesStatus");
            ViewBag.AscitiesStatusVB = AscitiesStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> LargestLesionSize = await _CommonBL.GetDefinationData("Lookups", "System", "LargestLesionSize");
            ViewBag.LargestLesionSizeVB = LargestLesionSize.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> MDTOutCome = await _CommonBL.GetDefinationData("Lookups", "System", "MDTOutCome");
            ViewBag.MDTOutComeVB = MDTOutCome.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> SiteOfAngioinvasion = await _CommonBL.GetDefinationData("Lookups", "System", "SiteOfAngioinvasion");
            ViewBag.SiteOfAngioinvasionDropdown = SiteOfAngioinvasion.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> TreatmentResponse = await _CommonBL.GetDefinationData("Lookups", "System", "TreatmentResponse");
            ViewBag.TreatmentResponseVB = TreatmentResponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> RefusalReason = await _CommonBL.GetDefinationData("Lookups", "System", "RefusalReason");
            ViewBag.RefusalReasonVB = RefusalReason.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SitesOfExtraHepaticDisease = await _CommonBL.GetDefinationData("Lookups", "System", "SitesOfExtraHepaticDisease");
            ViewBag.SitesOfExtraHepaticDiseaseVB = SitesOfExtraHepaticDisease.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SVRStatus = await _CommonBL.GetDefinationData("Lookups", "System", "SVRStatus");
            ViewBag.SVRStatusVB = SVRStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<VitalHistory> Data = new List<VitalHistory>();

            return View();
        }
        public async Task<ActionResult> PatientVisitAdvanceSearchResult(int? ChildStatus, int? MeldStatus, int? BCLCStatus, int? ECOG, int? TreatmentResponse,
        int? MdtOutcome, int? HepatitisStatus, int? AscitiesStatus, int? Svr, int? OtherComorbidities, int? AngioInvasion, double? AFP, bool LiverCirrhosis, bool PortalHypertension,bool Varices,bool MeetingMilan,bool UCSF,bool ASAN)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            Data = _PatientBL.GetVisitListAgainstSearch(ChildStatus, MeldStatus, BCLCStatus, ECOG, TreatmentResponse,
            MdtOutcome, HepatitisStatus, AscitiesStatus, Svr, OtherComorbidities, AngioInvasion, AFP, LiverCirrhosis, PortalHypertension, Varices, MeetingMilan, UCSF, ASAN);



            return View(Data);
        }
        public async Task<ActionResult> PatientListForMDT(DateTime? fromDate, DateTime? toDate)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            fromDate = fromDate ?? DateTime.Now;
            toDate = toDate ?? DateTime.Now;

            Data = _PatientBL.GetPatientListForMDTList(fromDate.Value, toDate.Value, GetUserLocationIdsCsv());

            ViewBag.FromDate = fromDate.Value;
            ViewBag.ToDate = toDate.Value;


            return View(Data);
        }
        public ActionResult ExportPatientListForMDT(DateTime? fromDate, DateTime? toDate)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            fromDate = fromDate ?? DateTime.Now;
            toDate = toDate ?? DateTime.Now;

            Data = _PatientBL.GetPatientListForMDTList(fromDate.Value, toDate.Value, GetUserLocationIdsCsv());

            DataTable dt = ToDataTable<VitalHistory>(Data);

            dt.Columns.Remove("EnteredOn");
            dt.Columns.Remove("PatientId");
            dt.Columns.Remove("VisitType");
          
            dt.Columns.Remove("CIC");
            dt.Columns.Remove("EnteredBy");
            dt.Columns.Remove("PatientDetailId");
            dt.Columns.Remove("VisitId");

            dt.Columns.Remove("MeldStatus");
            dt.Columns.Remove("BCLC");
            dt.Columns.Remove("ECOGStatus");
            dt.Columns.Remove("HepStatus");
            dt.Columns.Remove("AscitiesStatus");
            dt.Columns.Remove("OtherComorbities");
            dt.Columns.Remove("CTReportConclusion");
            dt.Columns.Remove("Angioinvasion");
            dt.Columns.Remove("SiteOfAngioinvasionVB");
            dt.Columns.Remove("ExtraHepaticDisease");
            dt.Columns.Remove("SiteOfHepaticDisease");
            dt.Columns.Remove("LiverCirrhosis");
            dt.Columns.Remove("PortalHypertension");
            dt.Columns.Remove("Varices");
            dt.Columns.Remove("SVRStatus");
            dt.Columns.Remove("PreviousTreatment");
            dt.Columns.Remove("TreatmentType");
            dt.Columns.Remove("TreatmentResponse");
            dt.Columns.Remove("DiagnosisName");
            dt.Columns.Remove("DiagnosisDetails");
            dt.Columns.Remove("MeetingMilan");
            dt.Columns.Remove("UCSF");
            dt.Columns.Remove("ASAN");
            dt.Columns.Remove("other");
            dt.Columns.Remove("otherDetail");
            dt.Columns.Remove("PtWillingForTransplant");
            dt.Columns.Remove("ReasonOfRefusal");
            dt.Columns.Remove("MDTOutCome");
            dt.Columns.Remove("ClinicId");
            dt.Columns.Remove("PreviousLab");
            dt.Columns.Remove("labreports");
            dt.Columns.Remove("protocols");
            dt.Columns.Remove("PatientDiagnosis");
            dt.Columns.Remove("OtherComorbitiesString");
            dt.Columns.Remove("Temp");
            dt.Columns.Remove("ChildStatusValue");
            dt.Columns.Remove("MeldStatusValue");
            dt.Columns.Remove("BCLCValue");
            dt.Columns.Remove("ECOGStatusValue");
            dt.Columns.Remove("TreatmentResponseValue");
            dt.Columns.Remove("HepStatusValue");
            dt.Columns.Remove("AscitiesStatusValue");
            dt.Columns.Remove("SVRStatusValue");
            dt.Columns.Remove("SiteOfAngioinvasion");
            dt.Columns.Remove("NumberOfLesionValue");
            dt.Columns.Remove("MDTOutComeOther");
            dt.Columns.Remove("TransplantDiscussion");
            dt.Columns.Remove("TransplantDiscussionDetail");
            dt.Columns.Remove("ImagingDate"); 
                    //public string PatientName { get; set; }
        //public string MDTOutComeString { get; set; }
        //public string HospitalNumber { get; set; }


        //Name of File  
        string fileName = "MDT Patient.xlsx";
            using (XLWorkbook wb = new XLWorkbook())
            {
                //Add DataTable in worksheet  
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    //Return xlsx Excel File  
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        private  DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Defining type of data column gives proper data table 
                var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, type);
            }
            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }
            //put a breakpoint here and check datatable
            return dataTable;
        }
        public ActionResult NoData(string message)
        {
            ViewBag.Message = message;
            return View();
        }
        #region [VitalHisotry_BoneMarrow_MDT]
        public async Task<ActionResult> VitalHistoryBMT(int visitId, string reason, int patId,string mdtype="")
        {
            ViewBag.VisitIdVB = visitId;
            ViewBag.VisitTypeVB = reason;
            //ViewBag.VisitDateTimeVB = VisitDateTime;
            List<KeyAndValuePair> CVS = await _CommonBL.GetDefinationData("Lookups", "System", "CVS");
            //select list item MVC internally manage it for dropdown id and text. we no need to make a manual list and convert it in list form
            ViewBag.cvsVB = CVS.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> WeightUnit = await _CommonBL.GetDefinationData("Lookups", "System", "WeightUnit");
            ViewBag.WeightUnitVB = WeightUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> HeightUnit = await _CommonBL.GetDefinationData("Lookups", "System", "HeightUnit");
            ViewBag.HeightUnitVB = HeightUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PresenttHx = await _CommonBL.GetDefinationData("Lookups", "System", "PresentHx");
            ViewBag.PresenttHxVB = PresenttHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PastHx = await _CommonBL.GetDefinationData("Lookups", "System", "PastHx");
            ViewBag.PastHxVB = PastHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> SReponse = await _CommonBL.GetDefinationData("Lookups", "System", "SResponse");
            ViewBag.sresponseVB = SReponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> SLiver = await _CommonBL.GetDefinationData("Lookups", "System", "SLiver");
            ViewBag.sliverVB = SLiver.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> Sspleen = await _CommonBL.GetDefinationData("Lookups", "System", "SSpleen");
            ViewBag.sspleenVB = Sspleen.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> PresentHistory = await _CommonBL.GetDefinationData("Lookups", "System", "PHx");
            ViewBag.presentHistoryVB = PresentHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> FamilyHistory = await _CommonBL.GetDefinationData("Lookups", "System", "FHx");
            ViewBag.FamilyHistoryVB = FamilyHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> DrugHistory = await _CommonBL.GetDefinationData("Lookups", "System", "DrugHx");
            ViewBag.DrugHistoryVB = DrugHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SecHistory = await _CommonBL.GetDefinationData("Lookups", "System", "SECHx");
            ViewBag.SecHistoryVB = SecHistory.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> Comorbidities = await _CommonBL.GetDefinationData("Lookups", "System", "ComorbiditiesBMT");
            ViewBag.ComorbiditiesVB = Comorbidities.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> TransfusionHx = await _CommonBL.GetDefinationData("Lookups", "System", "TransfusionHx");
            ViewBag.TransfusionHxVB = TransfusionHx.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<KeyAndValuePair> CIC = await _CommonBL.GetDefinationData("Lookups", "System", "CIC");
            ViewBag.CICVBB = CIC.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> TestGroup = await _CommonBL.GetDefinationData("Lookups", "System", "TestGroup");
            ViewBag.TestGroup = TestGroup.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> LabTestList = await _CommonBL.GetDefinationData("Lookups", "System", "LabTestList");
            ViewBag.LabTestListVB = LabTestList.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ResultLabel = await _CommonBL.GetDefinationData("Lookups", "System", "ResultLabel");
            ViewBag.ResultLabelVB = ResultLabel.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> Diagnosis = await _CommonBL.GetDefinationData("lookups", "System", "DiagnosisTypeBMT");
            ViewBag.DiagnosisVB = Diagnosis.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();
            //Changes
            List<KeyAndValuePair> DiseaseStatus = await _CommonBL.GetDefinationData("lookups", "System", "DiseaseStatusBMT");
            ViewBag.DiseaseStatusVB = DiseaseStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> Lansky= await _CommonBL.GetDefinationData("Lookups", "System", "LanskyBMT");
            ViewBag.LanskyVB = Lansky.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> HCTCI= await _CommonBL.GetDefinationData("Lookups", "System", "HCTCIBMT");
            ViewBag.HCTCIVB = HCTCI.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> ECOGStatus = await _CommonBL.GetDefinationData("Lookups", "System", "ECOGStatus");
            ViewBag.ECOGStatusVB = ECOGStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> HepB = await _CommonBL.GetDefinationData("Lookups", "System", "HEBBMT");
            ViewBag.HepBVB= HepB.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> HepC = await _CommonBL.GetDefinationData("Lookups", "System", "HEPCBMT");
            ViewBag.HepCVB= HepC.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> HIV= await _CommonBL.GetDefinationData("Lookups", "System", "HIVBMT");
            ViewBag.HIVVB = HIV.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> Stage = await _CommonBL.GetDefinationData("Lookups", "System", "StageBMT");
            ViewBag.StageVB = Stage.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();
            //Changes
            List<KeyAndValuePair> RiskScore = await _CommonBL.GetDefinationData("Lookups", "System", "RiskScoreBMT");
            ViewBag.RiskScoreVB = RiskScore.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();
            //Changes
            List<KeyAndValuePair> Chelation= await _CommonBL.GetDefinationData("Lookups", "System", "ChelationBMT");
            ViewBag.ChelationVB = Chelation.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            //Changes
            List<KeyAndValuePair> SiteOfLymphNodes= await _CommonBL.GetDefinationData("Lookups", "System", "SiteOfLymphNodesBMT");
            ViewBag.SiteOfLymphNodesVB = SiteOfLymphNodes.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value}).ToList();
            //Changes
            List<KeyAndValuePair> PreviousTreatmentTypeBMT = await _CommonBL.GetDefinationData("Lookups", "System", "PreviousTreatmentTypeBMT");
            ViewBag.PreviousTreatmentTypeBMTVB = PreviousTreatmentTypeBMT.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value}).ToList();
            //Changes
            List<KeyAndValuePair> TreatmentTypeBMT = await _CommonBL.GetDefinationData("Lookups", "System", "TreatmentTypeBMT");
            ViewBag.TreatmentTypeBMTVB = TreatmentTypeBMT.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value}).ToList();

            List<KeyAndValuePair> MDTOutCome = await _CommonBL.GetDefinationData("Lookups", "System", "MDTOutComesBMT");
            ViewBag.MDTOutComeVB = MDTOutCome.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> SiteOfAngioinvasion = await _CommonBL.GetDefinationData("Lookups", "System", "SiteOfAngioinvasion");
            ViewBag.SiteOfAngioinvasionDropdown = SiteOfAngioinvasion.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

            List<KeyAndValuePair> TreatmentResponse = await _CommonBL.GetDefinationData("Lookups", "System", "TreatmentResponse");
            ViewBag.TreatmentResponseVB = TreatmentResponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> RefusalReason = await _CommonBL.GetDefinationData("Lookups", "System", "RefusalReason");
            ViewBag.RefusalReasonVB = RefusalReason.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SitesOfExtraHepaticDisease = await _CommonBL.GetDefinationData("Lookups", "System", "SitesOfExtraHepaticDisease");
            ViewBag.SitesOfExtraHepaticDiseaseVB = SitesOfExtraHepaticDisease.Select(a => new SelectListItem() { Text = a.Value, Value = a.Value }).ToList();

           // List<KeyAndValuePair> MDTOutComes = await _CommonBL.GetDefinationData("Lookups", "System", "MDTOutCome");
           // ViewBag.MDTOutComeVB = MDTOutComes.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SVRStatus = await _CommonBL.GetDefinationData("Lookups", "System", "SVRStatus");
            ViewBag.SVRStatusVB = SVRStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            List<VitalHistoryBMT> Data = new List<VitalHistoryBMT>();

            //if (Data != null && Data.Count > 0)
            //{
            await Task.Run(() =>
            {
                Data = _PatientBL.GetBMTPatientDetail(visitId, patId);
                if (Data.Count > 0)
                {
                    //Data[0].labreports = _PatientBL.GetLabOrders(visitId);
                    //Data[0].PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
                    Data[0].PatientDiagnosis = _PatientBL.GetPatientDiagnosisAgainstVisitBMT(visitId);

                }
            }
            );
            //}

            ViewBag.VisitIdVB = visitId;
            ViewBag.patientIdVB = patId;
            ViewBag.reasonVB = TempData["reasonVB"];
            ViewBag.VisitDateTimeVB = TempData["VisitDateTimeVB"];// Convert.ToDateTime(visitDateTime ?? DateTime.Now.ToString()).ToString("dd -MMM-yyyy");

            return View(Data.FirstOrDefault() ?? new VitalHistoryBMT());
        }
        public async Task<ActionResult> GetLabOrdersBMT(int visitId = 0)
        {
            List<LabReportsBMT> labreports = new List<LabReportsBMT>();
            List<KeyAndValuePair> LabTestList = await _CommonBL.GetDefinationData("Lookups", "System", "LabTestList");
            ViewBag.LabTestListVB = LabTestList.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> ResultLabel = await _CommonBL.GetDefinationData("Lookups", "System", "ResultLabel");
            ViewBag.ResultLabelVB = ResultLabel.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ResultUnit = await _CommonBL.GetDefinationData("Lookups", "System", "ResultUnit");
            ViewBag.ResultUnitVB = ResultUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> ProtocoName = await _CommonBL.GetDefinationData("Lookups", "System", "ResultUnit");
            ViewBag.ProtocoNameVB = ProtocoName.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            if (visitId > 0)
            {
                await Task.Run(() =>
                {
                    labreports = _PatientBL.GetLabOrdersBMT(visitId);
                });
            }
            return View(labreports);
        }
        //public ActionResult GetPreviousVisitLabTestsBMT(int patientId, int visitId)
        //{
        //    List<LabReportsBMT> reports = _PatientBL.GetPreviousVisitLabTestsBMT(patientId, visitId);

        //    return View(reports);
        //}
        public async Task<ActionResult> savePatientDetail1BMT(VitalHistoryBMT vitalHistory)
        {
            AjaxResponse response = new AjaxResponse();
            int result = 0;
            try
            {
                if (vitalHistory == null)
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
                    Users u = Session["User"] as Users;
                    vitalHistory.EnteredBy = u.UserId;
                    vitalHistory.EnteredOn = DateTime.Now;
                    vitalHistory.ClinicId = Convert.ToInt32(Session["ClinicId"]);
                    result = await _PatientBL.insertUpdatePatientDetailBMT(vitalHistory);
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
                            RedirectUrl = Url.Action("GetPatientList", "Patient"),
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

        public async Task<ActionResult> PatientListOfBMTForMDT(DateTime? fromDate, DateTime? toDate)
        {
            List<VitalHistoryBMT> Data = new List<VitalHistoryBMT>();
            fromDate = fromDate ?? DateTime.Now;
            toDate = toDate ?? DateTime.Now;

            Data = _PatientBL.GetPatientListOfBMTForMDTList(fromDate.Value, toDate.Value, GetUserLocationIdsCsv());

            ViewBag.FromDate = fromDate.Value;
            ViewBag.ToDate = toDate.Value;


            return View(Data);
        }
        #endregion
    }







}