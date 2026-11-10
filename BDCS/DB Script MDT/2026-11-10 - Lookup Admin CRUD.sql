-- Adds the missing admin CRUD surface for System.Lookup (previously insert-only).
-- GetAllLookupsForAdmin returns every row (active + inactive) so the admin screen
-- can group by category and let an admin reactivate something they soft-deleted.

CREATE OR ALTER proc [System].[GetAllLookupsForAdmin]
As
Begin
	Select l.LookupId, l.DisplayValue, l.ReturnValue, l.LookupType, l.IsActive
	from System.[Lookup] l
	order by l.LookupType, l.DisplayValue;
End;
GO

CREATE OR ALTER proc [System].[UpdateLookupValue]
	@LookupId int,
	@DisplayValue varchar(200),
	@ReturnValue varchar(200),
	@LookupType varchar(100)
As
Begin
	Update System.[Lookup]
	Set DisplayValue = @DisplayValue,
		ReturnValue = @ReturnValue,
		LookupType = @LookupType
	Where LookupId = @LookupId;

	Select @@ROWCOUNT;
End;
GO

CREATE OR ALTER proc [System].[ToggleLookupActive]
	@LookupId int
As
Begin
	Update System.[Lookup]
	Set IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
	Where LookupId = @LookupId;

	Select @@ROWCOUNT;
End;
GO
