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

                        //EnteredBy=Convert.ToInt32(a["EnteredBy"]),
                        EnteredOn = Convert.ToDateTime(a["EnteredOn"]),
                        DiagnosisName=a["Diagnosis"].ToString(),
                        Rel=a["Rel"].ToString(),
                        

                    }).FirstOrDefault();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return Data;

        }


        //Bad way .. but it is working... dont tell Butt sb
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
                        LabReportId = Convert.ToInt32(a["CurrentReportId"]),
                        VisitId=ds.Tables[0].Columns.Contains("VisitId") ? Convert.ToInt32(a["VisitId"]) : 0,
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
                        DiagnosisName = a["Diagnosis"].ToString(),
                        DiagnosisDetails = a["DiagnosisDetails"].ToString(),
                        PastHxNAT = a["PastHxNAT"].ToString(),
                        PresentHx = a["PresentHx"].ToString(),
                        PastHx = a["PastHx"].ToString(),
                        FamilyHx = a["FamilyHx"].ToString(),
                        GynaeObsHx = a["GynaeObsHx"].ToString(),
                        SecHx = a["SecHx"].ToString(),
                        DrugHx = a["DrugHx"].ToString(),
                        TransfusionHx = a["TransfusionHx"].ToString(),
                        Comorbidites = a["Comorbidites"].ToString(),

                        SystemicCVS = a["SystemicCVS"].ToString(),
                        SystemicRes = a["SystemicRes"].ToString(),
                        SystemicLiver = a["SystemicLiver"].ToString(),
                        SystemicSpleen = a["SystemicSpleen"].ToString(),
                        Weight = a["Weight"].ToString(),
                        WeightUnit = a["WeightUnit"].ToString(),
                        Height = a["Height"].ToString(),
                        HeightUnit = a["HeightUnit"].ToString(),
                        Temp = a["Temp"].ToString(),
                        GPEJ = Convert.ToBoolean(a["GPEJ"]),
                        GPEA = Convert.ToBoolean(a["GPEA"]),
                        GPEL = Convert.ToBoolean(a["GPEL"]),
                        GPEC = Convert.ToBoolean(a["GPEC"]),
                        GPEK = Convert.ToBoolean(a["GPEK"]),
                        GPEE = Convert.ToBoolean(a["GPEE"]),
                        GPET = Convert.ToBoolean(a["GPET"]),
                        GPEComments = a["GPEComments"].ToString(),
                        BPSystolic = a["BPSystolic"].ToString(),
                        BPDistolic = a["BPDistolic"].ToString(),
                        NextFollowup = a["NextFollowup"].ToString(),
                        NextFollowupDate= Convert.ToDateTime(a["NextFollowupDate"] == DBNull.Value ? null : a["NextFollowupDate"]),
                        WeightUnitName=a["WeightUnitName"].ToString(),
                        HeightUnitName=a["HeightUnitName"].ToString(),


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
                new DbParameters() {Name="Diagnosis",Type=DbType.String,Value=vitalHistory.DiagnosisName },
                new DbParameters() {Name="DiagnosisDetails",Type=DbType.String,Value=vitalHistory.DiagnosisDetails },
                new DbParameters() {Name="PastHxNAT",Type=DbType.String,Value=vitalHistory.PastHxNAT },
                new DbParameters() {Name="PresentHx",Type=DbType.String,Value=vitalHistory.PresentHx },
                new DbParameters() {Name="PastHx",Type=DbType.String,Value=vitalHistory.PastHx },
                new DbParameters() {Name="FamilyHx",Type=DbType.String,Value=vitalHistory.FamilyHx },
                new DbParameters() {Name="GynaeObsHx",Type=DbType.String,Value=vitalHistory.GynaeObsHx },
                new DbParameters() {Name="SecHx",Type=DbType.String,Value=vitalHistory.SecHx },
                new DbParameters() {Name="DrugHx",Type=DbType.String,Value=vitalHistory.DrugHx },
                new DbParameters() {Name="TransfusionHx",Type=DbType.String,Value=vitalHistory.TransfusionHx },
                new DbParameters() {Name="Comorbidites",Type=DbType.String,Value=vitalHistory.Comorbidites },
                new DbParameters() {Name="SystemicCVS",Type=DbType.String,Value=vitalHistory.SystemicCVS },
                new DbParameters() {Name="SystemicRes",Type=DbType.String,Value=vitalHistory.SystemicRes },
                new DbParameters() {Name="SystemicLiver",Type=DbType.String,Value=vitalHistory.SystemicLiver },
                new DbParameters() {Name="SystemicSpleen",Type=DbType.String,Value=vitalHistory.SystemicSpleen },
                new DbParameters() {Name="Weight",Type=DbType.String,Value=vitalHistory.Weight },
                new DbParameters() {Name="WeightUnit",Type=DbType.String,Value=vitalHistory.WeightUnit },
                new DbParameters() {Name="Height",Type=DbType.String,Value=vitalHistory.Height },
                new DbParameters() {Name="HeightUnit",Type=DbType.String,Value=vitalHistory.HeightUnit },
                new DbParameters() {Name="GPEJ",Type=DbType.Boolean,Value=vitalHistory.GPEJ },
                new DbParameters() {Name="GPEA",Type=DbType.Boolean,Value=vitalHistory.GPEA },
                new DbParameters() {Name="GPEL",Type=DbType.Boolean,Value=vitalHistory.GPEL },
                new DbParameters() {Name="GPEC",Type=DbType.Boolean,Value=vitalHistory.GPEC },
                new DbParameters() {Name="GPEK",Type=DbType.Boolean,Value=vitalHistory.GPEK },
                new DbParameters() {Name="GPEE",Type=DbType.Boolean,Value=vitalHistory.GPEE },
                new DbParameters() {Name="GPET",Type=DbType.Boolean,Value=vitalHistory.GPET },
                new DbParameters() {Name="GPEComments",Type=DbType.String,Value=vitalHistory.GPEComments },
                new DbParameters() {Name="BPSystolic",Type=DbType.String,Value=vitalHistory.BPSystolic },
                new DbParameters() {Name="BPDistolic",Type=DbType.String,Value=vitalHistory.BPDistolic },
                new DbParameters() {Name="Temp",Type=DbType.String,Value=vitalHistory.Temp },
                new DbParameters() {Name="NextFollowup",Type=DbType.String,Value=vitalHistory.NextFollowup },
                new DbParameters() {Name="NextFollowupDate",Type=DbType.Date,Value=vitalHistory.NextFollowupDate },
                new DbParameters() {Name="EnteredBy",Type=DbType.String,Value=vitalHistory.EnteredBy },
new DbParameters() {Name="EnteredOn",Type=DbType.String,Value=vitalHistory.EnteredOn },
new DbParameters() {Name="ClinicId",Type=DbType.String,Value=vitalHistory.ClinicId },
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
        public List<Visit> GetVisitHistory(string procedureName,string schema,int parameters)
        {
            List<Visit> Data = new List<Visit>();
            String Query = QueryHelper.GetQueryString(procedureName, schema);
            List<DbParameters> pars = new List<DbParameters>()
            {
                new DbParameters() {Name="PatientId",Type=DbType.Int32,Value=parameters }
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

    }


}
