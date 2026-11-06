using BDCS.Data.Helpers;
using BDCS.Model.Patient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using BDCS.Model.Visit;
using BDCS.Model.VitalHistory;
using BDCS.Model.RoleRights;
using BDCS.Model.PatientServices;
using BDCS.Model.Misc;

namespace BDCS.Data
{
    public class PatientDAL
    {
        IDBManger _db;

        public PatientDAL()
        {
            _db = new SqlDbManager();
        }

        public List<Patient> PatientSearchByText(string procedureName,string schema,string parameter,int clinicId)
            {
            List<Patient> Data = new List<Patient>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "SearchText", Type = DbType.String, Value = parameter.ToString() },
                new DbParameters() {Name="ClinicId",Type=DbType.Int32,Value=clinicId },
 
            };
 
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Patient()
                    {
                     
                        PatientId =Convert.ToInt32(a["PatientId"]),
                        HospitalNumber=a["HospitalNumber"].ToString(),
                        MrNumber = a["MrNumber"].ToString(),
                        FirstName =a["FirstName"].ToString(),
                        LastName= a["LastName"].ToString(),
                        FatherSpouseName =a["FatherSpouseName"].ToString(),
                        Gender = (a["Gender"]).ToString(),
                        DOB=Convert.ToDateTime(a["DOB"]),
                        EmailId = a["DOB"].ToString(),
                        Ethnicity=a["Ethnicity"].ToString(),
                        MobileNumber= a["MobileNumber"].ToString(),
                        ImageURL = a["ImageURL"].ToString(),
                        EnteredOn = Convert.ToDateTime(a["EnteredOn"]),
                        DiscussionRequired = Convert.ToBoolean(a["DiscussionRequired"]),
                    }).ToList();
                }
            }
            catch (Exception )
            {
                throw;
            }
            return Data;
        }
       public async Task<int> InsertUpdatePatient(Patient patient,string procedureName,string schema)
        {
            Patient pat = new Patient();
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientId",Type=DbType.Int16, Value=patient.PatientId },
                new DbParameters() {Name="FirstName",Type=DbType.String, Value=patient.FirstName },
                new DbParameters() {Name="LastName", Type=DbType.String,Value=patient.LastName },
                //new DbParameters() {Name="MrNumber", Type=DbType.String,Value=patient.MrNumber },
                new DbParameters() {Name="Relationship", Type=DbType.String,Value=patient.Relationship },
                new DbParameters() {Name="FatherSpouseName", Type=DbType.String,Value=patient.FatherSpouseName },
                new DbParameters() {Name="GenderId", Type=DbType.String,Value=patient.GenderId },
                new DbParameters() {Name="DOB", Type=DbType.String,Value=patient.DOB },
                new DbParameters() {Name="BloodGroupId", Type=DbType.String,Value=patient.BloodGroupId },
                new DbParameters() {Name="CountryId", Type=DbType.String,Value=patient.CountryId == 0 ? null : patient.CountryId.ToString() },
                new DbParameters() {Name="ProvinceId", Type=DbType.Int32,Value=patient.ProvinceId == 0 ? null :   patient.ProvinceId.ToString() },
                new DbParameters() {Name="DistrictId", Type=DbType.Int32,Value=patient.DistrictId == 0 ? null : patient.DistrictId.ToString() },
                new DbParameters() {Name="City", Type=DbType.String,Value=patient.City  },
                new DbParameters() {Name="EmailId", Type=DbType.String,Value=patient.EmailId },
                new DbParameters() {Name="Ethnicity", Type=DbType.String,Value=patient.Ethnicity },
                new DbParameters() {Name="MobileNumber", Type=DbType.String,Value=patient.MobileNumber },
                new DbParameters() {Name="ContactNumber", Type=DbType.String,Value=patient.ContactNumber },
                new DbParameters() {Name="Address", Type=DbType.String,Value=patient.Address },
                new DbParameters() {Name="ReferredBy", Type=DbType.String,Value=patient.ReferredBy },
                //new DbParameters() {Name="DiagnosisId", Type=DbType.Int32,Value=patient.DiagnosisId },
                new DbParameters() {Name="IsDiabeties", Type=DbType.Int32,Value=patient.IsDiabeties },
                new DbParameters() {Name="Comments", Type=DbType.String,Value=patient.Comments },
                new DbParameters() {Name="ImageURL", Type=DbType.String,Value=patient.ImageURL },
                new DbParameters() {Name="IsActive", Type=DbType.String,Value=patient.IsActive },
                new DbParameters() {Name="EnteredBy", Type=DbType.String,Value=patient.EnteredBy },
                new DbParameters() {Name="EnteredOn", Type=DbType.String,Value=patient.EnteredOn },
                new DbParameters() {Name="UpdatedBy", Type=DbType.String,Value=patient.UpdatedBy },
                new DbParameters() {Name="UpdatedOn", Type=DbType.String,Value=patient.UpdatedOn },
                new DbParameters() {Name="ClinicId", Type=DbType.String,Value=patient.ClinicId },
                new DbParameters() {Name="HospitalNumber", Type=DbType.String,Value=patient.HospitalNumber },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }

        public async Task<int> DoesPatientHospitalNumberExists(string hospitalNumber, int patientId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DoesHospitalNumberExist", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="HospitalNumber", Type=DbType.String,Value=hospitalNumber},
                new DbParameters() {Name="PatientId", Type=DbType.Int32,Value=patientId},
            };

            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> CreateVisit(Visit visit,string procedureName,string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientId", Type=DbType.Int32,Value=visit.PatientId },
                new DbParameters() {Name="VisitDateTime", Type=DbType.DateTime,Value=visit.VisitDate },
                new DbParameters() {Name="Reason", Type=DbType.String,Value=visit.Reason },
                new DbParameters() {Name="ClinicId", Type=DbType.String,Value=visit.ClinicId },
                new DbParameters() {Name="MDTType", Type=DbType.String,Value=visit.MDTType},
            };
            try
            {
                result =  Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }

        public Patient GetPatientById(int patId,string procedureName,string schema)
        {
            Patient Data = new Patient();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {  Name="PatientId", Type=DbType.Int32, Value=patId}
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Patient()
                    {
                        PatientId=Convert.ToInt32(a["PatientId"]),
                        MrNumber=a["MrNumber"].ToString(),
                        FirstName=a["FirstName"].ToString(),
                        LastName = a["LastName"].ToString(),
                        Relationship=a["Relationship"].ToString(),
                        FatherSpouseName = a["FatherSpouseName"].ToString(),
                        GenderId=Convert.ToInt32(a["LookupId"]),
                        DOB = Convert.ToDateTime(a["DOB"]),
                        Gender=a["Gender"].ToString(),
                        DiagnosisId=Convert.ToInt32(a["DiagnosisId"]),
                        CountryId = Convert.ToInt32(a["CountryId"]),
                        ProvinceId = Convert.ToInt32(a["ProvinceId"]),
                        DistrictId = Convert.ToInt32(a["DistrictId"]),
                        EmailId = a["EmailId"].ToString(),
                        Ethnicity = a["Ethnicity"].ToString(),
                        MobileNumber = a["MobileNumber"].ToString(),
                        ContactNumber = a["ContactNumber"].ToString(),
                        Address = a["Address"].ToString(),
                        ReferredBy = a["ReferredBy"].ToString(),
                        IsDiabeties = Convert.ToBoolean(a["IsDiabeties"]),
                        Comments = a["Comments"].ToString(),
                        ImageURL = a["ImageURL"].ToString(),
                        BloodGroupId=Convert.ToInt32(a["BloodGroupId"]),
                        BloodGroupName=a["BloodGroupName"].ToString(),
                        //EnteredBy=Convert.ToInt32(a["EnteredBy"]),
                        EnteredOn = Convert.ToDateTime(a["EnteredOn"]),
                        DiagnosisName=a["Diagnosis"].ToString(),
                        Rel=a["Rel"].ToString(),
                        HospitalNumber = a["HospitalNumber"].ToString(),
                        

                    }).FirstOrDefault();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;

        }


        public async Task<int> ToggleDiscussionStatus(int patientId)
        {
            Patient pat = new Patient();
            int result = 0;
            string Query = QueryHelper.GetQueryString("ToggleDiscussionStatus", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientId",Type=DbType.Int32, Value=patientId },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception e)
            {

                throw;
            }
            return result;
        }

        //Bad way
        public List<Patient> GetPatientByIdForRpt(int patId, string procedureName, string schema)
        {
            List<Patient> Data = new List<Patient>();
            Data.Add(GetPatientById(patId,procedureName,schema));
            return Data;

        }
        public List<LabReports> GetLabOrders(string procedureName,string schema,int parameters)
        {
            List<LabReports> Data = new List<LabReports>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name="VisitId", Type=DbType.Int32, Value=parameters }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {

                    Data = ds.Tables[0].AsEnumerable().Select(a => new LabReports()
                    {
                        //LabReportId = Convert.ToInt32(a["LabReportId"]),
                        LabReportId = Convert.ToInt32(a["CurrentReportId"]),
                        VisitId =ds.Tables[0].Columns.Contains("VisitId") ? Convert.ToInt32(a["VisitId"]) : 0,
                        //PreviousReportId=Convert.ToInt32(a["PreviousReportId"]),
                        TestId = Convert.ToInt32(a["TestId"]==DBNull.Value ? 0 : a["TestId"]),
                        TestName = a["TestName"].ToString(),
                        TestDate = Convert.ToDateTime(a["TestDate"]),
                        TestResult = a["TestResult"].ToString(),
                        TestUnit = a["TestUnit"].ToString(),
                        TestResultLabel=a["TestResultLabel"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;
        }

        public List<LabReports> GetPreviousLabReport(string procedureName, string schema, int parameters)
        {
            List<LabReports> Data = new List<LabReports>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name="VisitId", Type=DbType.Int32, Value=parameters }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {

                    Data = ds.Tables[0].AsEnumerable().Select(a => new LabReports()
                    {
                        //LabReportId = Convert.ToInt32(a["CurrentReportId"]),

                        PreviousReportId=Convert.ToInt32(a["PreviousReportId"]),
                        TestId = Convert.ToInt32(a["TestId"] == DBNull.Value ? 0 : a["TestId"]),
                        TestName = a["TestName"].ToString(),
                        TestDate = Convert.ToDateTime(a["TestDate"]==DBNull.Value ? null: a["TestDate"]),
                        TestResult = a["TestResult"].ToString(),
                        TestUnit = a["TestUnit"].ToString(),
                        TestResultLabel = a["TestResultLabel"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;
        }


        public List<Protocols> getProtocols(string procedureName,string schema,int paramters)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="visitId", Type=DbType.Int32, Value=paramters }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        ProtocolId=Convert.ToInt32(a["ProtocolId"]),
                        ProtocoName = a["ProtocolName"].ToString(),
                        MedicineType =a["MedicineType"].ToString(),
                        Medicine=a["Medicine"].ToString(),
                        Dose=a["Dose"].ToString(),
                        DoseUnit=a["DoseUnit"].ToString(),
                        Route=a["Route"].ToString(),
                        Frequency=a["Frequency"].ToString(),
                        Duration=a["Duration"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public List<Protocols> GetPatientPreviousProtocols(int patientId, int visitId)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString("GetPatientPreviousProtocolDetail", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="visitId", Type=DbType.Int32, Value=visitId },
                new DbParameters { Name="PatientId", Type=DbType.Int32, Value=patientId}
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        ProtocolDetailId = Convert.ToInt32(a["ProtocolDetailId"]),
                        ProtocolId = Convert.ToInt32(a["ProtocolId"]),
                        ProtocoName = a["ProtocolName"].ToString(),
                        MedicineType = a["MedicineType"].ToString(),
                        Medicine = a["Medicine"].ToString(),
                        Dose = a["Dose"].ToString(),
                        DoseUnit = a["DoseUnit"].ToString(),
                        Route = a["Route"].ToString(),
                        Frequency = a["Frequency"].ToString(),
                        Duration = a["Duration"].ToString(),
                        IsDiscontinued = Convert.ToBoolean(a["IsDiscontinued"]),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }


        public List<Protocols> GetPatientProtocolDetail(string procedureName, string schema, int paramters)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="visitId", Type=DbType.Int32, Value=paramters }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        ProtocolDetailId=Convert.ToInt32(a["ProtocolDetailId"]),
                        ProtocolId = Convert.ToInt32(a["ProtocolId"] == DBNull.Value ? 0 : a["ProtocolId"]),
                        ProtocoName = a["ProtocolName"].ToString(),
                        MedicineType = a["MedicineType"].ToString(),
                        Medicine = a["Medicine"].ToString(),
                        Dose = a["Dose"].ToString(),
                        DoseUnit = a["DoseUnit"].ToString(),
                        Route = a["Route"].ToString(),
                        Frequency = a["Frequency"].ToString(),
                        Duration = a["Duration"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }
        //***
        public List<VitalHistory> GetPatientDetail(string procedureName, string schema, int parameter1,int paramter2)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="visitId",Type=DbType.Int32,Value=parameter1 },
                new DbParameters() {Name="patId",Type=DbType.Int32,Value=paramter2 },
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new VitalHistory()
                    {
                        PatientDetailId = Convert.ToInt32(a["PatientDetailId"]),
                        VisitId = Convert.ToInt32(a["VisitId"]),
                        VisitDateTime = Convert.ToDateTime(a["VisitDateTime"]),
                        VisitType = a["VisitType"].ToString(),
                        CIC = a["CIC"].ToString(),
                        ChildStatus = a["ChildStatus"].ToString(),
                        ChildStatusValue = a["ChildStatusValue"].ToString(),
                        MeldStatus = a["MeldStatus"].ToString(),
                        MeldStatusValue = a["MeldStatusValue"].ToString(),
                        BCLC = a["BCLC"].ToString(),
                        BCLCValue = a["BCLCValue"].ToString(),
                        ECOGStatus = a["ECOGStatus"].ToString(),
                        ECOGStatusValue = a["ECOGStatusValue"].ToString(),
                        HepStatus = a["HepStatus"].ToString(),
                        HepStatusValue = a["HepStatusValue"].ToString(),
                        AscitiesStatus = a["AscitiesStatus"].ToString(),
                        AscitiesStatusValue = a["AscitiesStatusValue"].ToString(),
                        OtherComorbities =  a["OtherComorbities"].ToString().Split(',').ToList(),
                        OtherComorbitiesString = a["OtherComorbities"].ToString(),
                        CTReportConclusion = a["CTReportConclusion"].ToString(),
                        Angioinvasion = Convert.ToBoolean(a["Angioinvasion"]),
                        SiteOfAngioinvasionVB = a["SiteOfAngioinvasionVB"].ToString().Split(',').ToList(),
                        ExtraHepaticDisease = Convert.ToBoolean(a["ExtraHepaticDisease"]),

                        //SiteOfHepaticDisease = a["SiteOfHepaticDisease"].ToString(),
                        SiteOfHepaticDisease = a["SiteOfHepaticDisease"].ToString().Split(',').ToList(),
                      
                        LiverCirrhosis = Convert.ToBoolean(a["LiverCirrhosis"]),
                        PortalHypertension = Convert.ToBoolean(a["PortalHypertension"]),
                        Varices = Convert.ToBoolean(a["Varices"]),
                        NumberOfLesion = a["NumberOfLesion"].ToString(),
                        NumberOfLesionValue = a["NumberOfLesionValue"].ToString(),
                        LargestLesionSize = a["LargestLesionSize"].ToString(),
                        SVRStatus = a["SVRStatus"].ToString(),
                        SVRStatusValue = a["SVRStatusValue"].ToString(),
                        PreviousTreatment = Convert.ToBoolean(a["PreviousTreatment"]),

                        TreatmentType = a["TreatmentType"].ToString().Split(',').ToList(),
                        
                        TreatmentResponse = a["TreatmentResponse"].ToString(),
                        TreatmentResponseValue = a["TreatmentResponseValue"].ToString(),

                        //TreatmentResponseValue = a["TreatmentResponseValue"].ToString(),
                        DiagnosisName = a["Diagnosis"].ToString(),
                        DiagnosisDetails = a["DiagnosisDetails"].ToString(),
                        MeetingMilan = Convert.ToBoolean(a["MeetingMilan"]),
                        UCSF = Convert.ToBoolean(a["UCSF"]),
                        ASAN = Convert.ToBoolean(a["ASAN"]),
                        other = Convert.ToBoolean(a["other"]),
                        otherDetail = a["otherDetail"].ToString(),
                        PtWillingForTransplant = Convert.ToBoolean(a["PtWillingForTransplant"]),
                        ReasonOfRefusal = a["ReasonOfRefusal"].ToString(),
                        MDTDiscussion = a["MDTDiscussion"].ToString(),
                        MDTOutCome = a["MDTOutCome"].ToString().Split(',').ToList(),
                        MDTOutComeString = a["MDTOutCome"].ToString(),
                        TreatmentTypeString = a["TreatmentType"].ToString(),
                        SiteOfHepaticDiseaseString = a["SiteOfHepaticDisease"].ToString(),

                        SiteOfAngioinvasion = a["SiteOfAngioinvasion"].ToString(),

                        MDTOutComeOther = a["MDTOutComeOther"].ToString(),
                        TransplantDiscussion = Convert.ToBoolean(a["TransplantDiscussion"]),
                        TransplantDiscussionDetail = a["TransplantDiscussionDetail"].ToString(),
                        //ImagingDate = Convert.ToDateTime(a["ImagingDate"]),

                        ImagingDate = Convert.ToDateTime(a["ImagingDate"] == DBNull.Value ? null : a["ImagingDate"]),



                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public List<Protocols> GetProtocolDetail(string procedureName, string schema, string name)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="ProtocolName", Type=DbType.Int32, Value=name }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        ProtocolId = Convert.ToInt32(a["ProtocolId"]),
                        ProtocoName = a["ProtocolName"].ToString(),
                        MedicineType = a["MedicineType"].ToString(),
                        Medicine = a["Medicine"].ToString(),
                        Dose = a["Dose"].ToString(),
                        DoseUnit = a["DoseUnit"].ToString(),
                        Route = a["Route"].ToString(),
                        Frequency = a["Frequency"].ToString(),
                        Duration = a["Duration"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public List<PatientServices> GetServiceDetails(string procedureName, string schema, string name)
        {
            List<PatientServices> Data = new List<PatientServices>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="serviceName", Type=DbType.Int32, Value=name }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new PatientServices()
                    {
                        ServiceId = Convert.ToInt32(a["ServiceId"]),
                        ServiceName = a["ServiceName"].ToString(),
                        Price = Convert.ToDouble(a["Price"]),
                        CptCode = a["CptCode"].ToString(),
                       
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<List<PatientServices>> GetServiceByName(string name)
        {
            List<PatientServices> Data = new List<PatientServices>();
            string Query = QueryHelper.GetQueryString("GetService", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="ServiceName", Type=DbType.String, Value=name }
            };
            try
            {
                DataSet ds = await _db.LoadDataSetAsync(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new PatientServices()
                    {
                        //ProtocolId = Convert.ToInt32(a["ProtocolId"]),
                        ServiceName = a["Value"].ToString(),

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<List<Protocols>> GetProtocolByName(string name)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString("GetProtocol", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="LookupType", Type=DbType.String, Value=name }
            };
            try
            {
                DataSet ds = await _db.LoadDataSetAsync(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        //ProtocolId = Convert.ToInt32(a["ProtocolId"]),
                        ProtocoName = a["Value"].ToString(),

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> insertUpdatePatientDetail(VitalHistory vitalHistory,string procedureName,string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientDetailId",Type=DbType.Int32,Value=vitalHistory.PatientDetailId },
                new DbParameters() {Name="PatientId", Type=DbType.Int32, Value=vitalHistory.PatientId },
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=vitalHistory.VisitId },
                new DbParameters() {Name="VisitDateTime",Type=DbType.DateTime,Value=vitalHistory.VisitDateTime },
                new DbParameters() {Name="VisitType",Type=DbType.String,Value=vitalHistory.VisitType },
                new DbParameters() {Name="CIC",Type=DbType.String,Value=vitalHistory.CIC },
                new DbParameters() {Name="ChildStatus",Type=DbType.String,Value=vitalHistory.ChildStatus },
                new DbParameters() {Name="MeldStatus",Type=DbType.String,Value=vitalHistory.MeldStatus },
                new DbParameters() {Name="BCLC",Type=DbType.String,Value=vitalHistory.BCLC },
                new DbParameters() {Name="ECOGStatus",Type=DbType.String,Value=vitalHistory.ECOGStatus },
                new DbParameters() {Name="HepStatus",Type=DbType.String,Value=vitalHistory.HepStatus },
                new DbParameters() {Name="AscitiesStatus",Type=DbType.String,Value=vitalHistory.AscitiesStatus },
              //  new DbParameters() {Name="OtherComorbities",Type=DbType.String,Value=vitalHistory.OtherComorbities },
                new DbParameters() {Name="CTReportConclusion",Type=DbType.String,Value=vitalHistory.CTReportConclusion },
                new DbParameters() {Name="Angioinvasion",Type=DbType.Boolean,Value=vitalHistory.Angioinvasion },
               // new DbParameters() {Name="SiteOfAngioinvasionVB",Type=DbType.String,Value=vitalHistory.SiteOfAngioinvasionVB },
                new DbParameters() {Name="ExtraHepaticDisease",Type=DbType.Boolean,Value=vitalHistory.ExtraHepaticDisease },
                //new DbParameters() {Name="SiteOfHepaticDisease",Type=DbType.String,Value=vitalHistory.SiteOfHepaticDisease },
                new DbParameters() {Name="LiverCirrhosis",Type=DbType.Boolean,Value=vitalHistory.LiverCirrhosis },
                new DbParameters() {Name="PortalHypertension",Type=DbType.Boolean,Value=vitalHistory.PortalHypertension },
                new DbParameters() {Name="Varices",Type=DbType.Boolean,Value=vitalHistory.Varices },
                new DbParameters() {Name="NumberOfLesion",Type=DbType.String,Value=vitalHistory.NumberOfLesion },
                new DbParameters() {Name="LargestLesionSize",Type=DbType.String,Value=vitalHistory.LargestLesionSize },
                new DbParameters() {Name="SVRStatus",Type=DbType.String,Value=vitalHistory.SVRStatus },
                new DbParameters() {Name="PreviousTreatment",Type=DbType.Boolean,Value=vitalHistory.PreviousTreatment },
                //new DbParameters() {Name="TreatmentType",Type=DbType.String,Value=vitalHistory.TreatmentType },
                new DbParameters() {Name="TreatmentResponse",Type=DbType.String,Value=vitalHistory.TreatmentResponse },
                new DbParameters() {Name="Diagnosis",Type=DbType.String,Value=vitalHistory.DiagnosisName },
                new DbParameters() {Name="DiagnosisDetails",Type=DbType.String,Value=vitalHistory.DiagnosisDetails },
                new DbParameters() {Name="MeetingMilan",Type=DbType.Boolean,Value=vitalHistory.MeetingMilan },
                new DbParameters() {Name="UCSF",Type=DbType.Boolean,Value=vitalHistory.UCSF },
                new DbParameters() {Name="ASAN",Type=DbType.Boolean,Value=vitalHistory.ASAN },
                new DbParameters() {Name="other",Type=DbType.Boolean,Value=vitalHistory.other },
                new DbParameters() {Name="otherDetail",Type=DbType.String,Value=vitalHistory.otherDetail },
                new DbParameters() {Name="PtWillingForTransplant",Type=DbType.Boolean,Value=vitalHistory.PtWillingForTransplant },
                new DbParameters() {Name="ReasonOfRefusal",Type=DbType.String,Value=vitalHistory.ReasonOfRefusal },
                new DbParameters() {Name="MDTDiscussion",Type=DbType.String,Value=vitalHistory.MDTDiscussion },
                //new DbParameters() {Name="MDTOutCome",Type=DbType.String,Value=vitalHistory.MDTOutCome },
                new DbParameters() {Name="EnteredBy",Type=DbType.String,Value=vitalHistory.EnteredBy },
                new DbParameters() {Name="EnteredOn",Type=DbType.String,Value=vitalHistory.EnteredOn },
                new DbParameters() {Name="ClinicId",Type=DbType.String,Value=vitalHistory.ClinicId },

                new DbParameters() {Name="MDTOutComeOther",Type=DbType.String,Value=vitalHistory.MDTOutComeOther },
                new DbParameters() {Name="TransplantDiscussion",Type=DbType.Boolean,Value=vitalHistory.TransplantDiscussion },
                new DbParameters() {Name="TransplantDiscussionDetail",Type=DbType.String,Value=vitalHistory.TransplantDiscussionDetail },
                new DbParameters() {Name="ImagingDate",Type=DbType.Date,Value=vitalHistory.ImagingDate },
                //new DbParameters() {Name="ImagingDate",Type=DbType.Date,Value=vitalHistory.ImagingDate == DateTime.MinValue ? null : vitalHistory.ImagingDate.ToString() },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
        //InsertOtherCormodities issues
        public async Task<int> InsertOtherComorbidities(int visitId, int patientId, string otherComorbidities)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertOtherCormodities", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="OtherComorbidity", Type=DbType.String, Value=otherComorbidities},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public async Task<int> InsertSiteOfAngioinvasion(int visitId, int patientId, string SiteOfAngioinvasion)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertSiteOfAngioinvasion", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="SiteOfAngioinvasion", Type=DbType.String, Value=SiteOfAngioinvasion},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
        public async Task<int> InsertMDTOutCome(int visitId, int patientId, string OutComeText)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertMDTOutCome", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="OutComeText", Type=DbType.String, Value=OutComeText},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public async Task<int> InsertSiteOfHepaticDisease(int visitId, int patientId, string SiteOfHepaticDiseaseText)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertSiteOfHepaticDisease", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="SiteOfHepaticDiseaseText", Type=DbType.String, Value=SiteOfHepaticDiseaseText},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
       public async Task<int> InsertTreatmentType(int visitId, int patientId, string TreatmentType)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertTreatmentType", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="TreatmentTypeText", Type=DbType.String, Value=TreatmentType},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }


        //public List<VitalHistory> GetAdvanceSearchResults(string procedureName, string schema, int parameter1, int paramter2)
        //{
        //    List<VitalHistory> Data = new List<VitalHistory>();
        //    string Query = QueryHelper.GetQueryString(procedureName, schema);
        //    List<DbParameters> pars = new List<DbParameters>()
        //    {
        //        new DbParameters() {Name="visitId",Type=DbType.Int32,Value=parameter1 },
        //        new DbParameters() {Name="patId",Type=DbType.Int32,Value=paramter2 },
        //    };
        //    try
        //    {
        //        DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
        //        if (ds?.Tables.Count > 0)
        //        {
        //            Data = ds.Tables[0].AsEnumerable().Select(a => new VitalHistory()
        //            {
        //                PatientDetailId = Convert.ToInt32(a["PatientDetailId"]),
        //                VisitId = Convert.ToInt32(a["VisitId"]),
        //                VisitDateTime = Convert.ToDateTime(a["VisitDateTime"]),
        //                VisitType = a["VisitType"].ToString(),
        //                CIC = a["CIC"].ToString(),
        //                ChildStatus = a["ChildStatus"].ToString(),
        //                MeldStatus = a["MeldStatus"].ToString(),
        //                BCLC = a["BCLC"].ToString(),
        //                ECOGStatus = a["ECOGStatus"].ToString(),
        //                HepStatus = a["HepStatus"].ToString(),
        //                AscitiesStatus = a["AscitiesStatus"].ToString(),
        //                OtherComorbities = a["OtherComorbities"].ToString().Split(',').ToList(),
        //                CTReportConclusion = a["CTReportConclusion"].ToString(),
        //                Angioinvasion = Convert.ToBoolean(a["Angioinvasion"]),
        //                SiteOfAngioinvasionVB = a["SiteOfAngioinvasionVB"].ToString().Split(',').ToList(),
        //                ExtraHepaticDisease = Convert.ToBoolean(a["ExtraHepaticDisease"]),
        //                SiteOfHepaticDisease = a["SiteOfHepaticDisease"].ToString(),
        //                LiverCirrhosis = Convert.ToBoolean(a["LiverCirrhosis"]),
        //                PortalHypertension = Convert.ToBoolean(a["PortalHypertension"]),
        //                Varices = Convert.ToBoolean(a["Varices"]),
        //                NumberOfLesion = a["NumberOfLesion"].ToString(),
        //                LargestLesionSize = a["LargestLesionSize"].ToString(),
        //                SVRStatus = a["SVRStatus"].ToString(),
        //                PreviousTreatment = Convert.ToBoolean(a["PreviousTreatment"]),
        //                TreatmentType = a["TreatmentType"].ToString(),
        //                TreatmentResponse = a["TreatmentResponse"].ToString(),
        //                DiagnosisName = a["Diagnosis"].ToString(),
        //                DiagnosisDetails = a["DiagnosisDetails"].ToString(),
        //                MeetingMilan = Convert.ToBoolean(a["MeetingMilan"]),
        //                UCSF = Convert.ToBoolean(a["UCSF"]),
        //                ASAN = Convert.ToBoolean(a["ASAN"]),
        //                other = Convert.ToBoolean(a["other"]),
        //                otherDetail = a["otherDetail"].ToString(),
        //                PtWillingForTransplant = Convert.ToBoolean(a["PtWillingForTransplant"]),
        //                ReasonOfRefusal = a["ReasonOfRefusal"].ToString(),
        //                MDTDiscussion = a["MDTDiscussion"].ToString(),
        //                MDTOutCome = a["MDTOutCome"].ToString().Split(',').ToList(),
        //            }).ToList();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }
        //    return Data;
        //}


        public async Task<int> insertUpdateLabReports(LabReports labReports,string procedureName,string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LabReportId", Type= DbType.Int32, Value= labReports.LabReportId },
                //new DbParameters() {Name="PreviousReportId",Type=DbType.Int32,Value=labReports.PreviousReportId },
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=labReports.VisitId },
                new DbParameters() {Name="PatientId", Type=DbType.Int32, Value=labReports.PatientId },
                new DbParameters() {Name="TestId", Type=DbType.Int32, Value=labReports.TestId == 0 ? null : labReports.TestId.ToString() },
                new DbParameters() {Name="TestName", Type=DbType.String, Value=labReports.TestName },
                new DbParameters() {Name="TestDate", Type=DbType.Date, Value=labReports.TestDate },
                new DbParameters() {Name="TestResult", Type=DbType.String, Value=labReports.TestResult },
                new DbParameters() {Name="TestUnit", Type=DbType.String, Value=labReports.TestUnit },
                new DbParameters() {Name="TestResultLabel", Type=DbType.String, Value=labReports.TestResultLabel },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> insertUpdateProtocol(Protocols protocol,string procedureName,string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="ProtocolDetailId", Type=DbType.Int32,Value=protocol.ProtocolDetailId },
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=protocol.VisitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=protocol.PatientId },
                new DbParameters() {Name="ProtocolId",Type=DbType.Int32,Value=protocol.ProtocolId == 0 ? null : protocol.ProtocolId.ToString() },
                new DbParameters() {Name="ProtocolName",Type=DbType.String,Value=protocol.ProtocoName },
                new DbParameters() {Name="MedicineType", Type=DbType.String, Value=protocol.MedicineType },
                new DbParameters() {Name="Medicine", Type=DbType.String, Value=protocol.Medicine },
                new DbParameters() {Name="Dose", Type=DbType.String, Value=protocol.Dose },
                new DbParameters() {Name="DoseUnit", Type=DbType.String, Value=protocol.DoseUnit },
                new DbParameters() {Name="Route", Type=DbType.String, Value=protocol.Route },
                new DbParameters() {Name="Frequency", Type=DbType.String, Value=protocol.Frequency },
                new DbParameters() {Name="Duration", Type=DbType.String, Value=protocol.Duration },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> insertUpdatePatientService(PatientServices patientServices, string procedureName, string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="ChargeServiceId", Type=DbType.Int32,Value=patientServices.ChargeServiceId },
                new DbParameters() {Name="ServiceId",Type=DbType.Int32,Value=patientServices.ServiceId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientServices.PatientId },
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=patientServices.VisitId },
                new DbParameters() {Name="Quantity",Type=DbType.String,Value=patientServices.Quantity },
                new DbParameters() {Name="Amount", Type=DbType.String, Value=patientServices.Amount },
                
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        public List<Visit> GetVisitHistory(string procedureName,string schema,int parameters,string mdtType)
        {
            List<Visit> Data = new List<Visit>();
            String Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=parameters },
                new DbParameters() {Name="MDTType",Type=DbType.String,Value=mdtType }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count>0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Visit
                    {
                        PatientId=Convert.ToInt32(a["PatientId"]),
                        VisitId=Convert.ToInt32(a["VisitId"]),
                        PatientName = a["PatientName"].ToString(),
                        VisitDate=Convert.ToDateTime(a["VisitDateTime"]),
                        Reason=a["Reason"].ToString(),
                        MDTType= a["MDTType"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Data;
        }

        public Users ValidateLogin(string procedureName, string schema, string loginId,string password)
        {
            Users Data = new Users();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LoginId",Type=DbType.String,Value=loginId },
                new DbParameters() {Name="Password",Type=DbType.String,Value=password },
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Users
                    {
                        LoginId = a["LoginId"].ToString(),
                        UserId=Convert.ToInt32(a["UserId"]),
                        UserFullName=a["UserFullName"].ToString(),
                        RoleId=Convert.ToInt32(a["RoleId"]),
                        Active =Convert.ToBoolean(a["Active"]),
                    }).FirstOrDefault();
                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public List<RoleRight> GetRoleRightsAgainstRole(string procedureName, string schema, int roleId)
        {
            List<RoleRight> Data = new List<RoleRight>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="RoleId",Type=DbType.Int32,Value=roleId}
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new RoleRight
                    {
                        RoleId  = Convert.ToInt32(a["RoleId"]),
                        RoleName = a["RoleName"].ToString(),
                        MenuId  = Convert.ToInt32(a["MenuId"]),
                        MenuName = a["MenuName"].ToString(),
                        MenuGroupName = a["MenuGroupName"].ToString(),
                        MenuParentId = Convert.ToInt32(a["MenuParentId"]),
                        CanView  = Convert.ToBoolean(a["CanView"]),
                        CanAddEdit = Convert.ToBoolean(a["CanAddEdit"]),
                        CanDelete = Convert.ToBoolean(a["CanDelete"])


                    }).ToList();

                }
            }
            catch (Exception)
            {

                throw;
            }
            return Data;
        }

        public async Task<int> CopyVisitData(int oldVisitId, int newVisitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("CopyVisitData", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="OldVisitId", Type=DbType.Int32,Value=oldVisitId },
                new DbParameters() {Name="NewVisitId",Type=DbType.Int32,Value=newVisitId },

            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        public async Task<int> CopyVisitDataBMT(int oldVisitId, int newVisitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("CopyVisitDataBMT", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="OldVisitId", Type=DbType.Int32,Value=oldVisitId },
                new DbParameters() {Name="NewVisitId",Type=DbType.Int32,Value=newVisitId },

            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }
        public List<Patient> GetFollowupPatientsForDate(DateTime date)
        {
            List<Patient> Data = new List<Patient>();
            string Query = QueryHelper.GetQueryString("GetPatientOnFollowupDate", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name = "p_Date", Type = DbType.Date, Value =date}

            };

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Patient()
                    {

                        PatientId = Convert.ToInt32(a["PatientId"]),
                        MrNumber = a["MrNumber"].ToString(),
                        FirstName = a["FirstName"].ToString(),
                        LastName = a["LastName"].ToString(),
                        FatherSpouseName = a["FatherSpouseName"].ToString(),
                        Gender = (a["Gender"]).ToString(),
                        DOB = Convert.ToDateTime(a["DOB"]),
                        EmailId = a["DOB"].ToString(),
                        Ethnicity = a["Ethnicity"].ToString(),
                        MobileNumber = a["MobileNumber"].ToString(),
                        ImageURL = a["ImageURL"].ToString(),
                        EnteredOn = Convert.ToDateTime(a["EnteredOn"]),
                    }).ToList();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return Data;
        }

        public List<LabReports> GetLabTestForGroupName(string groupName)
        {
            List<LabReports> Data = new List<LabReports>();
            string Query = QueryHelper.GetQueryString("GetLabTestAgainstGroup", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name="GroupName", Type=DbType.String, Value=groupName }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {

                    Data = ds.Tables[0].AsEnumerable().Select(a => new LabReports()
                    {
                        
                        TestId = Convert.ToInt32(a["TestId"] == DBNull.Value ? 0 : a["TestId"]),
                        TestName = a["TestName"].ToString(),

                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;
        }

        public List<LabReports> GetPreviousVisitLabTests(int patientId, int visitId)
        {
            List<LabReports> Data = new List<LabReports>();
            string Query = QueryHelper.GetQueryString("GetPreviousVisitLabTests", "Clinical");

            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name="PatientId", Type=DbType.Int32, Value=patientId},
                new DbParameters() { Name="VisitId", Type=DbType.Int32, Value=visitId },
            };

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {

                    Data = ds.Tables[0].AsEnumerable().Select(a => new LabReports()
                    {
                        TestName = a["TestName"].ToString(),
                        TestDate = Convert.ToDateTime(a["TestDate"] == DBNull.Value ? null : a["TestDate"]),
                        TestResult = a["TestResult"].ToString(),
                        VisitDateTime = Convert.ToDateTime(a["VisitDateTime"] == DBNull.Value ? null : a["VisitDateTime"]),

                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;
        }

        public List<PatientServices> GetPatientServiceAgainstVisit(int visitId)
        {
            List<PatientServices> Data = new List<PatientServices>();
            String Query = QueryHelper.GetQueryString("GetPatientServicesAgaintVisit", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=visitId}
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new PatientServices
                    {
                        ChargeServiceId = Convert.ToInt32(a["ChargeServiceId"]),
                        Amount = Convert.ToInt32(a["Amount"]),
                        PatientId = Convert.ToInt32(a["PatientId"]),
                        Quantity = Convert.ToInt32(a["Quantity"]),
                        ServiceId = Convert.ToInt32(a["ServiceId"]),
                        VisitId = Convert.ToInt32(a["VisitId"]),
                        ServiceName = a["ServiceName"].ToString(),
                        Price = Convert.ToDouble(a["Price"]),

                    }).ToList();
                    
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            return Data;
        }

        public async Task<int> DeleteProtocolDetail(int id)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteProtocolDetail", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="ProtocolDetailId", Type=DbType.Int32,Value=id }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public List<Protocols> GetFavoriteMedicineForUser(int userId)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString("GetFavoriteMedicineForUser", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="UserId", Type=DbType.Int32, Value=userId }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        MedicineType = a["MedicineType"].ToString(),
                        Medicine = a["MedicineName"].ToString(),
                        Dose = a["Dose"].ToString(),
                        DoseUnit = a["DoseUnit"].ToString(),
                        Route = a["Route"].ToString(),
                        Frequency = a["Frequency"].ToString(),
                        Duration = a["Duration"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public List<Protocols> SearchMedicineAndFavorites(string searchText, int userId)
        {
            List<Protocols> Data = new List<Protocols>();
            string Query = QueryHelper.GetQueryString("SearchMedicineAndFavorite", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="SearchText", Type=DbType.String, Value=searchText },
                new DbParameters { Name="UserId", Type=DbType.Int32, Value=userId }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new Protocols()
                    {
                        MedicineType = a["MedicineType"].ToString(),
                        Medicine = a["MedicineName"].ToString(),
                        Dose = a["Dose"].ToString(),
                        DoseUnit = a["DoseUnit"].ToString(),
                        Route = a["Route"].ToString(),
                        Frequency = a["Frequency"].ToString(),
                        Duration = a["Duration"].ToString(),
                        IsFavorite = Convert.ToBoolean(a["IsFavorite"]),
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> InsertMedicineFavorite(Protocols protocol)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertMedicineFavorite", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="MedicineName", Type=DbType.String,Value=protocol.Medicine },
                new DbParameters() {Name="MedicineType", Type=DbType.String,Value=protocol.MedicineType },
                new DbParameters() {Name="UserId", Type=DbType.Int32,Value=protocol.EnteredBy },
                new DbParameters() {Name="Dose", Type=DbType.String,Value=protocol.Dose },
                new DbParameters() {Name="DoseUnit", Type=DbType.String,Value=protocol.DoseUnit },
                new DbParameters() {Name="Frequency", Type=DbType.String,Value=protocol.Frequency },
                new DbParameters() {Name="Route", Type=DbType.String,Value=protocol.Route },
                new DbParameters() {Name="Duration", Type=DbType.String,Value=protocol.Duration },
            };

      
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DeleteFavoritemMedicine(int userId, string medicineName, string medicineType)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteUserFavoriteMedicine", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="UserId", Type=DbType.Int32,Value= userId },
                new DbParameters() {Name="MedicineName", Type=DbType.String,Value=medicineName},
                new DbParameters() {Name="MedicineType", Type=DbType.Int32,Value=medicineType },
            };


            try
            {
                result = await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars);
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> InsertUpdatePatientDiagnosis(PatientDiagnosis diagnosis)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertUpdatePatientDiagnosis", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientDiagnosisId", Type=DbType.Int32,Value=diagnosis.PatientDiagnosisId },
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=diagnosis.VisitId },
                new DbParameters() {Name="Diagnosis", Type=DbType.String,Value=diagnosis.Diagnosis },
                new DbParameters() {Name="EnteredBy", Type=DbType.Int32,Value=diagnosis.EnteredBy },

            };
            
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public List<PatientDiagnosis> GetPatientDiagnosisAgainstVisit(int visitId)
        {
            List<PatientDiagnosis> Data = new List<PatientDiagnosis>();
            string Query = QueryHelper.GetQueryString("GetPatientDiagnosisAgaintVisit", "dbo");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="VisitId", Type=DbType.Int32, Value=visitId},
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new PatientDiagnosis()
                    {
                        PatientDiagnosisId = Convert.ToInt32(a["PatientDiagnosisId"]),
                        VisitId = Convert.ToInt32(a["VisitId"]),
                        Diagnosis = a["Diagnosis"].ToString(),
                        IsActive = Convert.ToBoolean(a["IsActive"])

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> MarkPatientDiagnosisAsInactive(int patientDiagnosisId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("MarkDaignosisAsInactive", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientDiagnosisId", Type=DbType.Int32,Value=patientDiagnosisId},
            };

            try
            {
                result = await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars);
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> InsertPatientAlert(PatientAlert alert)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertPatientAlert", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientId", Type=DbType.Int32,Value=alert.PatientId},
                new DbParameters() {Name="AlertTitle", Type=DbType.String,Value=alert.AlertTitle},
                new DbParameters() {Name="AlertDescription", Type=DbType.String,Value=alert.AlertDescription},
                new DbParameters() {Name="ActiveFrom", Type=DbType.Date,Value=alert.ActiveFrom},
                new DbParameters() {Name="ActiveTill", Type=DbType.Date,Value=alert.ActiveTill},
                new DbParameters() {Name="IsActive", Type=DbType.Boolean,Value=alert.IsActive},
                new DbParameters() {Name="EnteredBy", Type=DbType.Int32,Value=alert.EnteredBy},
            };
            
            try
            {
                result = Convert.ToInt32(await  _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public List<PatientAlert> GetPatientActiveAlerts(int patientId)
        {
            List<PatientAlert> Data = new List<PatientAlert>();
            string Query = QueryHelper.GetQueryString("GetPatientAlerts", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="PatientId", Type=DbType.Int32, Value=patientId},
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new PatientAlert()
                    {
                        PatientAlertId = Convert.ToInt32(a["PatientAlertId"]),
                        AlertTitle = a["AlertTitle"].ToString(),
                        

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> MarkPatientAlertAsInactive(int patientAlertId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("MarkAlertAsInactive", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="AlertId", Type=DbType.Int32,Value=patientAlertId},
            };

            try
            {
                result = await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars);
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DiscontinueMedicine(int protocolDetailId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DiscontinueMedicine", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="ProtocolDetailId", Type=DbType.Int32,Value=protocolDetailId},
            };

            try
            {
                result = await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars);
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }


        #region Record searching


        public List<VitalHistory> GetVisitListAgainstSearch(int? ChildStatus, int? MeldStatus, int? BCLCStatus, int? ECOG, int? TreatmentResponse,
            int? MdtOutcome, int? HepatitisStatus, int? AscitiesStatus, int? Svr, int? OtherComorbidities, int? AngioInvasion, double? AFP, bool LiverCirrhosis, bool PortalHypertension, bool Varices, bool MeetingMilan, bool UCSF, bool ASAN)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            string Query = QueryHelper.GetQueryString("GetVisitListAgainstSearch", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name = "ChildStatus", Type = DbType.Int32, Value =ChildStatus},
                new DbParameters() {Name = "MeldStatus", Type = DbType.Int32, Value =MeldStatus},
                new DbParameters() {Name = "BCLCStatus", Type = DbType.Int32, Value =BCLCStatus},
                new DbParameters() {Name = "ECOG", Type = DbType.Int32, Value =ECOG},
                new DbParameters() {Name = "TreatmentResponse", Type = DbType.Int32, Value =TreatmentResponse},
                new DbParameters() {Name = "MdtOutcome", Type = DbType.Int32, Value =MdtOutcome},
                new DbParameters() {Name = "HepatitisStatus", Type = DbType.Int32, Value =HepatitisStatus},
                new DbParameters() {Name = "AscitiesStatus", Type = DbType.Int32, Value =AscitiesStatus},
                new DbParameters() {Name = "Svr", Type = DbType.Int32, Value =Svr},
                new DbParameters() {Name = "OtherComorbidities", Type = DbType.Int32, Value =OtherComorbidities},
                new DbParameters() {Name = "AngioInvasion", Type = DbType.Int32, Value =AngioInvasion},
                new DbParameters() {Name = "LiverCirrhosis", Type = DbType.Boolean, Value = LiverCirrhosis},
                new DbParameters() {Name = "AFP", Type = DbType.Double, Value =AFP},
                new DbParameters() {Name = "PortalHypertension", Type = DbType.Boolean, Value = PortalHypertension },
                new DbParameters() {Name = "Varices", Type = DbType.Boolean, Value = Varices },
                new DbParameters() {Name = "MeetingMilan", Type = DbType.Boolean, Value = MeetingMilan },
                new DbParameters() {Name = "UCSF", Type = DbType.Boolean, Value = UCSF },
                new DbParameters() {Name = "ASAN", Type = DbType.Boolean, Value = ASAN },
            };
            
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new VitalHistory()
                    {
                        PatientId = Convert.ToInt32(a["PatientId"]),
                        VisitId = Convert.ToInt32(a["VisitId"]),
                        PatientName = a["PatientName"]?.ToString(),
                        VisitDateTime = Convert.ToDateTime(a["VisitDate"]==DBNull.Value? DateTime.MinValue : a["VisitDate"]),
                        mdtType = a["MDTType"]?.ToString()
                    }).ToList();

                }
            }
            catch (Exception)
            {
                throw;
            }
            return Data;
        }

        #endregion

        public List<VitalHistory> GetPatientListForMDTList(DateTime fromDate, DateTime toDate)
        {
            List<VitalHistory> Data = new List<VitalHistory>();
            string Query = QueryHelper.GetQueryString("GetPatientListForMDTList", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="FromDate",Type=DbType.Date,Value=fromDate},
                new DbParameters() {Name="ToDate",Type=DbType.Date,Value=toDate },
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new VitalHistory()
                    {
                        PatientId = Convert.ToInt32(a["PatientId"]),
                        PatientName = a["PatientName"].ToString(),
                        VisitDateTime = Convert.ToDateTime(a["VisitDateTime"]),
                        HospitalNumber = a["HospitalNumber"].ToString(),
                        NumberOfLesionValue = a["NumberOfLesionValue"].ToString(),
                        NumberOfLesion = a["NumberOfLesionValue"].ToString(),
                        LargestLesionSize = a["LargestLesionSize"].ToString(),
                        ExtraHepaticDisease = Convert.ToBoolean(a["ExtraHepaticDisease"]),
                        Angioinvasion = Convert.ToBoolean(a["Angioinvasion"]),
                        MDTDiscussion = a["MDTDiscussion"].ToString(),
                        MDTOutComeString = a["MDTOutCome"].ToString(),
                        VisitId=Convert.ToInt32(a["VisitId"]),

                        
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }



        #region [BMT]
        #region [BMT Reporting DALS]
        public List<Patient> GetPatientByIdForRptBMT(int patId, string procedureName, string schema)
        {
            List<Patient> Data = new List<Patient>();
            Data.Add(GetPatientById(patId, procedureName, schema));
            return Data;

        }
        #endregion

        public List<VitalHistoryBMT> GetBMTPatientDetail(string procedureName, string schema, int parameter1, int paramter2)
        {
            List<VitalHistoryBMT> Data = new List<VitalHistoryBMT>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="visitId",Type=DbType.Int32,Value=parameter1 },
                new DbParameters() {Name="patId",Type=DbType.Int32,Value=paramter2 },
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new VitalHistoryBMT()
                    {
                        PatientDetailId = Convert.ToInt32(a["PatientDetailId"]),
                        VisitId = Convert.ToInt32(a["VisitId"]),
                        VisitDateTime = Convert.ToDateTime(a["VisitDateTime"]),
                        VisitType = a["VisitType"].ToString(),
                        CIC = a["CIC"].ToString(),
                        DiseaseStatusValReport= a["DiseaseStatusValue"].ToString(),
                        DiseaseStatus= a["DiseaseStatus"].ToString(),
                        LanskyValReport = a["LanskyValue"].ToString(),
                        Lansky = a["Lansky"].ToString(),
                        HCTCI = a["HCTCI"].ToString(),
                        HCTCIValReport = a["HCTCIValue"].ToString(),
                        ECOGStatusValReport= a["ECOGStatusValue"].ToString(),
                        ECOGStatus = a["ECOGStatus"].ToString(),
                        HepBStatusValReport = a["HepBStatusValue"].ToString(),
                        HepBStatus = a["HepBStatus"].ToString(),

                        HepCStatusValReport = a["HepCStatusValue"].ToString(),
                        HepCStatus = a["HepCStatus"].ToString(),
                        HIVStatusValReport = a["HIVStatusValue"].ToString(),
                        HIVStatus = a["HIVStatus"].ToString(),
                        Summary = a["Summary"].ToString(),
                        Stage = a["Stage"].ToString(),
                        //Stage = a["StageValue"].ToString(),

                        //RiskScore= a["RiskScoreValue"].ToString(),
                        RiskScore= a["RiskScore"].ToString(),
                        LiverBiopsy= a["LiverBiopsy"].ToString(),
                        PETCT= a["PETCT"].ToString(),
                        Chelation = a["Chelation"].ToString(),
                        Biopsy = a["Biopsy"].ToString(),
                        Liver= a["Liver"].ToString(),
                        Spleen= a["Spleen"].ToString(),
                        Lymphonodes = Convert.ToBoolean(a["Lymphonodes"]),
                        OtherComorbitiesBMT = a["OtherComorbidityIdBMTVB"].ToString().Split(',').ToList(),
                        SiteOfLymphnodes = a["SiteOfLymphNodesVB"].ToString().Split(',').ToList(), 
                        PreviousTreatment = Convert.ToBoolean(a["PreviousTreatment"]),
                        PreviousTreatmentBMT = a["PreviousTreatmentTypeBMTVB"].ToString().Split(',').ToList(),
                        TreatmentResponseValue = a["TreatmentResponse"].ToString(),
                        DiagnosisName = a["Diagnosis"].ToString(),
                        BMTTreatmentType = a["BMTTreatmentType"].ToString(),
                        PtWillingForTransplant = Convert.ToBoolean(a["PtWillingForTransplant"]),
                        ReasonOfRefusal = a["ReasonOfRefusal"].ToString(),
                        MDTDiscussion = a["MDTDiscussion"].ToString(),
                        MDTOutCome = a["MDTOutComeBMTVB"].ToString().Split(',').ToList(),
                        MDTOutComeOther = a["MDTOutComeOther"].ToString(),
                        TransplantDiscussion = Convert.ToBoolean(a["TransplantDiscussion"]),
                        TransplantDiscussionDetail = a["TransplantDiscussionDetail"].ToString(),
                        MDTDate= Convert.ToDateTime(a["MDTDate"]),
                        TreatmentType = a["TreatmentType"].ToString(),

                        //ImagingDate = Convert.ToDateTime(a["ImagingDate"] == DBNull.Value ? null : a["ImagingDate"]),



                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }
        public List<LabReportsBMT> GetLabOrdersBMT(string procedureName, string schema, int parameters)
        {
            List<LabReportsBMT> Data = new List<LabReportsBMT>();
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name="VisitId", Type=DbType.Int32, Value=parameters }
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {

                    Data = ds.Tables[0].AsEnumerable().Select(a => new LabReportsBMT()
                    {
                        LabReportIdBMT = Convert.ToInt32(a["LabReportIdBMT"]),
                        VisitId = ds.Tables[0].Columns.Contains("VisitId") ? Convert.ToInt32(a["VisitId"]) : 0,
                        //PreviousReportId=Convert.ToInt32(a["PreviousReportId"]),
                        TestId = Convert.ToInt32(a["TestId"] == DBNull.Value ? 0 : a["TestId"]),
                        TestName = a["TestName"].ToString(),
                        TestDate = Convert.ToDateTime(a["TestDate"]),
                        TestResult = a["TestResult"].ToString(),
                        TestUnit = a["TestUnit"].ToString(),
                        TestResultLabel = a["TestResultLabel"].ToString(),
                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;
        }
        public List<PatientDiagnosisBMT> GetPatientDiagnosisAgainstVisitBMT(int visitId)
        {
            List<PatientDiagnosisBMT> Data = new List<PatientDiagnosisBMT>();
            string Query = QueryHelper.GetQueryString("GetPatientDiagnosisAgaintVisitBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters { Name="VisitId", Type=DbType.Int32, Value=visitId},
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new PatientDiagnosisBMT()
                    {
                        PatientDiagnosisIdBMT= Convert.ToInt32(a["PatientDiagnosisIdBMT"]),
                        VisitId = Convert.ToInt32(a["VisitId"]),
                        Diagnosis = a["Diagnosis"].ToString(),
                        IsActive = Convert.ToBoolean(a["IsActive"]==DBNull.Value ? false: Convert.ToBoolean(a["IsActive"]))

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        public async Task<int> insertUpdatePatientDetailBMT(VitalHistoryBMT vitalHistory, string procedureName, string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientDetailId",Type=DbType.Int32,Value=vitalHistory.PatientDetailId },
                new DbParameters() {Name="PatientId", Type=DbType.Int32, Value=vitalHistory.PatientId },
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=vitalHistory.VisitId },
                new DbParameters() {Name="VisitDateTime",Type=DbType.DateTime,Value=vitalHistory.VisitDateTime },
                new DbParameters() {Name="VisitType",Type=DbType.String,Value=vitalHistory.VisitType },
                //new DbParameters() {Name="Diagnosis",Type=DbType.String,Value=vitalHistory.DiagnosisName },
                //new DbParameters() {Name="DiagnosisDetails",Type=DbType.String,Value=vitalHistory.DiagnosisDetails },
                new DbParameters() {Name="CIC",Type=DbType.String,Value=vitalHistory.CIC },
                new DbParameters() {Name="DiseaseStatus",Type=DbType.String,Value=vitalHistory.DiseaseStatus },
                new DbParameters() {Name="Lansky",Type=DbType.String,Value=vitalHistory.Lansky },
                new DbParameters() {Name="HCTCI",Type=DbType.String,Value=vitalHistory.HCTCI },
                new DbParameters() {Name="HepBStatus",Type=DbType.String,Value=vitalHistory.HepBStatus },
                new DbParameters() {Name="HepCStatus",Type=DbType.String,Value=vitalHistory.HepCStatus },
                new DbParameters() {Name="HIVStatus",Type=DbType.String,Value=vitalHistory.HIVStatus },
                new DbParameters() {Name="ECOGStatus",Type=DbType.String,Value=vitalHistory.ECOGStatus },
                //new DbParameters() {Name="OtherComorbities",Type=DbType.String,Value=vitalHistory.OtherComorbities },
                new DbParameters() {Name="Summary",Type=DbType.String,Value=vitalHistory.Summary },
                new DbParameters() {Name="Stage",Type=DbType.String,Value=vitalHistory.Stage },
                new DbParameters() {Name="RiskScore",Type=DbType.String,Value=vitalHistory.RiskScore },
                new DbParameters() {Name="Biopsy",Type=DbType.String,Value=vitalHistory.Biopsy },
                new DbParameters() {Name="PETCT",Type=DbType.String,Value=vitalHistory.PETCT },
                new DbParameters() {Name="Chelation",Type=DbType.String,Value=vitalHistory.Chelation },
                new DbParameters() {Name="Liver",Type=DbType.String,Value=vitalHistory.Liver },
                new DbParameters() {Name="Spleen",Type=DbType.String,Value=vitalHistory.Spleen },
                new DbParameters() {Name="Lymphonodes",Type=DbType.Boolean,Value=vitalHistory.Lymphonodes },
                //new DbParameters() {Name="SiteOfLymphnodes",Type=DbType.String,Value=vitalHistory.SiteOfLymphnodes },
                //new DbParameters() {Name="LiverBiopsy",Type=DbType.String,Value=vitalHistory.LiverBiopsy },
                new DbParameters() {Name="PreviousTreatment",Type=DbType.Boolean,Value=vitalHistory.PreviousTreatment },
                //new DbParameters() {Name="TreatmentType",Type=DbType.String,Value=vitalHistory.TreatmentType},
                new DbParameters() {Name="TreatmentResponse",Type=DbType.String,Value=vitalHistory.TreatmentResponse},
                new DbParameters() {Name="BMTTreatmentType",Type=DbType.String,Value=vitalHistory.BMTTreatmentType},
                new DbParameters() {Name="PtWillingForTransplant",Type=DbType.Boolean,Value=vitalHistory.PtWillingForTransplant },
                new DbParameters() {Name="ReasonOfRefusal",Type=DbType.String,Value=vitalHistory.ReasonOfRefusal },          
                new DbParameters() {Name="MDTDiscussion",Type=DbType.String,Value=vitalHistory.MDTDiscussion },
                //new DbParameters() {Name="MDTOutCome",Type=DbType.String,Value=vitalHistory.MDTOutCome},
                new DbParameters() {Name="MDTOutComeOther",Type=DbType.String,Value=vitalHistory.MDTOutComeOther },
                new DbParameters() {Name="TransplantDiscussion",Type=DbType.Boolean,Value=vitalHistory.TransplantDiscussion },
                new DbParameters() {Name="TransplantDiscussionDetail",Type=DbType.String,Value=vitalHistory.TransplantDiscussionDetail },
                new DbParameters() {Name="EnteredBy",Type=DbType.Int32,Value=vitalHistory.EnteredBy },
                new DbParameters() {Name="EnteredOn",Type=DbType.DateTime,Value=vitalHistory.EnteredOn },
                new DbParameters() {Name="ClinicId",Type=DbType.Int32,Value=vitalHistory.ClinicId },
                //new DbParameters() {Name="MDTDate",Type=DbType.Date,Value=vitalHistory.MDTDate == DateTime.MinValue ? null : vitalHistory.MDTDate.ToString() },
                new DbParameters() {Name="MDTDate",Type=DbType.Date,Value=vitalHistory.MDTDate },
                //public List<PatientDiagnosis> PatientDiagnosis { get; set; } = new List<PatientDiagnosis>();
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
        public async Task<int> insertUpdateLabReportsBMT(LabReportsBMT labReports, string procedureName, string schema)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="LabReportIdBMT", Type= DbType.Int32, Value= labReports.LabReportIdBMT },
                new DbParameters() {Name="VisitId",Type=DbType.Int32,Value=labReports.VisitId },
                new DbParameters() {Name="PatientId", Type=DbType.Int32, Value=labReports.PatientId },
                new DbParameters() {Name="TestId", Type=DbType.Int32, Value=labReports.TestId == 0 ? null : labReports.TestId.ToString() },
                new DbParameters() {Name="TestName", Type=DbType.String, Value=labReports.TestName },
                new DbParameters() {Name="TestDate", Type=DbType.Date, Value=labReports.TestDate },
                new DbParameters() {Name="TestResult", Type=DbType.String, Value=labReports.TestResult },
                new DbParameters() {Name="TestUnit", Type=DbType.String, Value=labReports.TestUnit },
                new DbParameters() {Name="TestResultLabel", Type=DbType.String, Value=labReports.TestResultLabel },
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
        public async Task<int> DeleteOtherComorbiditiesBMT(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteOtherComorbiditiesBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> InsertOtherComorbiditiesBMT(int visitId, int patientId, string otherComorbidities)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertOtherCormoditiesBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="OtherComorbidity", Type=DbType.String, Value=otherComorbidities},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }
        public async Task<int> DeleteSiteOfLymphnodesBMT(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteSiteOfLymphnodesBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> InsertSiteOfLymphnodesBMT(int visitId, int patientId, string SiteOfLymphnodes)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertSiteOfLymphnodesBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="SiteOfLymphnodes", Type=DbType.String, Value=SiteOfLymphnodes},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }

        public async Task<int> DeleteMDTOutcomeBMT(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteMDTOutcomeBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        //InsertMDTOutComeBMT
        public async Task<int> InsertMDTOutComeBMT(int visitId, int patientId, string OutComeText)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertMDTOutComeBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="OutComeText", Type=DbType.String, Value=OutComeText},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }


        public async Task<int> InsertUpdatePatientDiagnosisBMT(PatientDiagnosisBMT diagnosis)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertUpdatePatientDiagnosisBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientDiagnosisIdBMT", Type=DbType.Int32,Value=diagnosis.PatientDiagnosisIdBMT },
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=diagnosis.VisitId },
                new DbParameters() {Name="Diagnosis", Type=DbType.String,Value=diagnosis.Diagnosis },
                new DbParameters() {Name="EnteredBy", Type=DbType.Int32,Value=diagnosis.EnteredBy },
            };

            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DeletePreviousTreatmentBMT(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeletePreviousTreatmentBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> InsertPreviousTreatmentBMT(int visitId, int patientId, string PreviousTreatment)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("InsertPreviousTreatmentBMT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type= DbType.Int32, Value= visitId },
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=patientId },
                new DbParameters() {Name="PreviousTreatment", Type=DbType.String, Value=PreviousTreatment},
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteScalarAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {

                throw;
            }
            return result;
        }


        public List<LabReportsBMT> GetPreviousVisitLabTestsBMT(int patientId, int visitId)
        {
            List<LabReportsBMT> Data = new List<LabReportsBMT>();
            string Query = QueryHelper.GetQueryString("GetPreviousVisitLabTestsBMT", "Clinical");

            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() { Name="PatientId", Type=DbType.Int32, Value=patientId},
                new DbParameters() { Name="VisitId", Type=DbType.Int32, Value=visitId },
            };

            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {

                    Data = ds.Tables[0].AsEnumerable().Select(a => new LabReportsBMT()
                    {
                        TestName = a["TestName"].ToString(),
                        TestDate = Convert.ToDateTime(a["TestDate"] == DBNull.Value ? null : a["TestDate"]),
                        TestResult = a["TestResult"].ToString(),
                        VisitDateTime = Convert.ToDateTime(a["VisitDateTime"] == DBNull.Value ? null : a["VisitDateTime"]),


                    }).ToList();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;
        }

        #endregion


        #region Delete multi select patient details

        public async Task<int> DeleteSiteOfAngioInvasion(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteSiteOfAngioInvasion", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DeleteMDTOutcome(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteMDTOutcome", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DeleteSiteOfHepaticDisease(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteSiteOfHepaticDisease", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DeleteTreatmentType(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteTreatmentType", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        public async Task<int> DeleteOtherComorbidities(int visitId)
        {
            int result = 0;
            string Query = QueryHelper.GetQueryString("DeleteOtherComorbidities", "System");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="VisitId", Type=DbType.Int32,Value=visitId }
            };
            try
            {
                result = Convert.ToInt32(await _db.ExecuteNonQueryAsync(Query, CommandType.StoredProcedure, pars));
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }


        public List<VitalHistoryBMT> GetPatientListOfBMTForMDTList(DateTime fromDate, DateTime toDate)
        {
            List<VitalHistoryBMT> Data = new List<VitalHistoryBMT>();
            string Query = QueryHelper.GetQueryString("GetPatientListOfBMTForMDT", "Clinical");
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="FromDate",Type=DbType.Date,Value=fromDate},
                new DbParameters() {Name="ToDate",Type=DbType.Date,Value=toDate },
            };
            try
            {
                DataSet ds = _db.LoadDataSet(Query, CommandType.StoredProcedure, pars);
                if (ds?.Tables.Count > 0)
                {
                    Data = ds.Tables[0].AsEnumerable().Select(a => new VitalHistoryBMT()
                    {
                        PatientId = Convert.ToInt32(a["PatientId"]),
                        PatientName = a["PatientName"].ToString(),
                        VisitDateTime = Convert.ToDateTime(a["VisitDateTime"]),
                        HospitalNumber = a["HospitalNumber"].ToString(),
                        MDTDiscussion = a["MDTDiscussion"].ToString(),
                        MDTOutComeString = a["MDTOutCome"].ToString(),
                        VisitId = Convert.ToInt32(a["VisitId"]),

                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return Data;
        }

        #endregion


    }




}
