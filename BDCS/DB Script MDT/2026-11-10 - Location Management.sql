/*
	Step 2 - User Management: Location support (physical hospital site, e.g. PKLI Lahore, PKLI Rawalpindi)

	Context: System.Clinic / System.UserClinic (added in "2026-11-06 - Multi-Role and Clinic Admin.sql")
	already carries the "MDT Type" (MDT / MDT BMT) concept end-to-end - it drives Session["ClinicId"]
	used across patient/visit creation. That table is NOT touched here - its rows and behaviour are
	left exactly as they are, only its UI labels change (handled in the C# views, not in this script).

	This script adds a brand new, independent concept: Location (physical hospital site). A user can
	be assigned any number of MDT Types (existing Clinic mechanism) AND any number of Locations
	(this new mechanism) at the same time - the two lists are completely separate.

	Additive only: no existing table, column, proc or view is altered or dropped.
	Safe to re-run (every block is guarded).
*/

-- 1. Location table (new)
IF OBJECT_ID('System.Location') IS NULL
BEGIN
	CREATE TABLE System.Location (
		LocationId int IDENTITY(1,1) NOT NULL,
		LocationName varchar(500) NOT NULL,
		IsActive bit NULL,
		CONSTRAINT PK_Location PRIMARY KEY CLUSTERED (LocationId ASC)
	);
END
GO

-- 2. Seed starter locations (idempotent - safe to re-run, and safe if admins already renamed/added rows)
IF NOT EXISTS (SELECT 1 FROM System.Location WHERE LocationName = 'PKLI Lahore')
	INSERT INTO System.Location (LocationName, IsActive) VALUES ('PKLI Lahore', 1);
GO
IF NOT EXISTS (SELECT 1 FROM System.Location WHERE LocationName = 'PKLI Rawalpindi')
	INSERT INTO System.Location (LocationName, IsActive) VALUES ('PKLI Rawalpindi', 1);
GO

-- 3. UserLocation table (new) - many-to-many, mirrors System.UserClinic
IF OBJECT_ID('System.UserLocation') IS NULL
BEGIN
	CREATE TABLE System.UserLocation (
		UserLocationId int IDENTITY(1,1) NOT NULL,
		UserId int NOT NULL,
		LocationId int NOT NULL,
		AssignmentDate datetime NULL CONSTRAINT DF_UserLocation_AssignmentDate DEFAULT (SYSDATETIME()),
		CONSTRAINT PK_UserLocation PRIMARY KEY CLUSTERED (UserLocationId ASC)
	);

	ALTER TABLE System.UserLocation WITH CHECK ADD CONSTRAINT FK_UserLocation_Users FOREIGN KEY (UserId) REFERENCES System.Users (UserId);
	ALTER TABLE System.UserLocation CHECK CONSTRAINT FK_UserLocation_Users;

	ALTER TABLE System.UserLocation WITH CHECK ADD CONSTRAINT FK_UserLocation_Location FOREIGN KEY (LocationId) REFERENCES System.Location (LocationId);
	ALTER TABLE System.UserLocation CHECK CONSTRAINT FK_UserLocation_Location;
END
GO

-- 4. Location CRUD procs (mirror System.GetClinicList / GetClinicById / InsertUpdateClinic)
IF OBJECT_ID('System.GetLocationList') IS NOT NULL
	DROP PROCEDURE System.GetLocationList;
GO
CREATE PROCEDURE System.GetLocationList
AS
BEGIN
	SELECT LocationId, LocationName, IsActive
	FROM System.Location
	ORDER BY LocationName;
END
GO

IF OBJECT_ID('System.GetLocationById') IS NOT NULL
	DROP PROCEDURE System.GetLocationById;
GO
CREATE PROCEDURE System.GetLocationById
	@LocationId int
AS
BEGIN
	SELECT LocationId, LocationName, IsActive
	FROM System.Location
	WHERE LocationId = @LocationId;
END
GO

IF OBJECT_ID('System.InsertUpdateLocation') IS NOT NULL
	DROP PROCEDURE System.InsertUpdateLocation;
GO
CREATE PROCEDURE System.InsertUpdateLocation
	@LocationId int,
	@LocationName varchar(500),
	@IsActive bit = null
AS
BEGIN
	IF @LocationId <= 0
	BEGIN
		INSERT INTO System.Location (LocationName, IsActive)
		VALUES (@LocationName, @IsActive);
		SELECT SCOPE_IDENTITY();
	END
	ELSE
	BEGIN
		UPDATE TOP(1) System.Location
		SET LocationName = @LocationName, IsActive = @IsActive
		WHERE LocationId = @LocationId;
		SELECT @LocationId;
	END
END
GO

-- 5. UserLocation (Location assignment) procs - mirror Clinical.GetUserClinics / InsertUpdateUserClinics
-- Reuses System.SplitIds, already created by "2026-11-06 - Multi-Role and Clinic Admin.sql"
IF OBJECT_ID('Clinical.GetUserLocations') IS NOT NULL
	DROP PROCEDURE Clinical.GetUserLocations;
GO
CREATE PROCEDURE Clinical.GetUserLocations
	@UserId int
AS
BEGIN
	SELECT l.LocationId, l.LocationName
	FROM System.UserLocation ul
	INNER JOIN System.Location l ON ul.LocationId = l.LocationId
	WHERE ul.UserId = @UserId;
END
GO

IF OBJECT_ID('Clinical.InsertUpdateUserLocations') IS NOT NULL
	DROP PROCEDURE Clinical.InsertUpdateUserLocations;
GO
CREATE PROCEDURE Clinical.InsertUpdateUserLocations
	@UserId int,
	@LocationIds varchar(200)
AS
BEGIN
	DELETE FROM System.UserLocation WHERE UserId = @UserId;

	INSERT INTO System.UserLocation (LocationId, UserId, AssignmentDate)
	SELECT Id, @UserId, SYSDATETIME() FROM System.SplitIds(@LocationIds);
END
GO
