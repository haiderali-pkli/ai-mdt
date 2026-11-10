-- DeleteRoleRightAgainstRole used "delete top(1)", so saving a role only ever
-- removed ONE of however many duplicate (RoleId, MenuId) rows had already
-- accumulated, before inserting a fresh one net duplicates never shrank.
-- This is why the Roles screen showed the same menu name repeated (e.g. "Add
-- Patient" x3-8): System.Menu itself was fine, System.RoleRight had piled up
-- 58 rows for only 25 distinct (RoleId, MenuId) pairs, all agreeing on their
-- CanView/CanAddEdit/CanDelete values (verified before running this cleanup).

CREATE OR ALTER procedure [System].[DeleteRoleRightAgainstRole]
@RoleId int,
@MenuId int
as
begin
	delete System.RoleRight
	where RoleId = @RoleId
	and MenuId = @MenuId;
end
GO

-- One-time cleanup: keep the most recent row per (RoleId, MenuId), drop the rest.
;WITH Ranked AS (
	SELECT RightIdt,
		ROW_NUMBER() OVER (PARTITION BY RoleId, MenuId ORDER BY RightIdt DESC) AS rn
	FROM System.RoleRight
)
DELETE FROM System.RoleRight
WHERE RightIdt IN (SELECT RightIdt FROM Ranked WHERE rn > 1);
GO
