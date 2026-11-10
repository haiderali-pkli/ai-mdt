/*
	Step 3 - Location-based patient visibility.

	Goal: a patient can be tagged with the Location (physical hospital site - System.Location,
	added in "2026-11-10 - Location Management.sql") they were registered at. A user should only
	be able to see/open a patient tagged with a Location outside the set of Locations assigned to
	them (System.UserLocation).

	Backward-compatible by design:
	  - Clinical.Patient.LocationId is NULLABLE. Existing patients (and any new patient whose
	    operator has no Location assigned, or picks none) keep LocationId = NULL, which means
	    "visible to everyone" - exactly today's behaviour. Nothing already in the system becomes
	    invisible because of this change.
	  - A user with NO Locations assigned (@LocationIds = '') is treated as unrestricted - sees
	    everything, same as before this feature existed. Restriction only kicks in for a user once
	    an admin has actually assigned them one or more Locations.
	  - A user with Locations assigned sees patients whose LocationId is NULL (legacy/unassigned)
	    OR whose LocationId is one of their assigned Locations. Everything else is filtered out.

	Additive only: no existing table, column, proc or view is dropped. Procs listed below are
	recreated (DROP + CREATE) to add the new, optional @LocationIds parameter - existing callers
	that don't pass it get '' and see the old, unrestricted behaviour.
	Safe to re-run (every block is guarded).
*/

-- 1. Clinical.Patient.LocationId (new, nullable column + FK)
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Clinical.Patient') AND name = 'LocationId')
BEGIN
	ALTER TABLE Clinical.Patient ADD LocationId int NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Patient_Location')
BEGIN
	ALTER TABLE Clinical.Patient WITH CHECK ADD CONSTRAINT FK_Patient_Location FOREIGN KEY (LocationId) REFERENCES System.Location (LocationId);
	ALTER TABLE Clinical.Patient CHECK CONSTRAINT FK_Patient_Location;
END
GO

-- 2. InsertUpdatePatient - stamp new/updated patients with their operator's Location
IF OBJECT_ID('Clinical.InsertUpdatePatient') IS NOT NULL
	DROP PROCEDURE Clinical.InsertUpdatePatient;
GO
CREATE Proc [Clinical].[InsertUpdatePatient]
@PatientId int
,@FirstName varchar(50)
,@LastName varchar(50)=null
,@Relationship varchar(50)=null
,@FatherSpouseName varchar(50)=null
,@GenderId int
,@DOB datetime=null
,@BloodGroupId int=0
,@CountryId int
,@ProvinceId int
,@DistrictId int
,@City varchar(50)
,@EmailId varchar(50)
,@Ethnicity varchar(50)
,@MobileNumber varchar(50)
,@ContactNumber varchar(50)
,@Address varchar(50)
,@ReferredBy varchar(50)
,@DiagnosisId int=271
,@IsDiabeties int
,@Comments varchar(50)
,@ImageURL varchar(50)
,@IsActive int
,@EnteredBy int
,@EnteredOn datetime
,@UpdatedBy int
,@UpdatedOn datetime
,@ClinicId int
,@HospitalNumber varchar(50)=null
,@LocationId int=null
As

Begin
if @PatientId<=0
	Begin
	Declare @v_MaxNumber int,@vaMrNumber varchar(50), @DiagnosisCode varchar(50);
	Set @v_MaxNumber=(Select IsNull(Max(PatientId),0)+1 From Clinical.Patient);
	Set @DiagnosisCode = (select lu.ReturnValue from System.Lookup lu where lu.LookupId = @DiagnosisId );
	Select @vaMrNumber=Concat(Right(CONCAT('0',@GenderId),2), '-', @DiagnosisCode,'-', Right(CONCAT('000000',@v_MaxNumber),6));

	Select @vaMrNumber=Concat(@DiagnosisCode,'-', format(getdate(), 'MM-yy'), '-' , Right(CONCAT('000000',@v_MaxNumber),6));
	--Select @vaMrNumber=Concat(format(getdate(), 'MM-yy'), '-' , Right(CONCAT('000000',@v_MaxNumber),6));

	Insert into Clinical.Patient
	(
	FirstName
	,LastName
	,MrNumber
	,Relationship
	,FatherSpouseName
	,GenderId
	,DOB
	,BloodGroupId
	,CountryId
	,ProvinceId
	,DistrictId
	,City
	,EmailId
	,Ethnicity
	,MobileNumber
	,ContactNumber
	,[Address]
	,ReferredBy
	,DiagnosisId
	,IsDiabeties
	,Comments
	,ImageURL
	,IsActive
	,EnteredBy
	,EnteredOn
	,UpdatedBy
	,UpdatedOn
	,ClinicId
	,HospitalNumber
	,LocationId
	)
	Values
	(
	@FirstName
	,@LastName
	,@vaMrNumber
	,@Relationship
	,@FatherSpouseName
	,@GenderId
	,@DOB
	,@BloodGroupId
	,@CountryId
	,@ProvinceId
	,@DistrictId
	,@City
	,@EmailId
	,@Ethnicity
	,@MobileNumber
	,@ContactNumber
	,@Address
	,@ReferredBy
	,@DiagnosisId
	,@IsDiabeties
	,@Comments
	,@ImageURL
	,@IsActive
	,@EnteredBy
	,SYSDATETIME()
	,@UpdatedBy
	,@UpdatedOn
	,@ClinicId
	,@HospitalNumber
	,@LocationId
	);
	Select SCOPE_IDENTITY();
	End
