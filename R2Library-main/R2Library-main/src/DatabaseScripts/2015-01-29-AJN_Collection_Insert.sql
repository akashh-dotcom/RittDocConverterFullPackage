
INSERT INTO tResourceCollectionType (iResourceCollectionTypeId, vchResourceCollectionTypeName)
VALUES (3, 'AJN Books of the Year');

INSERT INTO tResourceCollection (iResourceCollectionTypeId, iResourceId, vchCreatorId, dtCreationDate, tiRecordStatus)
select 3, iResourceId, 'Ken Haberle', GetDate(), 1 from tResource where vchResourceISBN in
('1284041387','1284043517','032322590X','080363921X','1938835158','1938835034','1937554384','1462513174','0803624913','0803627785','0803639082',
'0803623143','0826109756','0826110002','0826197825','0826129668','0826108946','0826137350','1462515592','1938835263','1938835344','1938835387',
'1938835077','1451191049','1935864440','0826137458','0826134653')
