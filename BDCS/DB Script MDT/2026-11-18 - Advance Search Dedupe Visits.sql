/*
	Clinical.PatientDetail has pre-existing duplicate rows for the same VisitId in
	production data (confirmed: VisitId 10951 has 24 rows, 21314 has 11, 21343 has
	11, etc. - looks like rapid repeat-saves from some earlier bug, all identical
	except PatientDetailId/EnteredOn). GetVisitListAgainstSearch joined straight
	to Clinical.PatientDetail, so any visit that happens to be one of these
	duplicated ones was returned once per duplicate row - e.g. searching
	MdtOutcome='Transplant' + OtherComorbidities='Diabetes|HTN' returned 53 rows
	for only 33 distinct visits.

	Fix: join to a de-duplicated version of PatientDetail that keeps exactly one
	row per VisitId (the highest PatientDetailId - i.e. the most recently saved
	copy), so the search always returns exactly one row per visit regardless of
	how many duplicate detail rows exist underneath.

	Safe to re-run.
*/

CREATE OR ALTER PROC [Clinical].[GetVisitListAgainstSearch]
	@ChildStatus int = NULL,
	@MeldStatus int = NULL,
	@BCLCStatus int = NULL,
	@ECOG int = NULL,
	@TreatmentResponse int = NULL,
	@MdtOutcome varchar(4000) = NULL,
	@HepatitisStatus int = NULL,
	@AscitiesStatus int = NULL,
	@Svr int = NULL,
	@OtherComorbidities varchar(4000) = NULL,
	@AngioInvasion varchar(4000) = NULL,
	@SiteOfHepaticDisease varchar(4000) = NULL,
	@LiverCirrhosis bit = NULL,
	@PortalHypertension bit = NULL,
	@Varices bit = NULL,
	@MeetingMilan bit = NULL,
	@UCSF bit = NULL,
	@ASAN bit = NULL,
	@AFP float = NULL,
	@FromDate date = NULL,
	@ToDate date = NULL
AS
BEGIN
	-- safety cap: an unfiltered (or lightly filtered) search could otherwise match
	-- thousands of visits, each needing three correlated subqueries plus a full lab
	-- pivot in C# - same idea as the Patient List's "All" cap. Narrow the search
	-- further if more than this many results are genuinely needed.
	;WITH DedupedPatientDetail AS (
		SELECT *, ROW_NUMBER() OVER (PARTITION BY VisitId ORDER BY PatientDetailId DESC) AS rn
		FROM Clinical.PatientDetail
	)
	SELECT TOP (2000)
		pv.MDTType,
		p.PatientId,
		pd.VisitId,
		p.MrNumber AS MRN,
		CONCAT(p.FirstName, ' ', p.LastName) AS PatientName,
		pd.VisitDateTime AS VisitDate,
		dbo.GetLookUpValueAgainstId(pd.ChildStatus) AS ChildStatus,
		STUFF((
			SELECT ',' + sd.SiteOfHepaticDiseaseText
			FROM Clinical.SiteOfHepaticDisease sd
			WHERE sd.VisitId = pd.VisitId
			FOR XML PATH('')
		), 1, 1, '') AS SiteOfHepaticDiseaseString,
		dbo.GetLookUpValueAgainstId(pd.NumberOfLesion) AS NumberOfLesion,
		pd.LargestLesionSize,
		STUFF((
			SELECT ',' + tt.TreatmentTypeText
			FROM Clinical.TreatmentType tt
			WHERE tt.VisitId = pd.VisitId
			FOR XML PATH('')
		), 1, 1, '') AS TreatmentTypeString,
		pd.MDTDiscussion,
		STUFF((
			SELECT ',' + mo.OutcomeText
			FROM Clinical.MDTOutcome mo
			WHERE mo.VisitId = pd.VisitId
			FOR XML PATH('')
		), 1, 1, '') AS MDTOutComeString
	FROM Clinical.Patient p
	INNER JOIN DedupedPatientDetail pd ON p.PatientId = pd.PatientId AND pd.rn = 1
	LEFT JOIN Clinical.PatientVisit pv ON pv.VisitId = pd.VisitId
	WHERE (ISNULL(@ChildStatus,0) = 0 OR pd.ChildStatus = @ChildStatus)
	AND (ISNULL(@MeldStatus,0) = 0 OR pd.MeldStatus = @MeldStatus)
	AND (ISNULL(@BCLCStatus,0) = 0 OR pd.BCLC = @BCLCStatus)
	AND (ISNULL(@ECOG,0) = 0 OR pd.ECOGStatus = @ECOG)
	AND (ISNULL(@TreatmentResponse,0) = 0 OR pd.TreatmentResponse = @TreatmentResponse)
	AND (@MdtOutcome IS NULL OR EXISTS (
		SELECT 1 FROM Clinical.MDTOutcome mo2
		WHERE mo2.VisitId = pd.VisitId
		AND mo2.OutcomeText IN (SELECT Value FROM System.SplitValues(@MdtOutcome, '|'))
	))
	AND (ISNULL(@HepatitisStatus,0) = 0 OR pd.HepStatus = @HepatitisStatus)
	AND (ISNULL(@AscitiesStatus,0) = 0 OR pd.AscitiesStatus = @AscitiesStatus)
	AND (ISNULL(@Svr,0) = 0 OR pd.SVRStatus = @Svr)
	AND (@OtherComorbidities IS NULL OR EXISTS (
		SELECT 1 FROM Clinical.OtherComorbidities oc2
		WHERE oc2.VisitId = pd.VisitId
		AND oc2.OtherComorbidity IN (SELECT Value FROM System.SplitValues(@OtherComorbidities, '|'))
	))
	AND (@AngioInvasion IS NULL OR EXISTS (
		SELECT 1 FROM Clinical.SiteOfAngioinvasion sai2
		WHERE sai2.VisitId = pd.VisitId
		AND sai2.SiteOfAngioinvasion IN (SELECT Value FROM System.SplitValues(@AngioInvasion, '|'))
	))
	AND (@SiteOfHepaticDisease IS NULL OR EXISTS (
		SELECT 1 FROM Clinical.SiteOfHepaticDisease sd2
		WHERE sd2.VisitId = pd.VisitId
		AND sd2.SiteOfHepaticDiseaseText IN (SELECT Value FROM System.SplitValues(@SiteOfHepaticDisease, '|'))
	))
	AND (ISNULL(@AFP,0) = 0 OR EXISTS (
		SELECT 1 FROM Clinical.CurrentReports cr2
		WHERE cr2.VisitId = pd.VisitId AND cr2.TestName = 'AFP' AND TRY_CONVERT(float, cr2.TestResult) > @AFP
	))
	AND (ISNULL(@LiverCirrhosis,0) = 0 OR pd.LiverCirrhosis = @LiverCirrhosis)
	AND (ISNULL(@PortalHypertension,0) = 0 OR pd.PortalHypertension = @PortalHypertension)
	AND (ISNULL(@Varices,0) = 0 OR pd.Varices = @Varices)
	AND (ISNULL(@MeetingMilan,0) = 0 OR pd.MeetingMilan = @MeetingMilan)
	AND (ISNULL(@UCSF,0) = 0 OR pd.UCSF = @UCSF)
	AND (ISNULL(@ASAN,0) = 0 OR pd.ASAN = @ASAN)
	AND (@FromDate IS NULL OR pd.VisitDateTime >= @FromDate)
	AND (@ToDate IS NULL OR pd.VisitDateTime < DATEADD(day, 1, @ToDate))
	ORDER BY pd.VisitDateTime DESC;
END
GO