Else
	Begin
	Update Top(1) Clinical.Patient
	Set FirstName=@FirstName
	,LastName=@LastName
	,Relationship=@Relationship
	,FatherSpouseName=@FatherSpouseName
	,GenderId=@GenderId
	,DOB=@DOB
	,BloodGroupId=@BloodGroupId
	,CountryId=@CountryId
	,ProvinceId=@ProvinceId
	,DistrictId=@DistrictId
	,City=@City
	,EmailId=@EmailId
	,Ethnicity=@Ethnicity
	,MobileNumber=@MobileNumber
	,ContactNumber=@ContactNumber
	,[Address]=@Address
	,ReferredBy=@ReferredBy
	,DiagnosisId=@DiagnosisId
	,IsDiabeties=@IsDiabeties
	,Comments=@Comments
	,ImageURL=@ImageURL
	,IsActive=@IsActive
	,UpdatedOn=@UpdatedOn
	,ClinicId=@ClinicId
	,HospitalNumber = @HospitalNumber
	,LocationId = ISNULL(@LocationId, LocationId)
	Where PatientId=@PatientId;
	Select @PatientId;
	End
End
GO

-- 3. PatientSearchByText - filter the search/list results by the caller's assigned Locations
IF OBJECT_ID('Clinical.PatientSearchByText') IS NOT NULL
	DROP PROCEDURE Clinical.PatientSearchByText;
GO
CREATE proc [Clinical].[PatientSearchByText]
@SearchText varchar(50),
@ClinicId int,
@LocationIds varchar(200) = ''
As

Begin
Select top(20)
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
-- NOTE: these two used to be chained onto the OR group above with a bare "And" - because AND binds
-- tighter than OR in T-SQL, that meant ClinicId (and now LocationIds) was only ever actually enforced
-- for the last OR branch (Address match); every other match (name/MRN/hospital number/mobile) bypassed
-- it entirely. Parenthesizing the search-text OR group fixes that for both filters.
And p.ClinicId=@ClinicId
And (@LocationIds='' OR p.LocationId IS NULL OR p.LocationId IN (SELECT Id FROM System.SplitIds(@LocationIds)))
order by p.DiscussionRequired desc, p.EnteredOn desc

End;
GO

-- 4. GetPatientById - block direct-by-id access to a patient outside the caller's assigned Locations
IF OBJECT_ID('Clinical.GetPatientById') IS NOT NULL
	DROP PROCEDURE Clinical.GetPatientById;
GO
CREATE proc [Clinical].[GetPatientById]
@PatientId varchar(50),
@LocationIds varchar(200) = ''
As

Begin
Select
p.PatientId,p.MrNumber, p.FirstName,p.LastName,p.Relationship,p.FatherSpouseName,p.DOB,isnull(l.LookupId,0) LookupId,l.DisplayValue as Gender, isnull(c.COUNTRYID,0) COUNTRYID,c.COUNTRYNAME as Country,isnull(pr.PROVINCEID,0) PROVINCEID,pr.PROVINCENAME as Province,
isnull(d.DISTRICTID,0) DISTRICTID,d.DISTRICTNAME as District,p.EmailId,p.Ethnicity,p.MobileNumber,p.ContactNumber,p.[Address],p.ReferredBy,isnull(p.DiagnosisId,0) DiagnosisId,p.IsDiabeties, isnull(p.BloodGroupId,0) BloodGroupId,bg.DisplayValue BloodGroupName,
p.Comments,p.ImageURL,p.EnteredOn,ld.DisplayValue Diagnosis,rel.DisplayValue Rel, p.ImageURL, p.HospitalNumber, p.LocationId
From Clinical.Patient p
left Join System.Lookup l
On p.GenderId=l.LookupId

