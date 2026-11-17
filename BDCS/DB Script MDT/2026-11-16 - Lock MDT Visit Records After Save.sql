/*
	Feature: once a consultant saves the MDT visit record (Clinical.PatientDetail for
	Liver, Clinical.PatientDetailBMT for BMT), it becomes locked against further edits.
	Only a user holding the new "Unlock MDT Record" right (granted to Admin by default,
	but assignable to any role via the normal Role Rights screen) can save changes to an
	already-saved record. Enforced server-side in PatientController.savePatientDetail1 /
	savePatientDetail1BMT via SessionHelper.IsUserAuthorized - the UI check is just a
	courtesy, not the security boundary.

	Safe to re-run.
*/

-- 1. Lock flag on both MDT record tables.
IF COL_LENGTH('Clinical.PatientDetail', 'IsLocked') IS NULL
	ALTER TABLE Clinical.PatientDetail ADD IsLocked bit NOT NULL CONSTRAINT DF_PatientDetail_IsLocked DEFAULT (0);
GO

IF COL_LENGTH('Clinical.PatientDetailBMT', 'IsLocked') IS NULL
	ALTER TABLE Clinical.PatientDetailBMT ADD IsLocked bit NOT NULL CONSTRAINT DF_PatientDetailBMT_IsLocked DEFAULT (0);
GO

-- Records saved before this feature existed are still "already saved" - lock them too.
UPDATE Clinical.PatientDetail SET IsLocked = 1 WHERE IsLocked = 0;
UPDATE Clinical.PatientDetailBMT SET IsLocked = 1 WHERE IsLocked = 0;
GO

-- 2. New permission: "Unlock MDT Record". Lives in the Patient group next to
--    Create Visit / Visit History so it shows up in the same Role Rights section.
IF NOT EXISTS (SELECT 1 FROM System.Menu WHERE MenuName = 'Unlock MDT Record')
	INSERT INTO System.Menu (MenuName, MenuGroupName, Url, MenuParentId, Active)
	VALUES ('Unlock MDT Record', 'Patient', NULL, 1, 1);
GO

-- Granted to Admin by default; any other role can be granted it from the Role Rights screen.
INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete)
SELECT 1, m.MenuId, 1, 1, 0
FROM System.Menu m
WHERE m.MenuName = 'Unlock MDT Record'
AND NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 1 AND MenuId = m.MenuId);
GO

-- 3. Keep returning IsLocked from the read procs the Edit screens load from.
CREATE OR ALTER PROCEDURE [Clinical].[GetPatientDetail]
	@visitId int,
	@patId int
