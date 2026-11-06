using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BDCS.Model.Patient;
using BDCS.Data;
using BDCS.Model.Visit;
using BDCS.Model.VitalHistory;
using System.Transactions;
using BDCS.Model.RoleRights;
using BDCS.Model.PatientServices;
using System.Web;
using BDCS.Model.Misc;

namespace BDCS.Bussiness
{
   public class PatientBL
    {
        readonly PatientDAL _patientBL;
        
        public PatientBL()
        {
            _patientBL = new PatientDAL();
        }


        public List<Patient> GetPatientList(string searchText,int clinicId) => _patientBL.PatientSearchByText("PatientSearchByText", "Clinical", searchText,clinicId);
        public List<Patient> GetFollowupPatientsForDate(DateTime date) => _patientBL.GetFollowupPatientsForDate(date);
        public async Task<int> InsertUpdatePatient(Patient patient) =>await _patientBL.InsertUpdatePatient(patient, "InsertUpdatePatient", "Clinical");

        public async Task<int> DoesPatientHospitalNumberExists(string hospitalNumber, int patientId)
            => await _patientBL.DoesPatientHospitalNumberExists(hospitalNumber, patientId);

        public async Task<int> ToggleDiscussionStatus(int patientId) => await _patientBL.ToggleDiscussionStatus(patientId);
        public async Task<int> CreateVisit(Visit vist) => await _patientBL.CreateVisit(vist, "InsertUpdateVisit", "Clinical");

        public Patient GetPatientById(int patId) => _patientBL.GetPatientById(patId, "GetPatientById", "Clinical");
        public List<Patient> GetPatientByIdForRpt(int patId) => _patientBL.GetPatientByIdForRpt(patId, "GetPatientById", "Clinical");
        public List<LabReports> GetLabOrders(int visitId) => _patientBL.GetLabOrders("GetLabOrders", "Clinical", visitId);

        public List<LabReports> GetLabOrdersAgainstPreviousVisits(int visitId) => _patientBL.GetLabOrders("GetLabOrdersAgainstPreviousVisits", "Clinical", visitId);

        public List<LabReports> GetPreviousLabReport(int visitId) => _patientBL.GetPreviousLabReport("GetPreviousLabReport", "Clinical", visitId);

        public List<Protocols> GetPatientProtocolDetail(int visitId) => _patientBL.GetPatientProtocolDetail("GetPatientProtocolDetail", "Clinical", visitId);

        public List<Protocols> getProtocols(int visitId) => _patientBL.getProtocols("GetProtocols", "System", visitId);


        public List<Protocols> GetProtocolDetail(string name) => _patientBL.GetProtocolDetail("GetProtocolDetail", "System", name);

        public List<Protocols> GetPatientPreviousProtocols(int patientId, int visitId)
            => _patientBL.GetPatientPreviousProtocols(patientId, visitId);
        public async  Task<List<Protocols>> GetProtocolByName(string name) => await _patientBL.GetProtocolByName(name);

        public async Task<List<PatientServices>> GetServiceByName(string name) => await _patientBL.GetServiceByName(name);

        public List<PatientServices> GetServiceDetails(string name) => _patientBL.GetServiceDetails("GetServiceDetail", "System", name);

        public async Task<int> insertUpdatePatientServiceDetail(List<PatientServices> patientServices)
        {
            int ChargeServiceId = 0;


            if (patientServices != null)
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
                {
                    try
                    {

                        foreach (PatientServices item in patientServices)
                        {

                            ChargeServiceId = await _patientBL.insertUpdatePatientService(item, "PatientServiceDetail", "Clinical");
                        }

                        scope.Complete();
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        throw;
                    }

                }

      
            }
            return ChargeServiceId;
        }

