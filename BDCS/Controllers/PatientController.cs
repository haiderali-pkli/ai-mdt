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

        #region [AddEditPatient]
        public async Task<ActionResult> AddEditPatient(int patId = 0)
        {
            Patient pat = new Patient();
            if (patId > 0)
            {
                pat = _PatientBL.GetPatientById(patId);
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

            List<KeyAndValuePair> AgeUnit = new List<KeyAndValuePair>() {
                  new KeyAndValuePair() { Id = "Days", Value = "Days" },
                new KeyAndValuePair() { Id = "Years", Value = "Years" },
                new KeyAndValuePair() { Id = "Months", Value = "Months" },
              
            };

            
            ViewBag.DiagnosisVB = Diagnosis.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.RelationshipVB = RelationShip.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.GenderVB = Gender.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.CountryVB = Country.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.ReferredbyVB = Referredby.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            ViewBag.AgeUnit = AgeUnit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
     ViewBag.EthnicityVB = Ethnicity.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


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

        //LoginView Submit--> and call the -->ValidageLoginId and Rights-->PatientDAL-->AuthenticationFilter(check session values userId and Rigts)-->SessionHelper(check userobject and rights exists in session)-->
        #region [GetPatientList]
        [CheckUserRights(MenuName  = "Search Patient", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> GetPatientList(string searchText)
         {
            List<Patient> pat = new List<Patient>();
            int clinicId = Convert.ToInt32(Session["ClinicId"]);
            if (searchText!=null)
            {
                await Task.Run(() =>
                {
                    pat = _PatientBL.GetPatientList(searchText,clinicId);
                });
            }
            ViewBag.SearchText = searchText;
            List<KeyAndValuePair> visit = await _CommonBL.GetDefinationData("lookups", "System", "VisitType");
            ViewBag.VisitType = visit.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

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

        [CheckUserRights(MenuName = "Create Visit", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> CreatePatientVisit(int patId,string reason)
        {
            Visit visit = new Visit();
            AjaxResponse response = new AjaxResponse();
            visit.PatientId = patId;
            visit.Reason = reason;
            visit.VisitDate = DateTime.Now;
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

        #region [AddEditPatientDetail]
        public async Task<ActionResult> AddEditPatientDetail(int patId, string visitDateTime, int visitId = 0, string reason = "")
        {
            Patient Data = new Patient();
            List<PatientAlert> alert = new List<PatientAlert>();

            if (patId!=0 && visitId!=0)
            {
                await Task.Run(() =>
                    Data = _PatientBL.GetPatientById(patId)
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

        public ActionResult PatientHeader(int patId)
        {
            Patient Data = new Patient();
            List<PatientAlert> alert = new List<PatientAlert>();

            if (patId != 0)
            {
                Data = _PatientBL.GetPatientById(patId);
               
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
        public async Task <ActionResult> VitalHistory(int visitId,string reason,int patId)
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

            List<KeyAndValuePair> BCLC = await _CommonBL.GetDefinationData("Lookups", "System", "BCLC");
            ViewBag.BCLCVB = MeldStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> ECOGStatus = await _CommonBL.GetDefinationData("Lookups", "System", "ECOGStatus");
            ViewBag.ECOGStatusVB = ECOGStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> HepStatus = await _CommonBL.GetDefinationData("Lookups", "System", "HepStatus");
            ViewBag.HepStatusVB = HepStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();


            List<KeyAndValuePair> AscitiesStatus = await _CommonBL.GetDefinationData("Lookups", "System", "AscitiesStatus");
            ViewBag.AscitiesStatusVB = AscitiesStatus.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> LargestLesionSize = await _CommonBL.GetDefinationData("Lookups", "System", "LargestLesionSize");
            ViewBag.LargestLesionSizeVB = LargestLesionSize.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> MDTOutCome = await _CommonBL.GetDefinationData("Lookups", "System", "MDTOutCome");
            ViewBag.MDTOutComeVB = MDTOutCome.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SiteOfAngioinvasion = await _CommonBL.GetDefinationData("Lookups", "System", "SiteOfAngioinvasion");
            ViewBag.SiteOfAngioinvasionVB = SiteOfAngioinvasion.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> TreatmentResponse = await _CommonBL.GetDefinationData("Lookups", "System", "TreatmentResponse");
            ViewBag.TreatmentResponseVB = TreatmentResponse.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> RefusalReason = await _CommonBL.GetDefinationData("Lookups", "System", "RefusalReason");
            ViewBag.RefusalReasonVB = RefusalReason.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();

            List<KeyAndValuePair> SitesOfExtraHepaticDisease = await _CommonBL.GetDefinationData("Lookups", "System", "SitesOfExtraHepaticDisease");
            ViewBag.SitesOfExtraHepaticDiseaseVB = SitesOfExtraHepaticDisease.Select(a => new SelectListItem() { Text = a.Value, Value = a.Id }).ToList();
            
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

                            Data[0].PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
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

        public ActionResult GetPreviousVisitLabTests(int patientId, int visitId)
        {
            List<LabReports> reports = _PatientBL.GetPreviousVisitLabTests(patientId, visitId);



            return View(reports);
        }

        #endregion

        #region [PatientVisitHisotry]
        [CheckUserRights(MenuName = "Visit History", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> GetVisitHistory(int patId, string patName)
        {
            List<Visit> data = new List<Visit>();
            if (patId>0)
            {
                await Task.Run(() =>
                {
                    data = _PatientBL.GetVisitHistory(patId);
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
                    //here we want a list of rights and we pcik form roleId because we gave rights to role not to user
                    List<RoleRight> Rights = _user.GetRoleRightsAgainstRole(data.RoleId);
                    Session["Rights"] = Rights;
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
                    Patient = _PatientBL.GetPatientById(patId)
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


        [CheckUserRights(MenuName = "Visit History", Order = 1, UserRightType = RightType.View)]
        public async Task<ActionResult> PatientVisitReportRdlc(int patId, int visitId, string reason = "")
        {

            List<Patient> Patient = new List<Patient>();
            if (patId != 0 && visitId != 0)
            {
                await Task.Run(() =>
                    Patient = _PatientBL.GetPatientByIdForRpt(patId)
               );
            }


            List<VitalHistory> Data = new List<VitalHistory>();

            //if (Data != null && Data.Count > 0)
            //{
            await Task.Run(() =>
            {
                Data = _PatientBL.GetPatientDetail(visitId, patId);
                if (Data != null)
                {
                    Data[0].PreviousLab = _PatientBL.GetPreviousLabReport(visitId);
                    Data[0].labreports = _PatientBL.GetLabOrders(visitId);
                    Data[0].protocols = _PatientBL.GetPatientProtocolDetail(visitId);
                }
              }
            );


            Warning[] warnings;
            string mimeType;
            string[] streamids;
            string encoding;
            string filenameExtension;


            LocalReport Report = new LocalReport();

            Report.EnableExternalImages = true;
            Report.ReportPath = Server.MapPath("~/Reports/Patient.rdlc");
            Report.DataSources.Add(new ReportDataSource("PreviousLabReports", Data[0].PreviousLab));
            Report.DataSources.Add(new ReportDataSource("CurrentLabs", Data[0].labreports));
            Report.DataSources.Add(new ReportDataSource("Protocols", Data[0].protocols ));
            Report.DataSources.Add(new ReportDataSource("Patient", Patient));
            Report.DataSources.Add(new ReportDataSource("PatientDetail", Data));
            Report.SetParameters(new ReportParameter("PatientId", patId.ToString()));
            Report.SetParameters(new ReportParameter("VisitId", visitId.ToString()));
            Report.SetParameters(new ReportParameter("Reason", reason));
            string imagePath = new Uri(Server.MapPath(Patient[0].ImageURL)).AbsoluteUri;
            ReportParameter p = new ReportParameter("ImagePath", imagePath);

            Report.SetParameters(p);
            Report.SubreportProcessing += new SubreportProcessingEventHandler(LocalReport_SubreportProcessing);
          //  Report.SubreportProcessing += new SubreportProcessingEventHandler(PreviousResult_SubreportProcessing);

            Report.Refresh();

            var bytes = Report.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);

            return new FileContentResult(bytes, mimeType);

        }

        void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {

            int PatientId = Convert.ToInt32(e.Parameters["PatientId"].Values[0]);
            // our subreport has only 1 parameter
            int VisitId = Convert.ToInt32(e.Parameters["VisitId"].Values[0]);
            // our subreport has only 1 dataset
            string dataSourceName = e.DataSourceNames[0];
            List<VitalHistory> Data = new List<VitalHistory>();
            Data = new PatientBL().GetPatientDetail(VisitId, PatientId);

            List<PatientDiagnosis> Diagnosis = _PatientBL.GetPatientDiagnosisAgainstVisit(VisitId);
            if (Diagnosis != null && Diagnosis.Any())
            {
                Diagnosis = Diagnosis.Where(a => a.IsActive == true).ToList();
            }

            List<PatientAlert> Alerts =  _PatientBL.GetPatientActiveAlerts(PatientId);

            e.DataSources.Add(new ReportDataSource("PatientDetail", Data));
            e.DataSources.Add(new ReportDataSource("PreviousLabReports", new PatientBL().GetPreviousLabReport(VisitId)));
            e.DataSources.Add(new ReportDataSource("CurrentLabs", new PatientBL().GetLabOrders(VisitId)));
            e.DataSources.Add(new ReportDataSource("PreviousVisitsLabs", new PatientBL().GetLabOrdersAgainstPreviousVisits(VisitId).Where(a => a.VisitId == VisitId).ToList()));
            e.DataSources.Add(new ReportDataSource("Protocols", new PatientBL().GetPatientProtocolDetail(VisitId)));
            e.DataSources.Add(new ReportDataSource("Patient", new PatientBL().GetPatientByIdForRpt(PatientId)));
            e.DataSources.Add(new ReportDataSource("PatientDiagnosis", Diagnosis ));
            e.DataSources.Add(new ReportDataSource("PatientAlerts", Alerts));
        }


        void PreviousResult_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {

            int PatientId = Convert.ToInt32(e.Parameters["PatientId"].Values[0]);
            // our subreport has only 1 parameter
            int VisitId = Convert.ToInt32(e.Parameters["VisitId"].Values[0]);
            // our subreport has only 1 dataset
            string dataSourceName = e.DataSourceNames[0];
            List<VitalHistory> Data = new List<VitalHistory>();
            Data = new PatientBL().GetPatientDetail(VisitId, PatientId);
            
            e.DataSources.Add(new ReportDataSource("PreviousLabReports", new PatientBL().GetPreviousLabReport(VisitId)));

        }

        [CheckUserRights(MenuName = "Add Patient", Order = 1, UserRightType = RightType.View)]
        public ActionResult PatientCard(int PatientId)
        {
            Patient pat = new Patient();
            if (PatientId> 0)
            {
                pat = _PatientBL.GetPatientById(PatientId);
            }

            return View(pat);
        }
        [CheckUserRights(MenuName = "Create Visit", Order = 1, UserRightType = RightType.AddEdit)]
        public async Task<ActionResult> CopyVisit(int patientId, int oldVisitId )
        {
            Visit visit = new Visit();
            AjaxResponse response = new AjaxResponse();
            visit.PatientId = patientId;
            visit.Reason = "Followup";
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
                Data = _PatientBL.GetPatientById(patientId);
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

        public ActionResult PatientVisitAdvanceSearch()
        {

            return View();
        }

    }
}