As
Begin
Select d.PatientDetailId
	  ,d.PatientId
	  ,d.VisitId
	  ,d.VisitDateTime
	  ,d.VisitType
      ,d.CIC
      ,d.ChildStatus
	  ,dbo.GetLookUpValueAgainstId(d.ChildStatus) as ChildStatusValue
      ,d.MeldStatus
	  ,dbo.GetLookUpValueAgainstId(d.MeldStatus) as MeldStatusValue
      ,d.BCLC
	  ,dbo.GetLookUpValueAgainstId(d.BCLC) as BCLCValue
      ,d.ECOGStatus
	  ,dbo.GetLookUpValueAgainstId(d.ECOGStatus) as ECOGStatusValue
      ,d.HepStatus
	  ,dbo.GetLookUpValueAgainstId(d.HepStatus) as HepStatusValue
      ,d.AscitiesStatus
	  ,dbo.GetLookUpValueAgainstId(d.AscitiesStatus) as AscitiesStatusValue
      ,d.CTReportConclusion
      ,d.Angioinvasion
	  , stuff((
        select ',' + sa.SiteOfAngioinvasion
        from Clinical.SiteOfAngioinvasion sa
        where sa.VisitId = d.VisitId
        order by sa.SiteOfAngioinvasion
        for xml path('')
    ),1,1,'') as SiteOfAngioinvasionVB
      ,d.ExtraHepaticDisease
	  , stuff((
        select ',' + Sd.SiteOfHepaticDiseaseText
        from Clinical.SiteOfHepaticDisease Sd
        where Sd.VisitId = d.VisitId
        order by Sd.SiteOfHepaticDiseaseText
        for xml path('')
    ),1,1,'') as SiteOfHepaticDisease
      ,d.LiverCirrhosis
      ,d.PortalHypertension
      ,d.Varices
      ,d.NumberOfLesion NumberOfLesion
	  ,dbo.GetLookUpValueAgainstId(d.NumberOfLesion) as NumberOfLesionValue
      , d.LargestLesionSize
      ,d.SVRStatus
	  ,dbo.GetLookUpValueAgainstId(d.SVRStatus) as SVRStatusValue
      ,d.PreviousTreatment
	  , stuff((
        select ',' + tt.TreatmentTypeText
        from Clinical.TreatmentType tt
        where tt.VisitId = d.VisitId
        order by tt.TreatmentTypeText
        for xml path('')
    ),1,1,'') as TreatmentType
      ,d.TreatmentResponse
	  ,dbo.GetLookUpValueAgainstId(d.TreatmentResponse) as TreatmentResponseValue
	  ,d.diagnosis
	  ,d.diagnosisdetails
      ,d.MeetingMilan
      ,d.UCSF
      ,d.ASAN
      ,d.other
      ,d.otherDetail
      ,d.PtWillingForTransplant
      ,d.ReasonOfRefusal
      ,d.MDTDiscussion
	  , stuff((
        select ',' + mo.OutcomeText
        from Clinical.MDTOutcome mo
        where mo.VisitId = d.VisitId
        order by mo.OutcomeText
        for xml path('')
    ),1,1,'') as MDTOutCome
	  , stuff((
        select ',' + oc.OtherComorbidity
        from Clinical.OtherComorbidities oc
        where oc.VisitId = d.VisitId
        order by oc.OtherComorbidity
        for xml path('')
    ),1,1,'') as OtherComorbities,
	stuff((
        select ',' + sai.SiteOfAngioinvasion
        from Clinical.SiteOfAngioinvasion sai
        where sai.VisitId = d.VisitId
        order by sai.SiteOfAngioinvasion
        for xml path('')
    ),1,1,'') as SiteOfAngioinvasion,
	d.MDTOutComeOther,
	isnull(d.TransplantDiscussion,0) TransplantDiscussion,
	d.TransplantDiscussionDetail,
	d.ImagingDate,
	d.IsLocked
	  From Clinical.PatientDetail d
	  Where d.VisitId=@visitId And d.PatientId=@patId;
End
GO

CREATE OR ALTER PROCEDURE [Clinical].[GetPatientDetailBMT]
	@visitId int,
	@patId int
As
Begin
Select d.PatientDetailId
	  ,d.PatientId
	  ,d.VisitId
	  ,d.VisitDateTime
	  ,d.VisitType
      ,d.CIC
	  ,d.Diagnosis
      ,d.DiseaseStatus
	  ,dbo.GetLookUpValueAgainstId(d.DiseaseStatus) as DiseaseStatusValue
	  ,d.Lansky
	  ,dbo.GetLookUpValueAgainstId(d.Lansky) as LanskyValue
	  ,d.HCTCI
	  ,dbo.GetLookUpValueAgainstId(d.HCTCI) as HCTCIValue
	  ,d.ECOGStatus
	  ,dbo.GetLookUpValueAgainstId(d.ECOGStatus) as ECOGStatusValue
	  ,d.HepBStatus
	  ,dbo.GetLookUpValueAgainstId(d.HepBStatus) as HepBStatusValue
	  ,d.HepCStatus
	  ,dbo.GetLookUpValueAgainstId(d.HepCStatus) as HepCStatusValue
	  ,d.HIVStatus
      ,dbo.GetLookUpValueAgainstId(d.HIVStatus) as HIVStatusValue
	  ,d.OtherComorbities
	  ,d.Summary
	  ,d.Stage
	  ,dbo.GetLookUpValueAgainstId(d.Stage) as StageValue
	  ,d.RiskScore
	  ,d.Biopsy
	  ,d.PETCT
	  ,d.Chelation
	  ,d.Liver
	  ,d.Spleen
	  ,d.Lymphonodes
	  ,dbo.GetLookUpValueAgainstId(d.SiteOfLymphnodes) as SiteOfLymphnodes
	  ,d.SiteOfLymphnodes
	  ,d.LiverBiopsy
	  ,d.PreviousTreatment
	  ,d.TreatmentType
	  ,d.TreatmentResponse
	  ,d.BMTTreatmentType
	  ,d.PtWillingForTransplant
	  ,d.ReasonOfRefusal
	  ,d.MDTDiscussion
	  ,d.MDTOutCome
	  ,d.MDTOutComeOther
	  ,d.TransplantDiscussion
	  ,d.TransplantDiscussionDetail
	  ,d.MDTDate
	 , stuff((
        select ',' + oc.OtherComorbidity
        from Clinical.OtherComorbiditiesBMT oc
        where oc.VisitId = d.VisitId
        order by oc.OtherComorbidityIdBMT
        for xml path('')
    ),1,1,'') as OtherComorbidityIdBMTVB
	,d.TransplantDiscussionDetail
	, stuff((
        select ',' + sl.SiteOfLymphnodes
        from Clinical.SiteOfLymphnodesBMT sl
        where sl.VisitId = d.VisitId
        order by sl.LymphnodesId
        for xml path('')
    ),1,1,'') as SiteOfLymphnodesVB
	, stuff((
        select ',' + sl.PreviousTreatment
        from Clinical.PreviousTreatmentBMT sl
        where sl.VisitId = d.VisitId
        order by sl.PreviousTreatmentIdBMT
        for xml path('')
    ),1,1,'') as PreviousTreatmentTypeBMTVB
	, stuff((
        select ',' + oc.OutcomeText
        from Clinical.MDTOutcomeBMT oc
        where oc.VisitId = d.VisitId
        order by oc.MDTOutcomeIdBMT
        for xml path('')
    ),1,1,'') as MDTOutComeBMTVB
	,d.IsLocked
	  From Clinical.PatientDetailBMT d
	  Where d.VisitId=@visitId AND d.PatientId=@patId;
