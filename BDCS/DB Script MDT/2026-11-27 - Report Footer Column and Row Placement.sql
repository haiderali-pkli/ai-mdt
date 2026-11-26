/*
	Follow-up to "2026-11-26 - Report Footer Consultants.sql". That migration grouped
	active consultants into the 4 fixed footer boxes automatically - by first-seen
	Department order, with a 5th+ distinct department silently appended into the 4th
	box. The hospital wants direct control instead: pick exactly which of the 4 footer
	columns a consultant prints in, so adding a new doctor or retiring an old one can
	be placed/swapped deliberately without depending on how departments happen to sort.

	This adds:
	1. System.ReportConsultant.ColumnNumber (1-4) - which footer box this entry prints
	   in. The existing DisplayOrder column now serves as this entry's row position
	   within that column (ascending, nulls last) - renamed to "Row" on the admin
	   screen, no schema change needed for that part.
	2. Backfill: the 16 consultants seeded by the previous migration get ColumnNumber
	   set to match the column they were already printing in (1=Liver Transplant
	   Surgeons, 2=Interventional Radiologists, 3=Hepatologists & Gastroenterologists,
	   4=Pediatric Gastroenterologists), so nothing moves on deploy. Only runs once,
	   guarded by the column not existing yet - any consultant added/edited after this
	   migration keeps whatever ColumnNumber was explicitly chosen for it.
	3. CRUD procs updated to read/write ColumnNumber.

	See PatientController.BuildConsultantFooterParameters, which now groups strictly by
	this explicit ColumnNumber instead of the previous first-seen-department heuristic.

	Safe to re-run.
*/

IF COL_LENGTH('System.ReportConsultant', 'ColumnNumber') IS NULL
BEGIN
	ALTER TABLE System.ReportConsultant ADD ColumnNumber tinyint NOT NULL CONSTRAINT DF_ReportConsultant_ColumnNumber DEFAULT (1);
END
GO

-- One-time backfill so the 16 pre-existing seeded rows keep printing in the same column
-- they already appeared in. Only touches rows still sitting at the just-added default (1)
-- whose Department text exactly matches one of the 3 non-column-1 original departments, so
-- re-running this script after ColumnNumber has been customized for real is a no-op for any
-- row that's already been moved off its default. Liver Transplant / Hepatobillary Surgeons
-- needs no UPDATE - column 1 is already the default.
UPDATE System.ReportConsultant SET ColumnNumber = 2 WHERE ColumnNumber = 1 AND Department = 'Interventional and Diagnostic Radioogists';
UPDATE System.ReportConsultant SET ColumnNumber = 3 WHERE ColumnNumber = 1 AND Department = 'Hepatologists & Gastroenterologists';
UPDATE System.ReportConsultant SET ColumnNumber = 4 WHERE ColumnNumber = 1 AND Department = 'Pediatric Gastroenterologists';
GO

IF OBJECT_ID('System.GetReportConsultants') IS NOT NULL DROP PROCEDURE System.GetReportConsultants;
GO
CREATE PROCEDURE System.GetReportConsultants
AS
BEGIN
	SELECT ReportConsultantId, Department, ConsultantName, Designation, Qualifications,
		ColumnNumber, DisplayOrder, IsActive, BoldDepartment, BoldConsultantName, BoldDesignation, BoldQualifications
	FROM System.ReportConsultant
	ORDER BY ColumnNumber, ISNULL(DisplayOrder, 999999), ConsultantName;
END
GO

IF OBJECT_ID('System.GetReportConsultantById') IS NOT NULL DROP PROCEDURE System.GetReportConsultantById;
GO
CREATE PROCEDURE System.GetReportConsultantById
	@ReportConsultantId int
AS
BEGIN
	SELECT ReportConsultantId, Department, ConsultantName, Designation, Qualifications,
		ColumnNumber, DisplayOrder, IsActive, BoldDepartment, BoldConsultantName, BoldDesignation, BoldQualifications
	FROM System.ReportConsultant
	WHERE ReportConsultantId = @ReportConsultantId;
END
GO

IF OBJECT_ID('System.InsertUpdateReportConsultant') IS NOT NULL DROP PROCEDURE System.InsertUpdateReportConsultant;
GO
CREATE PROCEDURE System.InsertUpdateReportConsultant
	@ReportConsultantId int,
	@Department varchar(150),
	@ConsultantName varchar(150),
	@Designation varchar(150) = NULL,
	@Qualifications varchar(200) = NULL,
	@ColumnNumber tinyint = 1,
	@DisplayOrder int = NULL,
	@IsActive bit = 1,
	@BoldDepartment bit = 1,
	@BoldConsultantName bit = 0,
	@BoldDesignation bit = 0,
	@BoldQualifications bit = 0
AS
BEGIN
	IF @ReportConsultantId > 0
	BEGIN
		UPDATE System.ReportConsultant
		SET Department = @Department, ConsultantName = @ConsultantName, Designation = @Designation,
			Qualifications = @Qualifications, ColumnNumber = @ColumnNumber, DisplayOrder = @DisplayOrder, IsActive = @IsActive,
			BoldDepartment = @BoldDepartment, BoldConsultantName = @BoldConsultantName,
			BoldDesignation = @BoldDesignation, BoldQualifications = @BoldQualifications
		WHERE ReportConsultantId = @ReportConsultantId;
		SELECT @ReportConsultantId;
	END
	ELSE
	BEGIN
		INSERT INTO System.ReportConsultant
			(Department, ConsultantName, Designation, Qualifications, ColumnNumber, DisplayOrder, IsActive,
			 BoldDepartment, BoldConsultantName, BoldDesignation, BoldQualifications)
		VALUES
			(@Department, @ConsultantName, @Designation, @Qualifications, @ColumnNumber,
			 ISNULL(@DisplayOrder, (SELECT ISNULL(MAX(DisplayOrder), 0) + 1 FROM System.ReportConsultant WHERE ColumnNumber = @ColumnNumber)),
			 @IsActive, @BoldDepartment, @BoldConsultantName, @BoldDesignation, @BoldQualifications);
		SELECT SCOPE_IDENTITY();
	END
END
GO
