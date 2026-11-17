/*
	The "Admin" dropdown (Users, Roles, MDT Types, Locations) was wired to show for
	ANY logged-in user regardless of role, and the controllers behind it
	(UserController, RoleController, ClinicController, LocationController) had no
	[CheckUserRights] at all - any authenticated user, or a direct URL hit, could
	manage users/roles/clinics/locations.

	This adds those four as real, assignable Role Rights (same mechanism as
	Create Visit / Visit History / Unlock MDT Record) so an admin can grant them to
	any role from the existing Role Rights screen, instead of them being either
	wide open or hardcoded.

	"Lookups" already has its own right ("Add Lookup", MenuId 11) from before - left
	as-is, not duplicated here.

	Safe to re-run.
*/

INSERT INTO System.Menu (MenuName, MenuGroupName, Url, MenuParentId, Active)
SELECT v.MenuName, 'Admin', NULL, NULL, 1
FROM (VALUES ('Users'), ('Roles'), ('MDT Types'), ('Locations')) AS v(MenuName)
WHERE NOT EXISTS (SELECT 1 FROM System.Menu m WHERE m.MenuName = v.MenuName);
GO

-- Granted to Admin by default; any other role can be granted these from the Role Rights screen.
INSERT INTO System.RoleRight (RoleId, MenuId, CanView, CanAddEdit, CanDelete)
SELECT 1, m.MenuId, 1, 1, 0
FROM System.Menu m
WHERE m.MenuName IN ('Users', 'Roles', 'MDT Types', 'Locations')
AND NOT EXISTS (SELECT 1 FROM System.RoleRight WHERE RoleId = 1 AND MenuId = m.MenuId);
GO
