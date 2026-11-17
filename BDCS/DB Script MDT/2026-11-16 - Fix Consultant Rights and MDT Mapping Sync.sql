/*
	Fixes two role-rights bugs reported against the Consultant role / "test" user:

	1. System.RoleRight had no rows at all for the Consultant role (RoleId 2) on the
	   "Create Visit" (MenuId 9) and "Visit History" (MenuId 10) menus, so
	   [CheckUserRights] denied those actions outright for every Consultant.

	2. The Add/Edit User screen's "MDT Types" checkboxes only ever wrote to
	   System.UserClinic (added in the 2026-11-06 Multi-Role migration). But
	   BDCS/Views/Patient/GetPatientList.cshtml decides whether to render the Visit
	   History links and the Create Visit dropdown using the much older
	   System.MDTMapping/System.Lookup("MDTType") tables (CommonDAL.GetMDTTypesNameByUserId).
	   The two tables were never kept in sync, so a user could be correctly assigned
	   an MDT Type via the new screen and still see no Visit History / Create Visit
	   UI at all. Confirmed affecting existing users, not just the test account:
	   ahmad.zia (Consultant), akhan (Consultant), dr.sami (Front Desk).

	Fix: make Clinical.InsertUpdateUserClinics keep System.MDTMapping in sync with
	System.UserClinic every time MDT Types are saved, and backfill existing users.
	Safe to re-run.
*/

-- 1. Grant Consultant (RoleId=2) the two clinical rights it was missing.
IF NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 2 AND MenuId = 9)
	INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete) VALUES (2, 9, 1, 1, 0);

IF NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 2 AND MenuId = 10)
	INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete) VALUES (2, 10, 1, 0, 0);
GO

-- 2. Keep System.MDTMapping in sync whenever MDT Types (Clinics) are saved for a user.
CREATE OR ALTER PROCEDURE Clinical.InsertUpdateUserClinics
	@UserId int,
	@ClinicIds varchar(200)
AS
BEGIN
	DELETE FROM System.UserClinic WHERE UserId = @UserId;

	INSERT INTO System.UserClinic (ClinicId, UserId, AssignmentDate)
	SELECT Id, @UserId, SYSDATETIME() FROM System.SplitIds(@ClinicIds);

	-- Legacy mirror: GetMDTTypesNameByUserId (System.MDTMapping/System.Lookup) still
	-- gates the Visit History / Create Visit UI on the Patient Search screen, so every
	-- clinic assignment made here needs a matching MDTMapping row.
	DELETE FROM System.MDTMapping WHERE UserId = @UserId;

	INSERT INTO System.MDTMapping (UserId, lookupId)
	SELECT DISTINCT @UserId, l.LookupId
	FROM System.UserClinic uc
	INNER JOIN System.Clinic c ON c.ClinicId = uc.ClinicId
	INNER JOIN System.Lookup l ON l.LookupType = 'MDTType' AND l.IsActive = 1
		AND (
			(c.ClinicName LIKE '%Liver%' AND l.DisplayValue = 'Liver')
			OR (c.ClinicName LIKE '%BMT%' AND l.DisplayValue = 'Bone Marrow')
		)
	WHERE uc.UserId = @UserId;
END
GO

-- 3. Backfill: sync MDTMapping for every user who already has UserClinic rows but
--    was saved before this fix existed (re-running the proc per user is simplest).
DECLARE @UserId int;
DECLARE UserCursor CURSOR LOCAL FAST_FORWARD FOR
	SELECT DISTINCT UserId FROM System.UserClinic;
OPEN UserCursor;
FETCH NEXT FROM UserCursor INTO @UserId;
WHILE @@FETCH_STATUS = 0
BEGIN
	DECLARE @ClinicIds varchar(200) = STUFF((
		SELECT ',' + CAST(ClinicId AS varchar(10))
		FROM System.UserClinic WHERE UserId = @UserId
		FOR XML PATH('')
	), 1, 1, '');
	EXEC Clinical.InsertUpdateUserClinics @UserId = @UserId, @ClinicIds = @ClinicIds;
	FETCH NEXT FROM UserCursor INTO @UserId;
END
CLOSE UserCursor;
DEALLOCATE UserCursor;
GO