END
GO

-- 4. Lock the record the moment it's first saved, and keep it locked on every
--    subsequent (authorized) update.
CREATE OR ALTER PROCEDURE [Clinical].[InsertUpdatePatientDetail]
	@PatientDetailId int,
	@PatientId int,
	@VisitId int,
	@VisitDateTime datetime,
	@VisitType varchar (50),
	@CIC nvarchar (1500) NULL,
	@Diagnosis varchar(150) = NULL,
	@DiagnosisDetails varchar(500) = NULL,
	@ChildStatus varchar(50) NULL,
	@MeldStatus varchar(50) NULL,
	@BCLC varchar(50) NULL,
	@ECOGStatus varchar(50) NULL,
	@HepStatus varchar(50) NULL,
	@AscitiesStatus varchar(50) NULL,
	@OtherComorbities nvarchar(2000) = NULL,
	@CTReportConclusion nvarchar(4000) NULL,
	@Angioinvasion bit NULL,
	@SiteOfAngioinvasionVB nvarchar(2000) = NULL,
	@ExtraHepaticDisease bit NULL,
	@LiverCirrhosis bit NULL,
	@PortalHypertension bit NULL,
	@Varices bit NULL,
	@NumberOfLesion varchar(50) NULL,
	@LargestLesionSize varchar(50) NULL,
	@SVRStatus varchar(50) NULL,
	@PreviousTreatment bit NULL,
	@TreatmentResponse nvarchar(2000) NULL,
	@MeetingMilan bit NULL,
	@UCSF bit NULL,
	@ASAN bit NULL,
	@other bit NULL,
	@otherDetail nvarchar(2000)= NULL,
	@PtWillingForTransplant bit NULL,
	@ReasonOfRefusal nvarchar(2000)= NULL,
	@MDTDiscussion nvarchar(4000)= NULL,
	@MDTOutCome nvarchar(2000)= NULL,
	@EnteredBy int NULL,
	@EnteredOn datetime NULL,
	@ClinicId int NULL,
	@MDTOutComeOther nvarchar(4000) NULL,
	@TransplantDiscussion bit NULL,
	@TransplantDiscussionDetail nvarchar(4000) NULL,
	@ImagingDate date NULL
