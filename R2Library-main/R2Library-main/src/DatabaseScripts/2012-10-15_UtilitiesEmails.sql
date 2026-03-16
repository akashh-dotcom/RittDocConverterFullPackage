ALTER TABLE tNewResourceQue
ADD dtNewEditionEmail smalldateTime;

ALTER TABLE tNewResourceQue
ADD dtNewResourceEmail smalldateTime;

ALTER TABLE tNewResourceQue
ADD dtPurchasedEmail smalldateTime;

update tNewResourceQue 
set dtNewEditionEmail = GETDATE()
where tiProcessed = 1 and tiRecordStatus = 0;

update tNewResourceQue 
set dtNewResourceEmail = GETDATE()
where tiProcessed = 1 and tiRecordStatus = 0;

update tNewResourceQue 
set dtPurchasedEmail = GETDATE()
where tiProcessed = 1 and tiRecordStatus = 0;

