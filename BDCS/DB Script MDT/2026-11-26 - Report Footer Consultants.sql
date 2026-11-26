/*
	The consultant details printed in the footer of both the Liver MDT report
	(Reports\Patient.rdlc) and the Bone Marrow MDT report (Reports\PatientMaster.rdlc)
	were hardcoded directly into the two RDLC templates - four static text boxes per
	report, each with a fixed department heading and a fixed list of names. Adding,
	removing, or renaming a consultant meant editing both RDLC files by hand and
	redeploying. The two reports' footers were always identical anyway (same
	departments, same names, copy-pasted between the two RDLCs), so this adds ONE
	shared, configurable list that now feeds both reports' footers.

	This adds:
	1. System.ReportConsultant - one row per consultant entry (Department is free
	   text; consultants sharing the exact same Department string are grouped under
	   one heading on the printed footer). DisplayOrder controls both the order of
	   departments (first DisplayOrder seen per department) and the order of
	   consultants within a department. IsActive is a soft on/off switch (kept,
	   just excluded from print) distinct from a hard DELETE (fully removes the row).
	   Bold is configurable per individual field, per row (BoldDepartment,
	   BoldConsultantName, BoldDesignation, BoldQualifications) - see
	   PatientController.BuildConsultantFooterColumns, which turns the active rows
	   into the small HTML (<b> tags only) each report's footer placeholder renders.
	2. CRUD procs for the new admin screen (ReportConsultantController /
	   Views\ReportConsultant).
	3. Seed data: the exact consultants/departments that were previously hardcoded,
	   in the same left-to-right column order, so the printed report looks identical
	   immediately after this migration runs - only the *source* of that content
	   changes, not its initial appearance. (Original hardcoded spellings
	   "Hepatobillary" / "Radioogists" are preserved as-is in the seed; fix them from
	   the new screen if desired.)
	4. A new "Report Consultants" Role Right, same mechanism as Users / Roles / MDT
	   Types / Locations / Lab Tests.

	Safe to re-run.
*/

IF OBJECT_ID('System.ReportConsultant') IS NULL
BEGIN
	CREATE TABLE System.ReportConsultant (
		ReportConsultantId int IDENTITY(1,1) NOT NULL,
		Department varchar(150) NOT NULL,
		ConsultantName varchar(150) NOT NULL,
		Designation varchar(150) NULL,
		Qualifications varchar(200) NULL,
		DisplayOrder int NULL,
		IsActive bit NOT NULL CONSTRAINT DF_ReportConsultant_IsActive DEFAULT (1),
		BoldDepartment bit NOT NULL CONSTRAINT DF_ReportConsultant_BoldDepartment DEFAULT (1),
		BoldConsultantName bit NOT NULL CONSTRAINT DF_ReportConsultant_BoldConsultantName DEFAULT (0),
		BoldDesignation bit NOT NULL CONSTRAINT DF_ReportConsultant_BoldDesignation DEFAULT (0),
		BoldQualifications bit NOT NULL CONSTRAINT DF_ReportConsultant_BoldQualifications DEFAULT (0),
		CONSTRAINT PK_ReportConsultant PRIMARY KEY CLUSTERED (ReportConsultantId ASC)
	);
END
GO

-- Seed with the previously-hardcoded footer content, left-to-right column order,
-- only if the table is empty (never overwrite real configuration on a re-run).
IF NOT EXISTS (SELECT 1 FROM System.ReportConsultant)
BEGIN
	INSERT INTO System.ReportConsultant (Department, ConsultantName, DisplayOrder, IsActive, BoldDepartment)
	VALUES
		('Liver Transplant / Hepatobillary Surgeons', 'Prof. Dr. Faisal Saud Dar', 1, 1, 1),
		('Liver Transplant / Hepatobillary Surgeons', 'Dr. Ihsan Ul Haq', 2, 1, 1),
		('Liver Transplant / Hepatobillary Surgeons', 'Dr. Sohail Rashid', 3, 1, 1),
		('Liver Transplant / Hepatobillary Surgeons', 'Dr. M. Yasir Khan', 4, 1, 1),

		('Interventional and Diagnostic Radioogists', 'Dr. Ahmad Zia ud Din', 5, 1, 1),
		('Interventional and Diagnostic Radioogists', 'Dr. Abdullah Saeed', 6, 1, 1),
		('Interventional and Diagnostic Radioogists', 'Dr. Sana Kundi', 7, 1, 1),
		('Interventional and Diagnostic Radioogists', 'Dr. Tahir Malik', 8, 1, 1),
		('Interventional and Diagnostic Radioogists', 'Dr. M. Salman Rafique', 9, 1, 1),

		('Hepatologists & Gastroenterologists', 'Dr. Usman Aujla', 10, 1, 1),
		('Hepatologists & Gastroenterologists', 'Dr. Nouman Zakir', 11, 1, 1),
		('Hepatologists & Gastroenterologists', 'Dr. Khurram Anis', 12, 1, 1),
		('Hepatologists & Gastroenterologists', 'Dr. Ammara Naveed', 13, 1, 1),
		('Hepatologists & Gastroenterologists', 'Dr. Ahmad Karim Malik', 14, 1, 1),
		('Hepatologists & Gastroenterologists', 'Dr. Osama Tariq Butt', 15, 1, 1),

		('Pediatric Gastroenterologists', 'Dr. Iqtidar Seerat', 16, 1, 1);
