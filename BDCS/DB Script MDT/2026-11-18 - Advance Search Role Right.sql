/*
	Advance Search had no Role Right of its own - any logged-in user could reach
	it regardless of role. This adds it as a real, assignable Role Right (same
	mechanism as Create Visit / Visit History / Unlock MDT Record) so an admin
	can grant it to any role from the Role Rights screen.

	Safe to re-run.
*/

INSERT INTO System.Menu (MenuName, MenuGroupName, Url, MenuParentId, Active)
SELECT 'Advance Search', 'Patient', NULL, NULL, 1
WHERE NOT EXISTS (SELECT 1 FROM System.Menu WHERE MenuName = 'Advance Search');
GO

-- Granted to Admin by default; any other role can be granted it from the Role Rights screen.
INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete)
SELECT 1, m.MenuId, 1, 1, 0
FROM System.Menu m
WHERE m.MenuName = 'Advance Search'
AND NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 1 AND MenuId = m.MenuId);
GO
