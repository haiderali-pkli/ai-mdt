/*
	Visit History popup: the Print icon was always clickable even for a visit with no
	MDT record saved yet (nothing to print), and there was no visual way to tell a
	blank visit apart from one that's already been filled in and locked. Adds HasData
	(does Clinical.PatientDetail/PatientDetailBMT have a row for this visit) and
	IsLocked so the UI can disable Print and show a Blank/Locked status badge.

	Safe to re-run.
*/

CREATE OR ALTER proc [Clinical].[GetVisitHistory]
@patientId INT,
@MDTType VARCHAR(250)
AS
Begin
Select v.VisitId,v.PatientId, concat(p.FirstName , ' ' , p.LastName) as PatientName,v.VisitDateTime,v.Reason,v.MDTType,
	CASE WHEN pd.PatientDetailId IS NOT NULL OR pdb.PatientDetailId IS NOT NULL THEN 1 ELSE 0 END AS HasData,
	ISNULL(pd.IsLocked, ISNULL(pdb.IsLocked, 0)) AS IsLocked
from Clinical.PatientVisit v
inner join Clinical.Patient p
on v.PatientId=p.PatientId
left join Clinical.PatientDetail pd on pd.VisitId = v.VisitId
left join Clinical.PatientDetailBMT pdb on pdb.VisitId = v.VisitId
Where v.PatientId=@patientId
AND v.MDTType=@MDTType
order by v.VisitDateTime desc
End;
GO