        public async Task<int> insertUpdatePatientDetail(VitalHistory vitalHistory)
        {
            int patientDetailId = 0;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    //MasterTable
                    patientDetailId = await _patientBL.insertUpdatePatientDetail(vitalHistory, "InsertUpdatePatientDetail", "Clinical");

                    if (vitalHistory.OtherComorbities != null && vitalHistory.OtherComorbities.Any())
                    {
                        string csv = vitalHistory.OtherComorbities[0];
                        int deleted = await _patientBL.DeleteOtherComorbidities(vitalHistory.VisitId);
                        foreach (string cm in csv.Split(','))
                        {

                            int b = await _patientBL.InsertOtherComorbidities(vitalHistory.VisitId, vitalHistory.PatientId, cm);
                        }
                    }
                    if (vitalHistory.SiteOfAngioinvasionVB != null && vitalHistory.SiteOfAngioinvasionVB.Any())
                    {
                        string csv = vitalHistory.SiteOfAngioinvasionVB[0];
                        int deleted = await _patientBL.DeleteSiteOfAngioInvasion(vitalHistory.VisitId);
                        foreach (string cm in csv.Split(','))
                        {

                            int b = await _patientBL.InsertSiteOfAngioinvasion(vitalHistory.VisitId, vitalHistory.PatientId, cm);
                        }
                    }
                    if (vitalHistory.MDTOutCome != null && vitalHistory.MDTOutCome.Any())
                    {
                        string csv = vitalHistory.MDTOutCome[0];

                        int deleted = await _patientBL.DeleteMDTOutcome(vitalHistory.VisitId);

                        foreach (string mo in csv.Split(','))
                        {

                            int b = await _patientBL.InsertMDTOutCome(vitalHistory.VisitId, vitalHistory.PatientId, mo);
                        }
                    }
                    
                    if (vitalHistory.SiteOfHepaticDisease != null && vitalHistory.SiteOfHepaticDisease.Any())
                    {
                        string csv = vitalHistory.SiteOfHepaticDisease[0];

                        int deleted = await _patientBL.DeleteSiteOfHepaticDisease(vitalHistory.VisitId);

                        foreach (string sh in csv.Split(','))
                        {

                            int b = await _patientBL.InsertSiteOfHepaticDisease(vitalHistory.VisitId, vitalHistory.PatientId, sh);
                        }
                    }

     
                    if (vitalHistory.TreatmentType != null && vitalHistory.TreatmentType.Any())
                    {
                        string csv = vitalHistory.TreatmentType[0];

                        int deleted = await _patientBL.DeleteTreatmentType(vitalHistory.VisitId);

                        foreach (string tt in csv.Split(','))
                        {

                            int b = await _patientBL.InsertTreatmentType(vitalHistory.VisitId, vitalHistory.PatientId, tt);
                        }
                    }

                    //OldLabReport
                    if (vitalHistory.PreviousLab != null && vitalHistory.PreviousLab.Any())
                    {
                        foreach (LabReports item in vitalHistory.PreviousLab)
                        {
                            item.PatientId = vitalHistory.PatientId;

                            int b = await _patientBL.insertUpdateLabReports(item, "InsertUpdatePreviousReports", "Clinical");
                        }
                    }
                    //CurrentLabs
                    if (vitalHistory.labreports != null && vitalHistory.labreports.Any())
                    {
                        foreach (LabReports item in vitalHistory.labreports)
                        {
                            item.PatientId = vitalHistory.PatientId;
                            int c = await _patientBL.insertUpdateLabReports(item, "InsertUpdateCurrentReports", "Clinical");
                        }
                    }
                    //Protocos
                    if (vitalHistory.protocols != null && vitalHistory.protocols.Any())
                    {
                        foreach (Protocols item in vitalHistory.protocols)
                        {
                            item.PatientId = vitalHistory.PatientId;
                            item.VisitId = vitalHistory.VisitId;
                            //item.ProtocolDetailId = 
                            int d = await _patientBL.insertUpdateProtocol(item, "InsertUpdatePatientProtocolDetail", "Clinical");
                        }
                    }

                    if (vitalHistory.PatientDiagnosis != null && vitalHistory.PatientDiagnosis.Any())
                    {
                        foreach (PatientDiagnosis item in vitalHistory.PatientDiagnosis)
                        {

                            item.VisitId = vitalHistory.VisitId;
                            item.EnteredBy = vitalHistory.EnteredBy;
                            //item.ProtocolDetailId = 
                            int d = await _patientBL.InsertUpdatePatientDiagnosis(item);
                        }
                    }
                    scope.Complete();
                    //scope.Dispose();

                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    throw;
                }

               
            }
            return patientDetailId;
        }


        public List<Visit> GetVisitHistory(int patId,string mdtType) => _patientBL.GetVisitHistory("GetVisitHistory", "Clinical", patId,mdtType);
        public List<VitalHistory> GetPatientDetail(int visitId, int patId) => _patientBL.GetPatientDetail("GetPatientDetail", "Clinical", visitId, patId);
        public Users ValidateLogin(string loginId,string password) => _patientBL.ValidateLogin("ValidateLogin", "System", loginId,password);

        public async Task<int> CopyVisitData(int oldVisitId, int newVisitId) => await _patientBL.CopyVisitData(oldVisitId, newVisitId);
        public async Task<int> CopyVisitDataBMT(int oldVisitId, int newVisitId) => await _patientBL.CopyVisitDataBMT(oldVisitId, newVisitId);
        public List<LabReports> GetLabTestForGroupName(string groupName) => _patientBL.GetLabTestForGroupName(groupName);

        public List<LabReports> GetPreviousVisitLabTests(int patientId, int visitId) => _patientBL.GetPreviousVisitLabTests(patientId, visitId);

        public List<PatientServices> GetPatientServiceAgainstVisit(int visitId) => _patientBL.GetPatientServiceAgainstVisit(visitId);

        public async Task<int> DeleteProtocolDetail(int id) => await _patientBL.DeleteProtocolDetail(id);

        public List<Protocols> GetFavoriteMedicineForUser(int userId) => _patientBL.GetFavoriteMedicineForUser(userId);

        public List<Protocols> SearchMedicineAndFavorites(string searchText, int userId) => _patientBL.SearchMedicineAndFavorites(searchText, userId);

        public async Task<int> InsertMedicineFavorite(Protocols protocol) => await _patientBL.InsertMedicineFavorite(protocol);

        public async Task<int> DeleteFavoritemMedicine(int userId, string medicineName, string medicineType) 
            => await _patientBL.DeleteFavoritemMedicine(userId, medicineName, medicineType);

        public async Task<int> InsertUpdatePatientDiagnosis(PatientDiagnosis diagnosis)
            => await _patientBL.InsertUpdatePatientDiagnosis(diagnosis);

        public List<PatientDiagnosis> GetPatientDiagnosisAgainstVisit(int visitId)
            => _patientBL.GetPatientDiagnosisAgainstVisit(visitId);

        public async Task<int> MarkPatientDiagnosisAsInactive(int patientDiagnosisId)
            => await _patientBL.MarkPatientDiagnosisAsInactive(patientDiagnosisId);

        public async Task<int> InsertPatientAlert(PatientAlert alert)
            => await _patientBL.InsertPatientAlert(alert);

        public List<PatientAlert> GetPatientActiveAlerts(int patientId)
            => _patientBL.GetPatientActiveAlerts(patientId);

        public async Task<int> MarkPatientAlertAsInactive(int patientAlertId)
            => await _patientBL.MarkPatientAlertAsInactive(patientAlertId);

        public async Task<int> DiscontinueMedicine(int protocolDetailId)
            => await _patientBL.DiscontinueMedicine(protocolDetailId);

        public List<VitalHistory> GetVisitListAgainstSearch(int? ChildStatus, int? MeldStatus, int? BCLCStatus, int? ECOG, int? TreatmentResponse,
            int? MdtOutcome, int? HepatitisStatus, int? AscitiesStatus, int? Svr, int? OtherComorbidities, int? AngioInvasion, double? AFP, bool LiverCirrhosis, bool PortalHypertension, bool Varices, bool MeetingMilan, bool UCSF, bool ASAN)
            => _patientBL.GetVisitListAgainstSearch(ChildStatus, MeldStatus, BCLCStatus, ECOG, TreatmentResponse,
            MdtOutcome, HepatitisStatus, AscitiesStatus, Svr, OtherComorbidities, AngioInvasion, AFP, LiverCirrhosis, PortalHypertension, Varices, MeetingMilan, UCSF, ASAN);

        public List<VitalHistory> GetPatientListForMDTList(DateTime fromDate, DateTime toDate)
            => _patientBL.GetPatientListForMDTList(fromDate, toDate);


        #region [BMT]

        #region [BMT Reporting Methods]
        
        #endregion
        public List<VitalHistoryBMT> GetBMTPatientDetail(int visitId, int patId) => _patientBL.GetBMTPatientDetail("GetPatientDetailBMT", "Clinical", visitId, patId);

        public List<LabReportsBMT> GetLabOrdersBMT(int visitId) => _patientBL.GetLabOrdersBMT("GetLabOrdersBMT", "Clinical", visitId);
        public List<PatientDiagnosisBMT> GetPatientDiagnosisAgainstVisitBMT(int visitId)
            => _patientBL.GetPatientDiagnosisAgainstVisitBMT(visitId);
        public async Task<int> insertUpdatePatientDetailBMT(VitalHistoryBMT vitalHistory)
        {
            int patientDetailId = 0;

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    //MasterTable
                    //patientDetailId = await _patientBL.insertUpdatePatientDetail(vitalHistory, "InsertUpdatePatientDetail", "Clinical");
                    patientDetailId = await _patientBL.insertUpdatePatientDetailBMT(vitalHistory, "InsertUpdatePatientDetailBMT", "Clinical");

                    if (vitalHistory.labreports != null && vitalHistory.labreports.Any())
                    {
                        foreach (LabReportsBMT item in vitalHistory.labreports)
                        {
                            item.PatientId = vitalHistory.PatientId;
                            int c = await _patientBL.insertUpdateLabReportsBMT(item, "InsertUpdateCurrentReportsBMT", "Clinical");
                        }
                    }

                    if (vitalHistory.OtherComorbitiesBMT != null && vitalHistory.OtherComorbitiesBMT.Any())
                    {
                        //string csv = vitalHistory.OtherComorbities[0];
                        string csv = string.Join(",", vitalHistory.OtherComorbitiesBMT);
                        int deleted = await _patientBL.DeleteOtherComorbiditiesBMT(vitalHistory.VisitId);
                        foreach (string cm in csv.Split(','))
                        {
                            int b = await _patientBL.InsertOtherComorbiditiesBMT(vitalHistory.VisitId, vitalHistory.PatientId, cm);
                        }
                    }
                    if (vitalHistory.SiteOfLymphnodes != null && vitalHistory.SiteOfLymphnodes.Any())
                    {
                        //string csv = vitalHistory.SiteOfLymphnodes[0];
                        string csv = string.Join(",", vitalHistory.SiteOfLymphnodes);
                        int deleted = await _patientBL.DeleteSiteOfLymphnodesBMT(vitalHistory.VisitId);
                        foreach (string cm in csv.Split(','))
                        {
                            int b = await _patientBL.InsertSiteOfLymphnodesBMT(vitalHistory.VisitId, vitalHistory.PatientId, cm);
                        }
                    }
                    if (vitalHistory.MDTOutCome != null && vitalHistory.MDTOutCome.Any())
                    {
                        //string csv = vitalHistory.MDTOutCome[0];
                        string csv = string.Join(",", vitalHistory.MDTOutCome);

                        int deleted = await _patientBL.DeleteMDTOutcomeBMT(vitalHistory.VisitId);

                        foreach (string mo in csv.Split(','))
                        {
                            int b = await _patientBL.InsertMDTOutComeBMT(vitalHistory.VisitId, vitalHistory.PatientId, mo);
                        }
                    }

                    if (vitalHistory.PreviousTreatmentBMT != null && vitalHistory.PreviousTreatmentBMT.Any())
                    {
                        
                        string csv = string.Join(",", vitalHistory.PreviousTreatmentBMT);

                        int deleted = await _patientBL.DeletePreviousTreatmentBMT(vitalHistory.VisitId);

                        foreach (string mo in csv.Split(','))
                        {
                            int b = await _patientBL.InsertPreviousTreatmentBMT(vitalHistory.VisitId, vitalHistory.PatientId, mo);
                        }
                    }

                    if (vitalHistory.PatientDiagnosis != null && vitalHistory.PatientDiagnosis.Any())
                    {
                        foreach (PatientDiagnosisBMT item in vitalHistory.PatientDiagnosis)
                        {
                            item.VisitId = vitalHistory.VisitId;
                            item.EnteredBy = vitalHistory.EnteredBy;
                            //item.ProtocolDetailId = 
                            int d = await _patientBL.InsertUpdatePatientDiagnosisBMT(item);
                        }
                    }
                    scope.Complete();
                    //scope.Dispose();
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    throw;
                }
            }
            return patientDetailId;
        }

        public List<LabReportsBMT> GetPreviousVisitLabTestsBMT(int patientId, int visitId)
            => _patientBL.GetPreviousVisitLabTestsBMT(patientId, visitId);

        public List<VitalHistoryBMT> GetPatientListOfBMTForMDTList(DateTime fromDate, DateTime toDate)
            => _patientBL.GetPatientListOfBMTForMDTList(fromDate, toDate);
        #endregion
    }

}