As
Begin
if @PatientDetailId<=0
Begin
	Insert into Clinical.PatientDetail(
		PatientId, VisitId, VisitDateTime, VisitType, CIC, Diagnosis, DiagnosisDetails,
		ChildStatus, MeldStatus, BCLC, ECOGStatus, HepStatus, AscitiesStatus, OtherComorbities,
		CTReportConclusion, Angioinvasion, SiteOfAngioinvasionVB, ExtraHepaticDisease,
		LiverCirrhosis, PortalHypertension, Varices, NumberOfLesion, LargestLesionSize, SVRStatus,
		PreviousTreatment, TreatmentResponse, MeetingMilan, UCSF, ASAN, other, otherDetail,
		PtWillingForTransplant, ReasonOfRefusal, MDTDiscussion, MDTOutCome, EnteredBy, EnteredOn,
		ClinicId, MDTOutComeOther, TransplantDiscussion, TransplantDiscussionDetail, ImagingDate,
		IsLocked
	)
	Values(
		@PatientId, @VisitId, @VisitDateTime, @VisitType, @CIC, @Diagnosis, @DiagnosisDetails,
		@ChildStatus, @MeldStatus, @BCLC, @ECOGStatus, @HepStatus, @AscitiesStatus, @OtherComorbities,
		@CTREPORTCONCLUSION, @Angioinvasion, @SiteOfAngioinvasionVB, @ExtraHepaticDisease,
		@LiverCirrhosis, @PortalHypertension, @Varices, @NumberOfLesion, @LargestLesionSize, @SVRStatus,
		@PreviousTreatment, @TreatmentResponse, @MeetingMilan, @UCSF, @ASAN, @other, @otherDetail,
		@PtWillingForTransplant, @ReasonOfRefusal, @MDTDiscussion, @MDTOutCome, @EnteredBy, SYSDATETIME(),
		@ClinicId, @MDTOutComeOther, @TransplantDiscussion, @TransplantDiscussionDetail, @ImagingDate,
		1
	);
	Select SCOPE_IDENTITY();
End
Else
Begin
	Update Top(1) Clinical.PatientDetail
	Set
		VisitDateTime=@VisitDateTime
		,VisitType=@VisitType
		,CIC=@CIC
		,Diagnosis = @Diagnosis
		,DiagnosisDetails  = @DiagnosisDetails
		,ChildStatus=@ChildStatus
		,MeldStatus=@MeldStatus
		,BCLC=@BCLC
		,ECOGStatus=@ECOGStatus
		,HepStatus=@HepStatus
		,AscitiesStatus=@AscitiesStatus
		,OtherComorbities=@OtherComorbities
		,CTReportConclusion=@CTReportConclusion
		,Angioinvasion=@Angioinvasion
		,SiteOfAngioinvasionVB=@SiteOfAngioinvasionVB
		,ExtraHepaticDisease=@ExtraHepaticDisease
		,LiverCirrhosis=@LiverCirrhosis
		,PortalHypertension=@PortalHypertension
		,Varices=@Varices
		,NumberOfLesion=@NumberOfLesion
		,LargestLesionSize=@LargestLesionSize
		,SVRStatus=@SVRStatus
		,PreviousTreatment=@PreviousTreatment
		,TreatmentResponse=@TreatmentResponse
		,MeetingMilan=@MeetingMilan
		,UCSF=@UCSF
		,ASAN=@ASAN
		,other=@other
		,otherDetail=@otherDetail
		,PtWillingForTransplant=@PtWillingForTransplant
		,ReasonOfRefusal=@ReasonOfRefusal
		,MDTDiscussion=@MDTDiscussion
		,MDTOutCome=@MDTOutCome
		,ClinicId=@ClinicId
		,MDTOutComeOther=@MDTOutComeOther
		,TransplantDiscussion=@TransplantDiscussion
		,TransplantDiscussionDetail=@TransplantDiscussionDetail
		,ImagingDate=@ImagingDate
		,IsLocked = 1
		where PatientDetailId=@PatientDetailId;
		Select @PatientDetailId;
	End
End
GO

CREATE OR ALTER PROCEDURE [Clinical].[InsertUpdatePatientDetailBMT]
	@PatientDetailId int,
	@PatientId int,
	@VisitId int,
	@VisitDateTime datetime,
	@VisitType varchar (50),
	@CIC nvarchar (1500) NULL,
	@DiseaseStatus varchar(50) NULL,
	@Lansky varchar(50) NULL,
	@HCTCI varchar(50) NULL,
	@HepBStatus varchar(50) NULL,
	@HepCStatus varchar(50) NULL,
	@HIVStatus varchar(50) NULL,
	@ECOGStatus varchar(50) NULL,
	@OtherComorbities nvarchar(2000) = NULL,
	@Summary nvarchar(4000) NULL,
	@Stage varchar(50) NULL,
	@RiskScore VARCHAR(50) NULL,
	@Biopsy TEXT NULL,
	@PETCT TEXT NULL,
	@Chelation varchar(50) NULL,
	@Liver varchar(50) NULL,
	@Spleen VARCHAR(50) NULL,
	@Lymphonodes bit NULL,
	@SiteOfLymphnodes nvarchar(2000) = NULL,
	@PreviousTreatment bit NULL,
	@TreatmentResponse nvarchar(2000) = NULL,
	@BMTTreatmentType VARCHAR(150) NULL,
	@PtWillingForTransplant bit NULL,
	@ReasonOfRefusal nvarchar(2000) = NULL,
	@MDTDiscussion nvarchar(4000) = NULL,
	@MDTOutCome nvarchar(4000) = NULL,
	@MDTOutComeOther nvarchar(2000) = NULL,
	@TransplantDiscussion bit NULL,
	@TransplantDiscussionDetail nvarchar(2000) NULL,
	@MDTDate date NULL,
	@EnteredBy int NULL,
	@EnteredOn datetime NULL,
	@ClinicId int NULL