END
GO

IF OBJECT_ID('System.GetReportConsultants') IS NOT NULL DROP PROCEDURE System.GetReportConsultants;
GO
CREATE PROCEDURE System.GetReportConsultants
AS
BEGIN
	SELECT ReportConsultantId, Department, ConsultantName, Designation, Qualifications,
		DisplayOrder, IsActive, BoldDepartment, BoldConsultantName, BoldDesignation, BoldQualifications
	FROM System.ReportConsultant
	ORDER BY ISNULL(DisplayOrder, 999999), ConsultantName;
END
GO

IF OBJECT_ID('System.GetReportConsultantById') IS NOT NULL DROP PROCEDURE System.GetReportConsultantById;
GO
CREATE PROCEDURE System.GetReportConsultantById
	@ReportConsultantId int
AS
BEGIN
	SELECT ReportConsultantId, Department, ConsultantName, Designation, Qualifications,
		DisplayOrder, IsActive, BoldDepartment, BoldConsultantName, BoldDesignation, BoldQualifications
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
			Qualifications = @Qualifications, DisplayOrder = @DisplayOrder, IsActive = @IsActive,
			BoldDepartment = @BoldDepartment, BoldConsultantName = @BoldConsultantName,
			BoldDesignation = @BoldDesignation, BoldQualifications = @BoldQualifications
		WHERE ReportConsultantId = @ReportConsultantId;
		SELECT @ReportConsultantId;
	END
	ELSE
	BEGIN
		INSERT INTO System.ReportConsultant
			(Department, ConsultantName, Designation, Qualifications, DisplayOrder, IsActive,
			 BoldDepartment, BoldConsultantName, BoldDesignation, BoldQualifications)
		VALUES
			(@Department, @ConsultantName, @Designation, @Qualifications,
			 ISNULL(@DisplayOrder, (SELECT ISNULL(MAX(DisplayOrder), 0) + 1 FROM System.ReportConsultant)),
			 @IsActive, @BoldDepartment, @BoldConsultantName, @BoldDesignation, @BoldQualifications);
		SELECT SCOPE_IDENTITY();
	END
END
GO

IF OBJECT_ID('System.ToggleReportConsultantActive') IS NOT NULL DROP PROCEDURE System.ToggleReportConsultantActive;
GO
CREATE PROCEDURE System.ToggleReportConsultantActive
	@ReportConsultantId int
AS
BEGIN
	UPDATE System.ReportConsultant
	SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
	WHERE ReportConsultantId = @ReportConsultantId;

	SELECT @@ROWCOUNT;
END
GO

IF OBJECT_ID('System.DeleteReportConsultant') IS NOT NULL DROP PROCEDURE System.DeleteReportConsultant;
GO
CREATE PROCEDURE System.DeleteReportConsultant
	@ReportConsultantId int
AS
BEGIN
	DELETE FROM System.ReportConsultant WHERE ReportConsultantId = @ReportConsultantId;
	SELECT @@ROWCOUNT;
END
GO

-- ===== Role Rights =====

INSERT INTO System.Menu (MenuName, MenuGroupName, Url, MenuParentId, Active)
SELECT v.MenuName, 'Admin', NULL, NULL, 1
FROM (VALUES ('Report Consultants')) AS v(MenuName)
WHERE NOT EXISTS (SELECT 1 FROM System.Menu m WHERE m.MenuName = v.MenuName);
GO

-- Granted to Admin by default; any other role can be granted this from the Role Rights screen.
INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete)
SELECT 1, m.MenuId, 1, 1, 1
FROM System.Menu m
WHERE m.MenuName IN ('Report Consultants')
AND NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 1 AND MenuId = m.MenuId);
GO
