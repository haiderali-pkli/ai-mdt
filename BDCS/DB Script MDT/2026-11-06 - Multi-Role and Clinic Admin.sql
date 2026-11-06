/*
	Step 1 - User Management: Multi-Role + Multi-Location (Clinic) support
	Additive only: no existing table, column, proc or view is altered or dropped.
	Safe to re-run (every block is guarded).
*/

-- 1. Helper: split a CSV of ids (compat level 120 -> no STRING_SPLIT, use XML split)
IF OBJECT_ID('System.SplitIds') IS NOT NULL
	DROP FUNCTION System.SplitIds;
GO
SET QUOTED_IDENTIFIER ON;
GO
CREATE FUNCTION System.SplitIds (@CsvIds varchar(max))
RETURNS @Result TABLE (Id int)
AS
BEGIN
	DECLARE @xml xml = CAST('<i>' + REPLACE(ISNULL(@CsvIds,''), ',', '</i><i>') + '</i>' AS xml);

	INSERT INTO @Result (Id)
	SELECT CAST(LTRIM(RTRIM(T.c.value('.', 'varchar(20)'))) AS int)
	FROM @xml.nodes('/i') T(c)
	WHERE LTRIM(RTRIM(T.c.value('.', 'varchar(20)'))) <> '';

	RETURN;
END
GO

-- 2. UserRole table (new) + backfill from existing Users.RoleId
IF OBJECT_ID('System.UserRole') IS NULL
BEGIN
	CREATE TABLE System.UserRole (
		UserRoleId int IDENTITY(1,1) NOT NULL,
		UserId int NOT NULL,
		RoleId int NOT NULL,
		AssignmentDate datetime NULL CONSTRAINT DF_UserRole_AssignmentDate DEFAULT (SYSDATETIME()),
		CONSTRAINT PK_UserRole PRIMARY KEY CLUSTERED (UserRoleId ASC)
	);

	ALTER TABLE System.UserRole WITH CHECK ADD CONSTRAINT FK_UserRole_Users FOREIGN KEY (UserId) REFERENCES System.Users (UserId);
	ALTER TABLE System.UserRole CHECK CONSTRAINT FK_UserRole_Users;

	ALTER TABLE System.UserRole WITH CHECK ADD CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES System.Role (RoleId);
	ALTER TABLE System.UserRole CHECK CONSTRAINT FK_UserRole_Role;
END
GO

INSERT INTO System.UserRole (UserId, RoleId, AssignmentDate)
SELECT u.UserId, u.RoleId, SYSDATETIME()
FROM System.Users u
WHERE u.RoleId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM System.UserRole ur WHERE ur.UserId = u.UserId AND ur.RoleId = u.RoleId);
GO

-- 3. Role-related procs
IF OBJECT_ID('System.GetUserRoles') IS NOT NULL
	DROP PROCEDURE System.GetUserRoles;
GO
CREATE PROCEDURE System.GetUserRoles
	@UserId int
AS
BEGIN
	SELECT r.RoleId, r.RoleName
	FROM System.UserRole ur
	INNER JOIN System.Role r ON r.RoleId = ur.RoleId
	WHERE ur.UserId = @UserId
	AND r.Active = 1;
END
GO

IF OBJECT_ID('System.GetRightsAgainstRoles') IS NOT NULL
	DROP PROCEDURE System.GetRightsAgainstRoles;
GO
CREATE PROCEDURE System.GetRightsAgainstRoles
	@RoleIds varchar(200)
AS
BEGIN
	SELECT m.MenuId, MAX(m.MenuName) MenuName, MAX(m.MenuGroupName) MenuGroupName,
		MAX(m.Url) Url, ISNULL(MAX(m.MenuParentId),0) MenuParentId,
		MAX(CAST(rr.CanView AS int)) CanView, MAX(CAST(rr.CanAddEdit AS int)) CanAddEdit,
		MAX(CAST(rr.CanDelete AS int)) CanDelete
	FROM System.SplitIds(@RoleIds) s
	INNER JOIN System.Role r ON r.RoleId = s.Id AND r.Active = 1
	INNER JOIN System.RoleRight rr ON rr.RoleId = r.RoleId
	INNER JOIN System.Menu m ON m.MenuId = rr.MenuId AND m.Active = 1
	GROUP BY m.MenuId;
END
GO

IF OBJECT_ID('System.InsertUpdateUserRoles') IS NOT NULL
	DROP PROCEDURE System.InsertUpdateUserRoles;
GO
CREATE PROCEDURE System.InsertUpdateUserRoles
	@UserId int,
	@RoleIds varchar(200)
AS
BEGIN
	DELETE FROM System.UserRole WHERE UserId = @UserId;

	INSERT INTO System.UserRole (UserId, RoleId, AssignmentDate)
	SELECT @UserId, Id, SYSDATETIME() FROM System.SplitIds(@RoleIds);

	UPDATE System.Users SET RoleId = (SELECT MIN(Id) FROM System.SplitIds(@RoleIds)) WHERE UserId = @UserId;
END
GO

-- 4. Clinic (Location) CRUD procs
IF OBJECT_ID('System.GetClinicList') IS NOT NULL
	DROP PROCEDURE System.GetClinicList;
GO
CREATE PROCEDURE System.GetClinicList
AS
BEGIN
	SELECT ClinicId, ClinicName, ContactPerson, ContactNumber, IsActive
	FROM System.Clinic
	ORDER BY ClinicName;
END
GO

IF OBJECT_ID('System.GetClinicById') IS NOT NULL
	DROP PROCEDURE System.GetClinicById;
GO
CREATE PROCEDURE System.GetClinicById
	@ClinicId int
AS
BEGIN
	SELECT ClinicId, ClinicName, ContactPerson, ContactNumber, IsActive
	FROM System.Clinic
	WHERE ClinicId = @ClinicId;
END
GO

IF OBJECT_ID('System.InsertUpdateClinic') IS NOT NULL
	DROP PROCEDURE System.InsertUpdateClinic;
GO
CREATE PROCEDURE System.InsertUpdateClinic
	@ClinicId int,
	@ClinicName varchar(500),
	@ContactPerson varchar(500),
	@ContactNumber varchar(50),
	@IsActive bit = null
AS
BEGIN
	IF @ClinicId <= 0
	BEGIN
		INSERT INTO System.Clinic (ClinicName, ContactPerson, ContactNumber, IsActive)
		VALUES (@ClinicName, @ContactPerson, @ContactNumber, @IsActive);
		SELECT SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE TOP(1) System.Clinic
		SET ClinicName = @ClinicName, ContactPerson = @ContactPerson,
			ContactNumber = @ContactNumber, IsActive = @IsActive
		WHERE ClinicId = @ClinicId;
		SELECT @ClinicId;
	END
END
GO

-- 5. UserClinic (Location assignment) proc
IF OBJECT_ID('Clinical.InsertUpdateUserClinics') IS NOT NULL
	DROP PROCEDURE Clinical.InsertUpdateUserClinics;
GO
CREATE PROCEDURE Clinical.InsertUpdateUserClinics
	@UserId int,
	@ClinicIds varchar(200)
AS
BEGIN
	DELETE FROM System.UserClinic WHERE UserId = @UserId;

	INSERT INTO System.UserClinic (ClinicId, UserId, AssignmentDate)
	SELECT Id, @UserId, SYSDATETIME() FROM System.SplitIds(@ClinicIds);
END
GO
