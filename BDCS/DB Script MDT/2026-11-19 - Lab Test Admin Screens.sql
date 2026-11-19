/*
	System.LabTests and System.LabTestsBMT (the lab test catalogs that drive the
	default lab-entry grid on a visit - see Clinical.GetLabOrders / GetLabOrdersBMT,
	which read these tables whenever a visit has no results yet) had no admin UI at
	all - adding, renaming, or reordering a test meant a direct database edit.

	This adds:
	1. CRUD procs for both catalogs.
	2. Two new Role Rights ("Lab Tests", "Lab Tests BMT") so access can be granted
	   per role from the existing Role Rights screen, same mechanism as Users /
	   Roles / MDT Types / Locations.

	Both catalogs are read by their GetLabOrders* proc filtered to a fixed
	TestGroup value (System.LabTests: NULL: System.LabTestsBMT: 'BMT') - the new
	Insert/Update procs always write that same fixed value rather than exposing
	TestGroup as an editable field, so a typo here can't silently make a test
	disappear from the lab-entry grid.

	Safe to re-run.
*/

-- ===== Liver (System.LabTests, TestGroup IS NULL) =====

IF OBJECT_ID('System.GetLabTests') IS NOT NULL DROP PROCEDURE System.GetLabTests;
GO
CREATE PROCEDURE System.GetLabTests
AS
BEGIN
	SELECT TestId, TestName, TestUnit, [Order]
	FROM System.LabTests
	WHERE TestGroup IS NULL
	ORDER BY ISNULL([Order], 999999), TestName;
END
GO

IF OBJECT_ID('System.GetLabTestById') IS NOT NULL DROP PROCEDURE System.GetLabTestById;
GO
CREATE PROCEDURE System.GetLabTestById
	@TestId int
AS
BEGIN
	SELECT TestId, TestName, TestUnit, [Order]
	FROM System.LabTests
	WHERE TestId = @TestId AND TestGroup IS NULL;
END
GO

IF OBJECT_ID('System.InsertUpdateLabTest') IS NOT NULL DROP PROCEDURE System.InsertUpdateLabTest;
GO
CREATE PROCEDURE System.InsertUpdateLabTest
	@TestId int,
	@TestName varchar(50),
	@TestUnit varchar(50),
	@Order int = NULL
AS
BEGIN
	IF @TestId > 0
	BEGIN
		UPDATE System.LabTests
		SET TestName = @TestName, TestUnit = @TestUnit, [Order] = @Order
		WHERE TestId = @TestId AND TestGroup IS NULL;
		SELECT @TestId;
	END
	ELSE
	BEGIN
		INSERT INTO System.LabTests (TestName, TestUnit, TestGroup, [Order])
		VALUES (@TestName, @TestUnit, NULL, ISNULL(@Order, (SELECT ISNULL(MAX([Order]), 0) + 1 FROM System.LabTests WHERE TestGroup IS NULL)));
		SELECT SCOPE_IDENTITY();
	END
END
GO

IF OBJECT_ID('System.DeleteLabTest') IS NOT NULL DROP PROCEDURE System.DeleteLabTest;
GO
CREATE PROCEDURE System.DeleteLabTest
	@TestId int
AS
BEGIN
	DELETE FROM System.LabTests WHERE TestId = @TestId AND TestGroup IS NULL;
	SELECT @@ROWCOUNT;
END
GO

-- ===== Bone Marrow (System.LabTestsBMT, TestGroup = 'BMT') =====

IF OBJECT_ID('System.GetLabTestsBMT') IS NOT NULL DROP PROCEDURE System.GetLabTestsBMT;
GO
CREATE PROCEDURE System.GetLabTestsBMT
AS
BEGIN
	SELECT TestId, TestName, TestUnit, [Order]
	FROM System.LabTestsBMT
	WHERE TestGroup = 'BMT'
	ORDER BY ISNULL([Order], 999999), TestName;
END
GO

IF OBJECT_ID('System.GetLabTestByIdBMT') IS NOT NULL DROP PROCEDURE System.GetLabTestByIdBMT;
GO
CREATE PROCEDURE System.GetLabTestByIdBMT
	@TestId int
AS
BEGIN
	SELECT TestId, TestName, TestUnit, [Order]
	FROM System.LabTestsBMT
	WHERE TestId = @TestId AND TestGroup = 'BMT';
END
GO

IF OBJECT_ID('System.InsertUpdateLabTestBMT') IS NOT NULL DROP PROCEDURE System.InsertUpdateLabTestBMT;
GO
CREATE PROCEDURE System.InsertUpdateLabTestBMT
	@TestId int,
	@TestName varchar(50),
	@TestUnit varchar(50),
	@Order int = NULL
AS
BEGIN
	IF @TestId > 0
	BEGIN
		UPDATE System.LabTestsBMT
		SET TestName = @TestName, TestUnit = @TestUnit, [Order] = @Order
		WHERE TestId = @TestId AND TestGroup = 'BMT';
		SELECT @TestId;
	END
	ELSE
	BEGIN
		INSERT INTO System.LabTestsBMT (TestName, TestUnit, TestGroup, [Order])
		VALUES (@TestName, @TestUnit, 'BMT', ISNULL(@Order, (SELECT ISNULL(MAX([Order]), 0) + 1 FROM System.LabTestsBMT WHERE TestGroup = 'BMT')));
		SELECT SCOPE_IDENTITY();
	END
END
GO

IF OBJECT_ID('System.DeleteLabTestBMT') IS NOT NULL DROP PROCEDURE System.DeleteLabTestBMT;
GO
CREATE PROCEDURE System.DeleteLabTestBMT
	@TestId int
AS
BEGIN
	DELETE FROM System.LabTestsBMT WHERE TestId = @TestId AND TestGroup = 'BMT';
	SELECT @@ROWCOUNT;
END
GO

-- ===== Role Rights =====

INSERT INTO System.Menu (MenuName, MenuGroupName, Url, MenuParentId, Active)
SELECT v.MenuName, 'Admin', NULL, NULL, 1
FROM (VALUES ('Lab Tests'), ('Lab Tests BMT')) AS v(MenuName)
WHERE NOT EXISTS (SELECT 1 FROM System.Menu m WHERE m.MenuName = v.MenuName);
GO

-- Granted to Admin by default; any other role can be granted these from the Role Rights screen.
INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete)
SELECT 1, m.MenuId, 1, 1, 1
FROM System.Menu m
WHERE m.MenuName IN ('Lab Tests', 'Lab Tests BMT')
AND NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 1 AND MenuId = m.MenuId);
GO
