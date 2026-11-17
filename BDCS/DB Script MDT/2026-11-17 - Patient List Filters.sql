/*
	Patient Search screen: adds two filter modes instead of the hardcoded Top(20):
	- By Count: @FromDate IS NULL -> Top(@Count), same ordering as before (default 20,
	  unchanged default behaviour).
	- By Days: @FromDate IS NOT NULL -> filters EnteredOn between @FromDate and @ToDate
	  (inclusive), capped at Top(5000) as a sane safety net against an accidental huge
	  result set (e.g. "All").

	Safe to re-run.
*/

CREATE OR ALTER proc [Clinical].[PatientSearchByText]
@SearchText varchar(50),
@ClinicId int,
@LocationIds varchar(200) = '',
@Count int = 20,
@FromDate date = NULL,
@ToDate date = NULL
As

Begin
	IF @FromDate IS NOT NULL
	BEGIN
		Select top(5000)
		p.PatientId,p.MrNumber, p.HospitalNumber,p.FirstName,p.LastName,l.DisplayValue as Gender,p.Relationship,p.FatherSpouseName,p.DOB,p.CountryId,p.ProvinceId,
		p.DistrictId,p.EmailId,p.Ethnicity,p.MobileNumber,p.ContactNumber,p.[Address],p.ReferredBy,p.DiagnosisId,p.IsDiabeties,
		p.Comments,p.ImageURL,p.EnteredOn, isnull(p.DiscussionRequired,0) DiscussionRequired
		From Clinical.Patient p
		Inner Join System.Lookup l ON p.GenderId=l.LookupId
		Where (
			@SearchText=''
			OR CONCAT(p.FirstName,p.LastName) Like CONCAT('%',REPLACE(@SearchText,' ',''),'%')
			or p.MrNumber  Like CONCAT('%',REPLACE(@SearchText,' ',''),'%')
			or p.HospitalNumber  Like CONCAT('%',REPLACE(@SearchText,' ',''),'%')
			OR p.FatherSpouseName Like CONCAT('%',@SearchText,'%')
			or p.MobileNumber Like CONCAT('%',@SearchText,'%')
			OR p.Address Like CONCAT('%',@SearchText,'%')
		)
		And p.ClinicId=@ClinicId
		And (@LocationIds='' OR p.LocationId IS NULL OR p.LocationId IN (SELECT Id FROM System.SplitIds(@LocationIds)))
		And p.EnteredOn >= @FromDate And p.EnteredOn < DATEADD(day, 1, @ToDate)
		order by p.DiscussionRequired desc, p.EnteredOn desc
	END
	ELSE
	BEGIN
		Select top(@Count)
		p.PatientId,p.MrNumber, p.HospitalNumber,p.FirstName,p.LastName,l.DisplayValue as Gender,p.Relationship,p.FatherSpouseName,p.DOB,p.CountryId,p.ProvinceId,
		p.DistrictId,p.EmailId,p.Ethnicity,p.MobileNumber,p.ContactNumber,p.[Address],p.ReferredBy,p.DiagnosisId,p.IsDiabeties,
		p.Comments,p.ImageURL,p.EnteredOn, isnull(p.DiscussionRequired,0) DiscussionRequired
		From Clinical.Patient p
		Inner Join System.Lookup l ON p.GenderId=l.LookupId
		Where (
			@SearchText=''
			OR CONCAT(p.FirstName,p.LastName) Like CONCAT('%',REPLACE(@SearchText,' ',''),'%')
			or p.MrNumber  Like CONCAT('%',REPLACE(@SearchText,' ',''),'%')
			or p.HospitalNumber  Like CONCAT('%',REPLACE(@SearchText,' ',''),'%')
			OR p.FatherSpouseName Like CONCAT('%',@SearchText,'%')
			or p.MobileNumber Like CONCAT('%',@SearchText,'%')
			OR p.Address Like CONCAT('%',@SearchText,'%')
		)
		And p.ClinicId=@ClinicId
		And (@LocationIds='' OR p.LocationId IS NULL OR p.LocationId IN (SELECT Id FROM System.SplitIds(@LocationIds)))
		order by p.DiscussionRequired desc, p.EnteredOn desc
	END
End;
GO