left Join System.Country c
On p.CountryId=c.COUNTRYID

left Join System.Province pr
On p.ProvinceId=pr.PROVINCEID

left join System.District d

on p.DistrictId=d.DISTRICTID
left join System.Lookup ld on p.DiagnosisId=ld.LookupId
left join System.Lookup Rel on p.Relationship=rel.LookupId
left join System.Lookup bg on p.BloodGroupId=bg.LookupId

Where p.PatientId=@PatientId
And (@LocationIds='' OR p.LocationId IS NULL OR p.LocationId IN (SELECT Id FROM System.SplitIds(@LocationIds)));

End;
GO

-- 5. MDT meeting-list screens - same Location filter applied to the patients they surface
IF OBJECT_ID('Clinical.GetPatientListForMDTList') IS NOT NULL
	DROP PROCEDURE Clinical.GetPatientListForMDTList;
GO
CREATE proc [Clinical].[GetPatientListForMDTList]
@FromDate date,
@ToDate date,
@LocationIds varchar(200) = ''
as
begin

	select p.PatientId, CONCAT(p.FirstName, ' ' , p.lastname) as PatientName, p.HospitalNumber,
	pd.VisitDateTime , isnull(pd.NumberOfLesion,0) NumberOfLesion, isnull(pd.LargestLesionSize,0) LargestLesionSize,
	isnull(pd.ExtraHepaticDisease,0) ExtraHepaticDisease, isnull(pd.Angioinvasion,0) Angioinvasion,dbo.GetLookUpValueAgainstId(pd.NumberOfLesion) as NumberOfLesionValue,
	pd.MDTDiscussion
	    , stuff((
        select ',' + mo.OutcomeText
        from Clinical.MDTOutcome mo
        where mo.VisitId = pd.VisitId
        order by mo.OutcomeText
        for xml path('')
    ),1,1,'') as MDTOutCome,
	pd.VisitId

	from Clinical.Patient p
	inner join Clinical.PatientDetail pd
		on p.PatientId = pd.PatientId

	WHERE  (cast(pd.VisitDateTime as date) between @FromDate and @ToDate )


	and (MDTOutcome is not null or pd.MDTDiscussion is not null  or pd.CTReportConclusion is not null )
		AND (P.DiscussionRequired=1)
		AND (@LocationIds='' OR p.LocationId IS NULL OR p.LocationId IN (SELECT Id FROM System.SplitIds(@LocationIds)));

end
GO

IF OBJECT_ID('Clinical.GetPatientListOfBMTForMDT') IS NOT NULL
	DROP PROCEDURE Clinical.GetPatientListOfBMTForMDT;
GO
CREATE proc [Clinical].[GetPatientListOfBMTForMDT]
  @FromDate DATE,
    @ToDate DATE,
	@LocationIds varchar(200) = ''
as
begin
	select
		p.PatientId, ISNULL(p.DiscussionRequired,0) AS DiscussionRequired,
		CONCAT(p.FirstName, ' ' , p.lastname) as PatientName,
		p.HospitalNumber,

		pd.VisitDateTime , isnull(pd.DiseaseStatus,0) DiseaseStatus,
		isnull(pd.Diagnosis,0) Diagnosis,
		isnull(pd.Lansky,0) Lansky,
		isnull(pd.HCTCI,0) HCTCI,
		--dbo.GetLookUpValueAgainstId(pd.NumberOfLesion) as NumberOfLesionValue,
		pd.MDTDiscussion
	    , stuff((
    select
		',' + mo.OutcomeText
        from Clinical.MDTOutcomeBMT mo
        where mo.VisitId = pd.VisitId
        order by mo.OutcomeText
        for xml path('')
    ),1,1,'') as MDTOutCome,
	pd.VisitId

	from Clinical.Patient p
	inner join Clinical.PatientDetailBMT pd
		on p.PatientId = pd.PatientId
	--LEFT JOIN Clinical.PatientVisit pv
	--ON pd.VisitId=pv.VisitId


	where (cast(pd.VisitDateTime as date) between @FromDate and @ToDate )
	and (MDTOutcome is not null or pd.MDTDiscussion is not NULL)
	AND (P.DiscussionRequired=1)
	AND (@LocationIds='' OR p.LocationId IS NULL OR p.LocationId IN (SELECT Id FROM System.SplitIds(@LocationIds)));

end
GO
