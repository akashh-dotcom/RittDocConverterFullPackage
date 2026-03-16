Alter Table tSavedReports 
add bIncludePurchased bit not null default 0;

Alter Table tSavedReports 
add bIncludePda bit not null default 0;

Alter Table tSavedReports 
add bIncludeToc bit not null default 0;


---------------------------------------
Alter Table tSavedReports 
add iPeriod int not null default 0;

Alter Table tSavedReports 
add dtPeriodStartDate smalldatetime null;

Alter Table tSavedReports 
add dtPeriodEndDate smalldatetime null;



update tSavedReports
set bIncludePurchased = 1,
	bIncludePda = 1,
	bIncludeToc = 	(case 
						when (bPurchasedOnly = 1)
						then 
							0
						else
							1
					end),
	iPeriod = 3;
	
	
	
Alter Table tUser 
add tiReceivePdaReport tinyint NOT NULL DEFAULT ((0)) 

	
update tUser
set tiReceivePdaAddToCart = 1,
	tiReceivePdaReport = 1
where iRoleId = 1
	
Alter Table tuser
Add tiReceiveArchivedAlert tinyint not null default ((0));

ALTER VIEW [dbo].[vPreludeCustomer]
AS
select accountNumber as vchAccountNumber, accountName as vchAccountName, 
       billToAddress1 as vchBillToAddress1, billToCity as vchBillToCity, 
       billToState as vchBillToState, billToZip as vchBillToZip, billToCountry as vchBillToCountry, 
       confirmEmail as vchEmailAddress, billToPhone as vchBillToPhone, billToFax as vchBillToFax, 
       isRIS as vchIsRIS, billToAddress2 as vchBillToAddress2, billToAddress3 as vchBillToAddress3,
	   territory as vchTerritory 
from   [technoserv04\sql2005].PreludeData.dbo.Customer