As
Begin
    if @PatientDetailId <= 0
    Begin
        Insert into Clinical.PatientDetailBMT(
            PatientId, VisitId, VisitDateTime, VisitType, CIC, DiseaseStatus, Lansky, HCTCI,
            HepBStatus, HepCStatus, HIVStatus, ECOGStatus, OtherComorbities, Summary, Stage,
            RiskScore, Biopsy, PETCT, Chelation, Liver, Spleen, Lymphonodes, SiteOfLymphnodes,
            PreviousTreatment, TreatmentResponse, BMTTreatmentType, PtWillingForTransplant,
            ReasonOfRefusal, MDTDiscussion, MDTOutCome, MDTOutComeOther, TransplantDiscussion,
            TransplantDiscussionDetail, MDTDate, EnteredBy, EnteredOn, ClinicId, IsLocked
        )
        Values(
            @PatientId, @VisitId, @VisitDateTime, @VisitType, @CIC, @DiseaseStatus, @Lansky, @HCTCI,
            @HepBStatus, @HepCStatus, @HIVStatus, @ECOGStatus, @OtherComorbities, @Summary, @Stage,
            @RiskScore, @Biopsy, @PETCT, @Chelation, @Liver, @Spleen, @Lymphonodes, @SiteOfLymphnodes,
            @PreviousTreatment, @TreatmentResponse, @BMTTreatmentType, @PtWillingForTransplant,
            @ReasonOfRefusal, @MDTDiscussion, @MDTOutCome, @MDTOutComeOther, @TransplantDiscussion,
            @TransplantDiscussionDetail, @MDTDate, @EnteredBy, SYSDATETIME(), @ClinicId, 1
        );
        Select SCOPE_IDENTITY();
    End
    Else
    Begin
        Update Top(1) Clinical.PatientDetailBMT
        Set
            VisitDateTime = @VisitDateTime,
            VisitType = @VisitType,
            CIC = @CIC,
            DiseaseStatus = @DiseaseStatus,
            Lansky = @Lansky,
            HCTCI = @HCTCI,
            ECOGStatus = @ECOGStatus,
            HepBStatus = @HepBStatus,
            HepCStatus = @HepCStatus,
            HIVStatus = @HIVStatus,
            OtherComorbities = @OtherComorbities,
            Summary = @Summary,
            Stage = @Stage,
            RiskScore = @RiskScore,
            Biopsy = @Biopsy,
            PETCT = @PETCT,
            Chelation = @Chelation,
            Liver = @Liver,
            Spleen = @Spleen,
            Lymphonodes = @Lymphonodes,
            SiteOfLymphnodes = @SiteOfLymphnodes,
            PreviousTreatment = @PreviousTreatment,
            TreatmentResponse = @TreatmentResponse,
            BMTTreatmentType = @BMTTreatmentType,
            PtWillingForTransplant = @PtWillingForTransplant,
            ReasonOfRefusal = @ReasonOfRefusal,
            MDTDiscussion = @MDTDiscussion,
            MDTOutCome = @MDTOutCome,
            MDTOutComeOther = @MDTOutComeOther,
            TransplantDiscussion = @TransplantDiscussion,
            TransplantDiscussionDetail = @TransplantDiscussionDetail,
            MDTDate = @MDTDate,
            EnteredBy = @EnteredBy,
            EnteredOn = SYSDATETIME(),
            ClinicId = @ClinicId,
            IsLocked = 1
        where PatientDetailId = @PatientDetailId;
        Select @PatientDetailId;
    End
End
GO
