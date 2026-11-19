/*
	Advance Search enhancements:
	1. MDT Outcome, Any Other Comorbidities, Site Of Angioinvasion, and Site Of
	   Extra Hepatic Disease History become multi-select filters (matching the
	   same ListBoxFor + Chosen.js pattern already used in the VitalHistory entry
	   forms for these exact fields).
	2. A From/To visit-date range filter is added.
	3. The result set gains the columns requested for the results grid/export:
	   MRN, Patient Name, Visit Date, Child Status, Site Of Hepatic Disease,
	   Number Of Lesion, Largest Lesion Size, Treatment Type, MDT Discussion,
	   MDT Outcome - plus current lab results, pivoted into columns in C#
	   (System.GetCurrentLabsForVisits below supplies the raw rows for that).

	Why a real string-splitter instead of the existing XML-based System.SplitIds:
	these four fields store free text, not just digits, and several of their real
	lookup values contain commas (MDTOutCome) or the delimiter-breaking XML
	characters &/</> (several lookup types) - an XML split would silently corrupt
	those values. System.SplitValues below uses plain string functions instead,
	and the app now joins multi-select values with '|' (verified against zero
	rows containing '|' in these lookup categories) rather than ',' for exactly
	this reason.

	Safe to re-run.
*/

-- 1. Safe text splitter (handles any character, not just digits/XML-safe text).
IF OBJECT_ID('System.SplitValues') IS NOT NULL
	DROP FUNCTION System.SplitValues;
GO
CREATE FUNCTION System.SplitValues (@Values varchar(max), @Delimiter varchar(5))
RETURNS @Result TABLE (Value varchar(500))
AS
BEGIN
	DECLARE @Pos int = 1, @NextPos int, @DelimLen int = LEN(@Delimiter), @Piece varchar(500);
	SET @Values = ISNULL(@Values, '');

	WHILE @Pos <= LEN(@Values) + 1
	BEGIN
		SET @NextPos = CHARINDEX(@Delimiter, @Values, @Pos);
		IF @NextPos = 0 SET @NextPos = LEN(@Values) + 1;

		SET @Piece = LTRIM(RTRIM(SUBSTRING(@Values, @Pos, @NextPos - @Pos)));
		IF @Piece <> ''
			INSERT INTO @Result (Value) VALUES (@Piece);

		SET @Pos = @NextPos + @DelimLen;
	END
	RETURN;
END
GO

-- 2. Raw current-lab rows for a set of visits, pivoted into columns in C# rather
--    than here (test names vary per visit, so the column set isn't fixed SQL-side).
IF OBJECT_ID('Clinical.GetCurrentLabsForVisits') IS NOT NULL
	DROP PROCEDURE Clinical.GetCurrentLabsForVisits;
GO
CREATE PROCEDURE Clinical.GetCurrentLabsForVisits
	@VisitIds varchar(max)
AS
BEGIN
	SELECT cr.VisitId, cr.TestName, cr.TestResult
	FROM Clinical.CurrentReports cr
	WHERE cr.VisitId IN (SELECT Id FROM System.SplitIds(@VisitIds));
END
GO

-- 3. The search itself: EXISTS-based semi-joins for filtering (so a visit with
--    several matching comorbidities/outcomes/sites still returns exactly one
--    row), and correlated STUFF(...FOR XML PATH('')) subqueries to show every
--    matching child value in the result row instead of just the filter hit.
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
	INNER JOIN Clinical.PatientDetail pd ON p.PatientId = pd.PatientId
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
