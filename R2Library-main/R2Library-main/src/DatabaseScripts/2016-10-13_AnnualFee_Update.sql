
Update tInstitution
set dtAnnualFee = zz.dateAdded
from tInstitution i
join (
		select i.iInstitutionId, min(irl2.dtPdaAddedDate) as dateAdded
		from tInstitution i
		left join tInstitutionResourceLicense irl on i.iInstitutionId = irl.iInstitutionId and irl.dtFirstPurchaseDate is not null
		join tInstitutionResourceLicense irl2 on i.iInstitutionId = irl2.iInstitutionId 
		where i.tiPdaEulaSigned = 1 and i.dtAnnualFee is null and irl.iInstitutionResourceLicenseId is null
		group by i.iInstitutionId
	) zz on i.iInstitutionId = zz.iInstitutionId
where i.iInstitutionAcctStatusId = 1
