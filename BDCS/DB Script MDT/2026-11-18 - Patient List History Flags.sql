/*
	Patient Search list: when a user has both the Liver and Bone Marrow MDT
	rights, "Liver History" and "BMT History" were always shown as live links
	even for a patient who only ever had a visit under ONE of those types -
	clicking the other one just opened an empty history screen. Add
	HasLiverHistory / HasBoneMarrowHistory flags so the view can disable
	whichever link has nothing behind it, instead of making the user click in
	to find out.

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
		p.Comments,p.ImageURL,p.EnteredOn, isnull(p.DiscussionRequired,0) DiscussionRequired,
		CAST(CASE WHEN EXISTS (SELECT 1 FROM Clinical.PatientVisit pv WHERE pv.PatientId = p.PatientId AND pv.MDTType = 'Liver') THEN 1 ELSE 0 END AS bit) AS HasLiverHistory,
		CAST(CASE WHEN EXISTS (SELECT 1 FROM Clinical.PatientVisit pv WHERE pv.PatientId = p.PatientId AND pv.MDTType = 'BoneMarrow') THEN 1 ELSE 0 END AS bit) AS HasBoneMarrowHistory
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
		p.Comments,p.ImageURL,p.EnteredOn, isnull(p.DiscussionRequired,0) DiscussionRequired,
		CAST(CASE WHEN EXISTS (SELECT 1 FROM Clinical.PatientVisit pv WHERE pv.PatientId = p.PatientId AND pv.MDTType = 'Liver') THEN 1 ELSE 0 END AS bit) AS HasLiverHistory,
		CAST(CASE WHEN EXISTS (SELECT 1 FROM Clinical.PatientVisit pv WHERE pv.PatientId = p.PatientId AND pv.MDTType = 'BoneMarrow') THEN 1 ELSE 0 END AS bit) AS HasBoneMarrowHistory
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
