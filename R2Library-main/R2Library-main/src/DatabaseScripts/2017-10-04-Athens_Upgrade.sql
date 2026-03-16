alter table tuser
add vchAthensTargetedId varchar (50) null;

alter table tInstitution
add vchAthensScopedAffiliation varchar(max) null;
go



UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',123doc.com' else '123doc.com' end WHERE [vchAthensOrgId] like '%5921442%' and len('5921442') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762748.eng.nhs.uk' else '5762748.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762748%' and len('5762748') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',6107516.wales.nhs.uk' else '6107516.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%6107516%' and len('6107516') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7472037.wales.nhs.uk' else '7472037.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7472037%' and len('7472037') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',abtassociates.com' else 'abtassociates.com' end WHERE [vchAthensOrgId] like '%70339913%' and len('70339913') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',seha.ae' else 'seha.ae' end WHERE [vchAthensOrgId] like '%68694858%' and len('68694858') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tcaabudhabi.ae' else 'tcaabudhabi.ae' end WHERE [vchAthensOrgId] like '%70215683%' and len('70215683') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',elecbook.com' else 'elecbook.com' end WHERE [vchAthensOrgId] like '%5010829%' and len('5010829') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523079.swanlibraries.net' else '69523079.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523079%' and len('69523079') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',acs-schools.com' else 'acs-schools.com' end WHERE [vchAthensOrgId] like '%66992417%' and len('66992417') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ocvc.ac.uk' else 'ocvc.ac.uk' end WHERE [vchAthensOrgId] like '%4757304%' and len('4757304') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ampltd.co.uk' else 'ampltd.co.uk' end WHERE [vchAthensOrgId] like '%7046822%' and len('7046822') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073201.eng.nhs.uk' else '4073201.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073201%' and len('4073201') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',amnch.ie' else 'amnch.ie' end WHERE [vchAthensOrgId] like '%856730%' and len('856730') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',adfsdemo.openathens.net' else 'adfsdemo.openathens.net' end WHERE [vchAthensOrgId] like '%69205659%' and len('69205659') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',d125.org' else 'd125.org' end WHERE [vchAthensOrgId] like '%68921588%' and len('68921588') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',adventisthealthcare.com' else 'adventisthealthcare.com' end WHERE [vchAthensOrgId] like '%70306001%' and len('70306001') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',anii.org.uy' else 'anii.org.uy' end WHERE [vchAthensOrgId] like '%70365582%' and len('70365582') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ahrq.hhs.gov' else 'ahrq.hhs.gov' end WHERE [vchAthensOrgId] like '%66937413%' and len('66937413') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',agilent.com' else 'agilent.com' end WHERE [vchAthensOrgId] like '%68778361%' and len('68778361') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69688988.iuhealth.org' else '69688988.iuhealth.org' end WHERE [vchAthensOrgId] like '%69688988%' and len('69688988') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044116.eng.nhs.uk' else '4044116.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044116%' and len('4044116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aip.org' else 'aip.org' end WHERE [vchAthensOrgId] like '%69707046%' and len('69707046') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66270965.us.af.mil' else '66270965.us.af.mil' end WHERE [vchAthensOrgId] like '%66270965%' and len('66270965') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058671.eng.nhs.uk' else '4058671.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058671%' and len('4058671') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ajman.ac.ae' else 'ajman.ac.ae' end WHERE [vchAthensOrgId] like '%70210359%' and len('70210359') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ahus.no' else 'ahus.no' end WHERE [vchAthensOrgId] like '%68601451%' and len('68601451') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',55198945.akzonobel.com' else '55198945.akzonobel.com' end WHERE [vchAthensOrgId] like '%55198945%' and len('55198945') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118160.akzonobel.com' else '66118160.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118160%' and len('66118160') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118161.akzonobel.com' else '66118161.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118161%' and len('66118161') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66161719.akzonobel.com' else '66161719.akzonobel.com' end WHERE [vchAthensOrgId] like '%66161719%' and len('66161719') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118163.akzonobel.com' else '66118163.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118163%' and len('66118163') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924251.va.gov' else '54924251.va.gov' end WHERE [vchAthensOrgId] like '%54924251%' and len('54924251') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044118.eng.nhs.uk' else '4044118.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044118%' and len('4044118') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924224.va.gov' else '54924224.va.gov' end WHERE [vchAthensOrgId] like '%54924224%' and len('54924224') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633430.va.gov' else '43633430.va.gov' end WHERE [vchAthensOrgId] like '%43633430%' and len('43633430') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',alfredhealth.gov.au' else 'alfredhealth.gov.au' end WHERE [vchAthensOrgId] like '%54714365%' and len('54714365') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',amu.ac.in' else 'amu.ac.in' end WHERE [vchAthensOrgId] like '%70005187%' and len('70005187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aiims.ac.in' else 'aiims.ac.in' end WHERE [vchAthensOrgId] like '%68753633%' and len('68753633') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12798585.wales.nhs.uk' else '12798585.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%12798585%' and len('12798585') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523080.swanlibraries.net' else '69523080.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523080%' and len('69523080') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69846488.ps.openathens.net' else '69846488.ps.openathens.net' end WHERE [vchAthensOrgId] like '%69846488%' and len('69846488') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924237.va.gov' else '54924237.va.gov' end WHERE [vchAthensOrgId] like '%54924237%' and len('54924237') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66755897.medlinet.amedd.army.mil' else '66755897.medlinet.amedd.army.mil' end WHERE [vchAthensOrgId] like '%66755897%' and len('66755897') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aap.org' else 'aap.org' end WHERE [vchAthensOrgId] like '%15805774%' and len('15805774') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aacc.org' else 'aacc.org' end WHERE [vchAthensOrgId] like '%67507318%' and len('67507318') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aaas.org' else 'aaas.org' end WHERE [vchAthensOrgId] like '%68272612%' and len('68272612') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',acs.org' else 'acs.org' end WHERE [vchAthensOrgId] like '%69557049%' and len('69557049') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ace.edu' else 'ace.edu' end WHERE [vchAthensOrgId] like '%70005213%' and len('70005213') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66151664.acponline.org' else '66151664.acponline.org' end WHERE [vchAthensOrgId] like '%66151664%' and len('66151664') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ada.org' else 'ada.org' end WHERE [vchAthensOrgId] like '%69594084%' and len('69594084') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',econlit.org' else 'econlit.org' end WHERE [vchAthensOrgId] like '%55208426%' and len('55208426') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aiaa.org' else 'aiaa.org' end WHERE [vchAthensOrgId] like '%67011392%' and len('67011392') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ama-assn.org' else 'ama-assn.org' end WHERE [vchAthensOrgId] like '%12628063%' and len('12628063') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',the-aps.org' else 'the-aps.org' end WHERE [vchAthensOrgId] like '%67004901%' and len('67004901') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',psych.org' else 'psych.org' end WHERE [vchAthensOrgId] like '%54911574%' and len('54911574') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',apa.org' else 'apa.org' end WHERE [vchAthensOrgId] like '%5519125%' and len('5519125') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',asmusa.org' else 'asmusa.org' end WHERE [vchAthensOrgId] like '%67243038%' and len('67243038') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',asco.org' else 'asco.org' end WHERE [vchAthensOrgId] like '%7233260%' and len('7233260') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hematology.org' else 'hematology.org' end WHERE [vchAthensOrgId] like '%13434115%' and len('13434115') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',amritahospital.org' else 'amritahospital.org' end WHERE [vchAthensOrgId] like '%70216572%' and len('70216572') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70026501.oaebsco.com' else '70026501.oaebsco.com' end WHERE [vchAthensOrgId] like '%70026501%' and len('70026501') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7472074.wales.nhs.uk' else '7472074.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7472074%' and len('7472074') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',anglia.ac.uk' else 'anglia.ac.uk' end WHERE [vchAthensOrgId] like '%133%' and len('133') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.anglia.ac.uk' else 'sp.anglia.ac.uk' end WHERE [vchAthensOrgId] like '%1822879%' and len('1822879') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073159.eng.nhs.uk' else '4073159.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073159%' and len('4073159') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',vla.defra.gsi.gov.uk' else 'vla.defra.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%6913797%' and len('6913797') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',annualreviews.org' else 'annualreviews.org' end WHERE [vchAthensOrgId] like '%69472062%' and len('69472062') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iibr.gov.uk' else 'iibr.gov.uk' end WHERE [vchAthensOrgId] like '%68168892%' and len('68168892') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',anonymouspetcare.com' else 'anonymouspetcare.com' end WHERE [vchAthensOrgId] like '%68526168%' and len('68526168') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pfizer.com' else 'pfizer.com' end WHERE [vchAthensOrgId] like '%67918917%' and len('67918917') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',apexsot.edu' else 'apexsot.edu' end WHERE [vchAthensOrgId] like '%69414829%' and len('69414829') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',agu.edu.bh' else 'agu.edu.bh' end WHERE [vchAthensOrgId] like '%68124359%' and len('68124359') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aaschool.ac.uk' else 'aaschool.ac.uk' end WHERE [vchAthensOrgId] like '%66671503%' and len('66671503') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514919.bibliosalut.com' else '54514919.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514919%' and len('54514919') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514920.bibliosalut.com' else '54514920.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514920%' and len('54514920') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54540301.ahsl.arizona.edu' else '54540301.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54540301%' and len('54540301') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66270966.us.army.mil' else '66270966.us.army.mil' end WHERE [vchAthensOrgId] like '%66270966%' and len('66270966') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',arnothealth.org' else 'arnothealth.org' end WHERE [vchAthensOrgId] like '%70120098%' and len('70120098') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5343880.eng.nhs.uk' else '5343880.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5343880%' and len('5343880') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aucb.ac.uk' else 'aucb.ac.uk' end WHERE [vchAthensOrgId] like '%1717941%' and len('1717941') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',artstor.org' else 'artstor.org' end WHERE [vchAthensOrgId] like '%69996409%' and len('69996409') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',arup.com' else 'arup.com' end WHERE [vchAthensOrgId] like '%68863361%' and len('68863361') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924193.va.gov' else '54924193.va.gov' end WHERE [vchAthensOrgId] like '%54924193%' and len('54924193') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223611.eng.nhs.uk' else '4223611.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223611%' and len('4223611') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ashridge.org.uk' else 'ashridge.org.uk' end WHERE [vchAthensOrgId] like '%68121385%' and len('68121385') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',acm.org' else 'acm.org' end WHERE [vchAthensOrgId] like '%66173555%' and len('66173555') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',assuta.co.il' else 'assuta.co.il' end WHERE [vchAthensOrgId] like '%68845005%' and len('68845005') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',astm.org' else 'astm.org' end WHERE [vchAthensOrgId] like '%69742723%' and len('69742723') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',aston.ac.uk' else 'aston.ac.uk' end WHERE [vchAthensOrgId] like '%102%' and len('102') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cengage.co.uk' else 'cengage.co.uk' end WHERE [vchAthensOrgId] like '%5675184%' and len('5675184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924194.va.gov' else '54924194.va.gov' end WHERE [vchAthensOrgId] like '%54924194%' and len('54924194') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54754623.atlantichealth.org' else '54754623.atlantichealth.org' end WHERE [vchAthensOrgId] like '%54754623%' and len('54754623') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',atypon.com' else 'atypon.com' end WHERE [vchAthensOrgId] like '%7433022%' and len('7433022') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',atypon.com' else 'atypon.com' end WHERE [vchAthensOrgId] like '%7392335%' and len('7392335') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69032264.wslhd.health.nsw.gov.au' else '69032264.wslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%69032264%' and len('69032264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',auroracollege.nt.ca' else 'auroracollege.nt.ca' end WHERE [vchAthensOrgId] like '%69988551%' and len('69988551') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',21century.com.au' else '21century.com.au' end WHERE [vchAthensOrgId] like '%43657173%' and len('43657173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',acer.edu.au' else 'acer.edu.au' end WHERE [vchAthensOrgId] like '%43617370%' and len('43617370') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ahpra.gov.au' else 'ahpra.gov.au' end WHERE [vchAthensOrgId] like '%69369111%' and len('69369111') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118164.akzonobel.com' else '66118164.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118164%' and len('66118164') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762695.eng.nhs.uk' else '5762695.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762695%' and len('5762695') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',awe.co.uk' else 'awe.co.uk' end WHERE [vchAthensOrgId] like '%12757832%' and len('12757832') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ayrcoll.ac.uk' else 'ayrcoll.ac.uk' end WHERE [vchAthensOrgId] like '%6714808%' and len('6714808') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12606376.ahsl.arizona.edu' else '12606376.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12606376%' and len('12606376') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',babraham.bbsrc.ac.uk' else 'babraham.bbsrc.ac.uk' end WHERE [vchAthensOrgId] like '%85826%' and len('85826') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ballarathealth.gov.au' else 'ballarathealth.gov.au' end WHERE [vchAthensOrgId] like '%60400842%' and len('60400842') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',banrep.gov.co' else 'banrep.gov.co' end WHERE [vchAthensOrgId] like '%70011245%' and len('70011245') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bids.org.bd' else 'bids.org.bd' end WHERE [vchAthensOrgId] like '%67860645%' and len('67860645') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',buet.ac.bd' else 'buet.ac.bd' end WHERE [vchAthensOrgId] like '%67579245%' and len('67579245') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bankstown.nsw.gov.au' else 'bankstown.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54432888%' and len('54432888') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54319653.ahsl.arizona.edu' else '54319653.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54319653%' and len('54319653') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849248.ahsl.arizona.edu' else '12849248.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12849248%' and len('12849248') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849234.ahsl.arizona.edu' else '12849234.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%68891012%' and len('68891012') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54516280.ahsl.arizona.edu' else '54516280.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54516280%' and len('54516280') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54319651.ahsl.arizona.edu' else '54319651.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54319651%' and len('54319651') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54319654.ahsl.arizona.edu' else '54319654.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54319654%' and len('54319654') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54319652.ahsl.arizona.edu' else '54319652.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54319652%' and len('54319652') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54319649.ahsl.arizona.edu' else '54319649.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54319649%' and len('54319649') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69479524.ahsl.arizona.edu' else '69479524.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%69479524%' and len('69479524') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849234.ahsl.arizona.edu' else '12849234.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12849234%' and len('12849234') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',60451895.ahsl.arizona.edu' else '60451895.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%60451895%' and len('60451895') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54319650.ahsl.arizona.edu' else '54319650.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54319650%' and len('54319650') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bhsi.com' else 'bhsi.com' end WHERE [vchAthensOrgId] like '%66328977%' and len('66328977') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',baptisthealth.net' else 'baptisthealth.net' end WHERE [vchAthensOrgId] like '%38508955%' and len('38508955') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biu.ac.il' else 'biu.ac.il' end WHERE [vchAthensOrgId] like '%53947016%' and len('53947016') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531171.eng.nhs.uk' else '5531171.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531171%' and len('5531171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',barnardos.org.uk' else 'barnardos.org.uk' end WHERE [vchAthensOrgId] like '%66056218%' and len('66056218') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',barnardos.ie' else 'barnardos.ie' end WHERE [vchAthensOrgId] like '%69225365%' and len('69225365') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531514.eng.nhs.uk' else '5531514.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531514%' and len('5531514') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531163.eng.nhs.uk' else '5531163.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531163%' and len('5531163') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',barnsley.ac.uk' else 'barnsley.ac.uk' end WHERE [vchAthensOrgId] like '%1714264%' and len('1714264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315906.eng.nhs.uk' else '4315906.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315906%' and len('4315906') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531172.eng.nhs.uk' else '5531172.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531172%' and len('5531172') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',basch.com' else 'basch.com' end WHERE [vchAthensOrgId] like '%68604350%' and len('68604350') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073147.eng.nhs.uk' else '4073147.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073147%' and len('4073147') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bathspa.ac.uk' else 'bathspa.ac.uk' end WHERE [vchAthensOrgId] like '%146931%' and len('146931') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924179.va.gov' else '54924179.va.gov' end WHERE [vchAthensOrgId] like '%54924179%' and len('54924179') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924219.va.gov' else '54924219.va.gov' end WHERE [vchAthensOrgId] like '%54924219%' and len('54924219') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924201.va.gov' else '54924201.va.gov' end WHERE [vchAthensOrgId] like '%54924201%' and len('54924201') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',baycare.org' else 'baycare.org' end WHERE [vchAthensOrgId] like '%60354204%' and len('60354204') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43610959.dhss.delaware.gov' else '43610959.dhss.delaware.gov' end WHERE [vchAthensOrgId] like '%43610959%' and len('43610959') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',baylorschool.org' else 'baylorschool.org' end WHERE [vchAthensOrgId] like '%69855618%' and len('69855618') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66992209.polk.amedd.army.mil' else '66992209.polk.amedd.army.mil' end WHERE [vchAthensOrgId] like '%66992209%' and len('66992209') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bbc.co.uk' else 'bbc.co.uk' end WHERE [vchAthensOrgId] like '%70184061%' and len('70184061') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',swindon.bbsrc.ac.uk' else 'swindon.bbsrc.ac.uk' end WHERE [vchAthensOrgId] like '%85837%' and len('85837') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7371907.wales.nhs.uk' else '7371907.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7371907%' and len('7371907') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',6195759.wales.nhs.uk' else '6195759.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%6195759%' and len('6195759') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7459015.wales.nhs.uk' else '7459015.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7459015%' and len('7459015') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514896.va.gov' else '54514896.va.gov' end WHERE [vchAthensOrgId] like '%54514896%' and len('54514896') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073181.eng.nhs.uk' else '4073181.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073181%' and len('4073181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bedford.ac.uk' else 'bedford.ac.uk' end WHERE [vchAthensOrgId] like '%3620153%' and len('3620153') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073176.eng.nhs.uk' else '4073176.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073176%' and len('4073176') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523081.swanlibraries.net' else '69523081.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523081%' and len('69523081') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43610960.dhss.delaware.gov' else '43610960.dhss.delaware.gov' end WHERE [vchAthensOrgId] like '%43610960%' and len('43610960') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523082.swanlibraries.net' else '69523082.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523082%' and len('69523082') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ariela.tel-aviv.gov.il' else 'ariela.tel-aviv.gov.il' end WHERE [vchAthensOrgId] like '%65786539%' and len('65786539') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597589.houstontx.gov' else '69597589.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597589%' and len('69597589') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523083.swanlibraries.net' else '69523083.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523083%' and len('69523083') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bendigohealthcare.gov.au' else 'bendigohealthcare.gov.au' end WHERE [vchAthensOrgId] like '%55177850%' and len('55177850') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bennington.edu' else 'bennington.edu' end WHERE [vchAthensOrgId] like '%70267393%' and len('70267393') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523084.swanlibraries.net' else '69523084.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523084%' and len('69523084') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',berklee.edu' else 'berklee.edu' end WHERE [vchAthensOrgId] like '%70266361%' and len('70266361') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223535.eng.nhs.uk' else '4223535.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223535%' and len('4223535') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523085.swanlibraries.net' else '69523085.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523085%' and len('69523085') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bethel.de' else 'bethel.de' end WHERE [vchAthensOrgId] like '%68109690%' and len('68109690') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bexley.ac.uk' else 'bexley.ac.uk' end WHERE [vchAthensOrgId] like '%1787232%' and len('1787232') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54333409.bibliosalut.com' else '54333409.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54333409%' and len('54333409') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gobiernodecanarias.org' else 'gobiernodecanarias.org' end WHERE [vchAthensOrgId] like '%67522392%' and len('67522392') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ingentaconnect.com' else 'ingentaconnect.com' end WHERE [vchAthensOrgId] like '%219%' and len('219') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69821031%' and len('69821031') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bilborough.ac.uk' else 'bilborough.ac.uk' end WHERE [vchAthensOrgId] like '%2703246%' and len('2703246') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5160384.eng.nhs.uk' else '5160384.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5160384%' and len('5160384') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biogen.com' else 'biogen.com' end WHERE [vchAthensOrgId] like '%69599485%' and len('69599485') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biomedcentral.com' else 'biomedcentral.com' end WHERE [vchAthensOrgId] like '%12661270%' and len('12661270') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biomedcentral.com' else 'biomedcentral.com' end WHERE [vchAthensOrgId] like '%12661280%' and len('12661280') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biomedcentral.com' else 'biomedcentral.com' end WHERE [vchAthensOrgId] like '%54549663%' and len('54549663') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biomedcentral.com' else 'biomedcentral.com' end WHERE [vchAthensOrgId] like '%3396028%' and len('3396028') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',arl.org' else 'arl.org' end WHERE [vchAthensOrgId] like '%66074482%' and len('66074482') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bbsrc.ac.uk' else 'bbsrc.ac.uk' end WHERE [vchAthensOrgId] like '%101%' and len('101') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',biotronik.com' else 'biotronik.com' end WHERE [vchAthensOrgId] like '%68784620%' and len('68784620') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bbk.ac.uk' else 'bbk.ac.uk' end WHERE [vchAthensOrgId] like '%15022%' and len('15022') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195506.eng.nhs.uk' else '5195506.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195506%' and len('5195506') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195496.eng.nhs.uk' else '5195496.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195496%' and len('5195496') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bcu.ac.uk' else 'bcu.ac.uk' end WHERE [vchAthensOrgId] like '%187%' and len('187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195492.eng.nhs.uk' else '5195492.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195492%' and len('5195492') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195486.eng.nhs.uk' else '5195486.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195486%' and len('5195486') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195487.eng.nhs.uk' else '5195487.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195487%' and len('5195487') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924196.va.gov' else '54924196.va.gov' end WHERE [vchAthensOrgId] like '%54924196%' and len('54924196') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195498.eng.nhs.uk' else '5195498.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195498%' and len('5195498') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bishopb-college.ac.uk' else 'bishopb-college.ac.uk' end WHERE [vchAthensOrgId] like '%569709%' and len('569709') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bishopg.ac.uk' else 'bishopg.ac.uk' end WHERE [vchAthensOrgId] like '%580908%' and len('580908') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195499.eng.nhs.uk' else '5195499.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195499%' and len('5195499') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',blackpoolsixth.ac.uk' else 'blackpoolsixth.ac.uk' end WHERE [vchAthensOrgId] like '%68960633%' and len('68960633') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044098.eng.nhs.uk' else '4044098.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044098%' and len('4044098') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69032263.wslhd.health.nsw.gov.au' else '69032263.wslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%69032263%' and len('69032263') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',brcn.edu' else 'brcn.edu' end WHERE [vchAthensOrgId] like '%68517486%' and len('68517486') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bloomsbury.com' else 'bloomsbury.com' end WHERE [vchAthensOrgId] like '%65469237%' and len('65469237') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523086.swanlibraries.net' else '69523086.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523086%' and len('69523086') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bmjgroup.com' else 'bmjgroup.com' end WHERE [vchAthensOrgId] like '%4087881%' and len('4087881') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924252.va.gov' else '54924252.va.gov' end WHERE [vchAthensOrgId] like '%54924252%' and len('54924252') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044158.eng.nhs.uk' else '4044158.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044158%' and len('4044158') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112323.eng.nhs.uk' else '4112323.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112323%' and len('4112323') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710685.bshsi.org' else '53710685.bshsi.org' end WHERE [vchAthensOrgId] like '%53710685%' and len('53710685') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710695.bshsi.org' else '53710695.bshsi.org' end WHERE [vchAthensOrgId] like '%53710695%' and len('53710695') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710672.bshsi.org' else '53710672.bshsi.org' end WHERE [vchAthensOrgId] like '%53710672%' and len('53710672') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710696.bshsi.org' else '53710696.bshsi.org' end WHERE [vchAthensOrgId] like '%53710696%' and len('53710696') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43640323.bshsi.org' else '43640323.bshsi.org' end WHERE [vchAthensOrgId] like '%68778594%' and len('68778594') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43640323.bshsi.org' else '43640323.bshsi.org' end WHERE [vchAthensOrgId] like '%43640323%' and len('43640323') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710684.bshsi.org' else '53710684.bshsi.org' end WHERE [vchAthensOrgId] like '%53710684%' and len('53710684') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710675.bshsi.org' else '53710675.bshsi.org' end WHERE [vchAthensOrgId] like '%53710675%' and len('53710675') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710697.bshsi.org' else '53710697.bshsi.org' end WHERE [vchAthensOrgId] like '%53710697%' and len('53710697') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710698.bshsi.org' else '53710698.bshsi.org' end WHERE [vchAthensOrgId] like '%53710698%' and len('53710698') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710699.bshsi.org' else '53710699.bshsi.org' end WHERE [vchAthensOrgId] like '%53710699%' and len('53710699') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710700.bshsi.org' else '53710700.bshsi.org' end WHERE [vchAthensOrgId] like '%53710700%' and len('53710700') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710676.bshsi.org' else '53710676.bshsi.org' end WHERE [vchAthensOrgId] like '%53710676%' and len('53710676') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70182820.bshsi.org' else '70182820.bshsi.org' end WHERE [vchAthensOrgId] like '%70182820%' and len('70182820') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710701.bshsi.org' else '53710701.bshsi.org' end WHERE [vchAthensOrgId] like '%53710701%' and len('53710701') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710674.bshsi.org' else '53710674.bshsi.org' end WHERE [vchAthensOrgId] like '%53710674%' and len('53710674') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710677.bshsi.org' else '53710677.bshsi.org' end WHERE [vchAthensOrgId] like '%53710677%' and len('53710677') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710702.bshsi.org' else '53710702.bshsi.org' end WHERE [vchAthensOrgId] like '%53710702%' and len('53710702') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710678.bshsi.org' else '53710678.bshsi.org' end WHERE [vchAthensOrgId] like '%53710678%' and len('53710678') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710703.bshsi.org' else '53710703.bshsi.org' end WHERE [vchAthensOrgId] like '%53710703%' and len('53710703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',boras.se' else 'boras.se' end WHERE [vchAthensOrgId] like '%70149767%' and len('70149767') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',boston.ac.uk' else 'boston.ac.uk' end WHERE [vchAthensOrgId] like '%69986477%' and len('69986477') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bls.org' else 'bls.org' end WHERE [vchAthensOrgId] like '%70372031%' and len('70372031') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924171.va.gov' else '54924171.va.gov' end WHERE [vchAthensOrgId] like '%54924171%' and len('54924171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bournemouth.ac.uk' else 'bournemouth.ac.uk' end WHERE [vchAthensOrgId] like '%261%' and len('261') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bpglobal.com' else 'bpglobal.com' end WHERE [vchAthensOrgId] like '%67527427%' and len('67527427') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bppls.com' else 'bppls.com' end WHERE [vchAthensOrgId] like '%1798841%' and len('1798841') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bracu.ac.bd' else 'bracu.ac.bd' end WHERE [vchAthensOrgId] like '%66348955%' and len('66348955') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058672.eng.nhs.uk' else '4058672.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058672%' and len('4058672') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058673.eng.nhs.uk' else '4058673.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058673%' and len('4058673') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',brainfuseinc.com' else 'brainfuseinc.com' end WHERE [vchAthensOrgId] like '%70284455%' and len('70284455') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bridgend.ac.uk' else 'bridgend.ac.uk' end WHERE [vchAthensOrgId] like '%3475426%' and len('3475426') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bpthosp.org' else 'bpthosp.org' end WHERE [vchAthensOrgId] like '%69455265%' and len('69455265') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523087.swanlibraries.net' else '69523087.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523087%' and len('69523087') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112875.eng.nhs.uk' else '4112875.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112875%' and len('4112875') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.eng.nhs.uk' else 'sp.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69962443%' and len('69962443') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223612.eng.nhs.uk' else '4223612.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223612%' and len('4223612') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bhasvic.ac.uk' else 'bhasvic.ac.uk' end WHERE [vchAthensOrgId] like '%5943612%' and len('5943612') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bir.org.uk' else 'bir.org.uk' end WHERE [vchAthensOrgId] like '%67391745%' and len('67391745') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thebritishmuseum.ac.uk' else 'thebritishmuseum.ac.uk' end WHERE [vchAthensOrgId] like '%15776190%' and len('15776190') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69443603.eng.nhs.uk' else '69443603.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69443603%' and len('69443603') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bsigroup.com' else 'bsigroup.com' end WHERE [vchAthensOrgId] like '%1076494%' and len('1076494') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523088.swanlibraries.net' else '69523088.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523088%' and len('69523088') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bromley.ac.uk' else 'bromley.ac.uk' end WHERE [vchAthensOrgId] like '%66294231%' and len('66294231') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bromley.ac.uk' else 'bromley.ac.uk' end WHERE [vchAthensOrgId] like '%191088%' and len('191088') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66289368.eng.nhs.uk' else '66289368.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66289368%' and len('66289368') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38306282.va.gov' else '38306282.va.gov' end WHERE [vchAthensOrgId] like '%38306282%' and len('38306282') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',brookehouse.ac.uk' else 'brookehouse.ac.uk' end WHERE [vchAthensOrgId] like '%4294772%' and len('4294772') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',brookehouse.ac.uk' else 'brookehouse.ac.uk' end WHERE [vchAthensOrgId] like '%4294772%' and len('4294772') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523089.swanlibraries.net' else '69523089.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523089%' and len('69523089') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523090.swanlibraries.net' else '69523090.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523090%' and len('69523090') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38306281.va.gov' else '38306281.va.gov' end WHERE [vchAthensOrgId] like '%38306281%' and len('38306281') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66079024.shell.com' else '66079024.shell.com' end WHERE [vchAthensOrgId] like '%66079024%' and len('66079024') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223526.eng.nhs.uk' else '4223526.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223526%' and len('4223526') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68894388.bucks.ac.uk' else '68894388.bucks.ac.uk' end WHERE [vchAthensOrgId] like '%68894388%' and len('68894388') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68894387.bucks.ac.uk' else '68894387.bucks.ac.uk' end WHERE [vchAthensOrgId] like '%68894387%' and len('68894387') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bucks.ac.uk' else 'bucks.ac.uk' end WHERE [vchAthensOrgId] like '%68882875%' and len('68882875') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bupa.com' else 'bupa.com' end WHERE [vchAthensOrgId] like '%69582138%' and len('69582138') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',48674604.bvdinfo.com' else '48674604.bvdinfo.com' end WHERE [vchAthensOrgId] like '%48674604%' and len('48674604') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',48674605.bvdinfo.com' else '48674605.bvdinfo.com' end WHERE [vchAthensOrgId] like '%48674605%' and len('48674605') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bvdinfo.com' else 'bvdinfo.com' end WHERE [vchAthensOrgId] like '%12653836%' and len('12653836') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bvdinfo.com' else 'bvdinfo.com' end WHERE [vchAthensOrgId] like '%582828%' and len('582828') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195542.eng.nhs.uk' else '5195542.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195542%' and len('5195542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924184.va.gov' else '54924184.va.gov' end WHERE [vchAthensOrgId] like '%54924184%' and len('54924184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',65911929.bibliosalut.com' else '65911929.bibliosalut.com' end WHERE [vchAthensOrgId] like '%65911929%' and len('65911929') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cabi.org' else 'cabi.org' end WHERE [vchAthensOrgId] like '%5765427%' and len('5765427') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cabrini.com.au' else 'cabrini.com.au' end WHERE [vchAthensOrgId] like '%68175065%' and len('68175065') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cadencehealth.org' else 'cadencehealth.org' end WHERE [vchAthensOrgId] like '%69153124%' and len('69153124') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69450621.ckn.qldhealth.com' else '69450621.ckn.qldhealth.com' end WHERE [vchAthensOrgId] like '%69450621%' and len('69450621') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058674.eng.nhs.uk' else '4058674.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058674%' and len('4058674') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',calicolifesciences.com' else 'calicolifesciences.com' end WHERE [vchAthensOrgId] like '%68146258%' and len('68146258') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523091.swanlibraries.net' else '69523091.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523091%' and len('69523091') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523092.swanlibraries.net' else '69523092.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523092%' and len('69523092') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12520024.cambridge.com' else '12520024.cambridge.com' end WHERE [vchAthensOrgId] like '%12520024%' and len('12520024') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cambridge.com' else 'cambridge.com' end WHERE [vchAthensOrgId] like '%4033049%' and len('4033049') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4077652.eng.nhs.uk' else '4077652.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4077652%' and len('4077652') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073196.eng.nhs.uk' else '4073196.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073196%' and len('4073196') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531164.eng.nhs.uk' else '5531164.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531164%' and len('5531164') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',library.nsw.uca.org.au' else 'library.nsw.uca.org.au' end WHERE [vchAthensOrgId] like '%70004310%' and len('70004310') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',campbelltown.nsw.gov.au' else 'campbelltown.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54432924%' and len('54432924') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924180.va.gov' else '54924180.va.gov' end WHERE [vchAthensOrgId] like '%54924180%' and len('54924180') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',canterbury.ac.uk' else 'canterbury.ac.uk' end WHERE [vchAthensOrgId] like '%140%' and len('140') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',canterbury.nsw.gov.au' else 'canterbury.nsw.gov.au' end WHERE [vchAthensOrgId] like '%55158034%' and len('55158034') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924230.va.gov' else '54924230.va.gov' end WHERE [vchAthensOrgId] like '%54924230%' and len('54924230') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7472022.wales.nhs.uk' else '7472022.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7472022%' and len('7472022') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cghsnc.org' else 'cghsnc.org' end WHERE [vchAthensOrgId] like '%70058650%' and len('70058650') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7451962.eng.nhs.uk' else '7451962.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%7451962%' and len('7451962') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',carey.ac.nz' else 'carey.ac.nz' end WHERE [vchAthensOrgId] like '%68678835%' and len('68678835') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cmi.edu.jm' else 'cmi.edu.jm' end WHERE [vchAthensOrgId] like '%69699432%' and len('69699432') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cchsl.com' else 'cchsl.com' end WHERE [vchAthensOrgId] like '%67562220%' and len('67562220') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',carilionclinic.org' else 'carilionclinic.org' end WHERE [vchAthensOrgId] like '%67608560%' and len('67608560') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924240.va.gov' else '54924240.va.gov' end WHERE [vchAthensOrgId] like '%54924240%' and len('54924240') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54950379.va.gov' else '54950379.va.gov' end WHERE [vchAthensOrgId] like '%54950379%' and len('54950379') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849236.ahsl.arizona.edu' else '12849236.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12849236%' and len('12849236') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66234916.shell.com' else '66234916.shell.com' end WHERE [vchAthensOrgId] like '%66234916%' and len('66234916') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044135.eng.nhs.uk' else '4044135.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044135%' and len('4044135') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cengage.com' else 'cengage.com' end WHERE [vchAthensOrgId] like '%67626419%' and len('67626419') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cengage.co.uk' else 'cengage.co.uk' end WHERE [vchAthensOrgId] like '%1707290%' and len('1707290') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cengage.co.uk' else 'cengage.co.uk' end WHERE [vchAthensOrgId] like '%2792862%' and len('2792862') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',centrahealth.com' else 'centrahealth.com' end WHERE [vchAthensOrgId] like '%68238020%' and len('68238020') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924197.va.gov' else '54924197.va.gov' end WHERE [vchAthensOrgId] like '%54924197%' and len('54924197') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531216.eng.nhs.uk' else '5531216.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531216%' and len('5531216') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633428.va.gov' else '43633428.va.gov' end WHERE [vchAthensOrgId] like '%43633428%' and len('43633428') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cbsl.lk' else 'cbsl.lk' end WHERE [vchAthensOrgId] like '%69593261%' and len('69593261') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dunstable.ac.uk' else 'dunstable.ac.uk' end WHERE [vchAthensOrgId] like '%1787197%' and len('1787197') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',centralgippslandhealth.gov.au' else 'centralgippslandhealth.gov.au' end WHERE [vchAthensOrgId] like '%55177845%' and len('55177845') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5634904.eng.nhs.uk' else '5634904.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5634904%' and len('5634904') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044171.eng.nhs.uk' else '4044171.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044171%' and len('4044171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924243.va.gov' else '54924243.va.gov' end WHERE [vchAthensOrgId] like '%54924243%' and len('54924243') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924174.va.gov' else '54924174.va.gov' end WHERE [vchAthensOrgId] like '%54924174%' and len('54924174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',unipe.br' else 'unipe.br' end WHERE [vchAthensOrgId] like '%69989769%' and len('69989769') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cepol.europa.eu' else 'cepol.europa.eu' end WHERE [vchAthensOrgId] like '%67960507%' and len('67960507') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',certara.com' else 'certara.com' end WHERE [vchAthensOrgId] like '%70201460%' and len('70201460') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69954705.oaebsco.com' else '69954705.oaebsco.com' end WHERE [vchAthensOrgId] like '%69954705%' and len('69954705') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924195.va.gov' else '54924195.va.gov' end WHERE [vchAthensOrgId] like '%54924195%' and len('54924195') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ciob.org.uk' else 'ciob.org.uk' end WHERE [vchAthensOrgId] like '%15951356%' and len('15951356') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',chathamhouse.org' else 'chathamhouse.org' end WHERE [vchAthensOrgId] like '%66925289%' and len('66925289') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531205.eng.nhs.uk' else '5531205.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531205%' and len('5531205') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531204.eng.nhs.uk' else '5531204.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531204%' and len('5531204') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',east.chclc.org' else 'east.chclc.org' end WHERE [vchAthensOrgId] like '%70289591%' and len('70289591') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044154.eng.nhs.uk' else '4044154.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044154%' and len('4044154') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315942.eng.nhs.uk' else '4315942.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315942%' and len('4315942') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924245.va.gov' else '54924245.va.gov' end WHERE [vchAthensOrgId] like '%54924245%' and len('54924245') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523093.swanlibraries.net' else '69523093.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523093%' and len('69523093') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523094.swanlibraries.net' else '69523094.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523094%' and len('69523094') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',choa.org' else 'choa.org' end WHERE [vchAthensOrgId] like '%68923811%' and len('68923811') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cuh.ie' else 'cuh.ie' end WHERE [vchAthensOrgId] like '%48670230%' and len('48670230') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924213.va.gov' else '54924213.va.gov' end WHERE [vchAthensOrgId] like '%54924213%' and len('54924213') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12606376.ahsl.arizona.edu' else '12606376.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%67538537%' and len('67538537') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ctksfc.ac.uk' else 'ctksfc.ac.uk' end WHERE [vchAthensOrgId] like '%2625604%' and len('2625604') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ciap.health.nsw.gov.au' else 'ciap.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%70412752%' and len('70412752') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523095.swanlibraries.net' else '69523095.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523095%' and len('69523095') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924214.va.gov' else '54924214.va.gov' end WHERE [vchAthensOrgId] like '%54924214%' and len('54924214') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38216938.philips.com' else '38216938.philips.com' end WHERE [vchAthensOrgId] like '%38216938%' and len('38216938') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38216933.philips.com' else '38216933.philips.com' end WHERE [vchAthensOrgId] like '%38216933%' and len('38216933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38216919.philips.com' else '38216919.philips.com' end WHERE [vchAthensOrgId] like '%38216919%' and len('38216919') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ccn.ac.uk' else 'ccn.ac.uk' end WHERE [vchAthensOrgId] like '%1423560%' and len('1423560') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058627.eng.nhs.uk' else '4058627.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058627%' and len('4058627') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',citybathcoll.ac.uk' else 'citybathcoll.ac.uk' end WHERE [vchAthensOrgId] like '%1701128%' and len('1701128') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',city.ac.uk' else 'city.ac.uk' end WHERE [vchAthensOrgId] like '%141%' and len('141') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523096.swanlibraries.net' else '69523096.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523096%' and len('69523096') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69746690%' and len('69746690') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',clarksoncollege.edu' else 'clarksoncollege.edu' end WHERE [vchAthensOrgId] like '%70094625%' and len('70094625') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4113690.eng.nhs.uk' else '4113690.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4113690%' and len('4113690') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924229.va.gov' else '54924229.va.gov' end WHERE [vchAthensOrgId] like '%54924229%' and len('54924229') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ccad.ac.uk' else 'ccad.ac.uk' end WHERE [vchAthensOrgId] like '%1764877%' and len('1764877') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ckn.qldhealth.com' else 'ckn.qldhealth.com' end WHERE [vchAthensOrgId] like '%68176746%' and len('68176746') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',clinicalskills.net' else 'clinicalskills.net' end WHERE [vchAthensOrgId] like '%68009881%' and len('68009881') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69968933.oaebsco.com' else '69968933.oaebsco.com' end WHERE [vchAthensOrgId] like '%69968933%' and len('69968933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019639.communitylibrary.net' else '70019639.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019639%' and len('70019639') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',idp.cna.co.in' else 'idp.cna.co.in' end WHERE [vchAthensOrgId] like '%66785181%' and len('66785181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924186.va.gov' else '54924186.va.gov' end WHERE [vchAthensOrgId] like '%54924186%' and len('54924186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54589490.ahsl.arizona.edu' else '54589490.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%54589490%' and len('54589490') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019625.communitylibrary.net' else '70019625.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019625%' and len('70019625') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54520573.ncahs.health.nsw.gov.au' else '54520573.ncahs.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54520573%' and len('54520573') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073148.eng.nhs.uk' else '4073148.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073148%' and len('4073148') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',colchester.ac.uk' else 'colchester.ac.uk' end WHERE [vchAthensOrgId] like '%1770432%' and len('1770432') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cambria.ac.uk' else 'cambria.ac.uk' end WHERE [vchAthensOrgId] like '%2617892%' and len('2617892') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ceredigion.ac.uk' else 'ceredigion.ac.uk' end WHERE [vchAthensOrgId] like '%66735983%' and len('66735983') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cem.ac.uk' else 'cem.ac.uk' end WHERE [vchAthensOrgId] like '%7762054%' and len('7762054') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',college-optometrists.org' else 'college-optometrists.org' end WHERE [vchAthensOrgId] like '%12420455%' and len('12420455') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',npia.pnn.police.uk' else 'npia.pnn.police.uk' end WHERE [vchAthensOrgId] like '%7202798%' and len('7202798') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',collegiate-va.org' else 'collegiate-va.org' end WHERE [vchAthensOrgId] like '%70206056%' and len('70206056') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924216.va.gov' else '54924216.va.gov' end WHERE [vchAthensOrgId] like '%54924216%' and len('54924216') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',crh.org' else 'crh.org' end WHERE [vchAthensOrgId] like '%69362074%' and len('69362074') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195536.eng.nhs.uk' else '5195536.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195536%' and len('5195536') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762686.eng.nhs.uk' else '5762686.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762686%' and len('5762686') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762709.eng.nhs.uk' else '5762709.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762709%' and len('5762709') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762719.eng.nhs.uk' else '5762719.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762719%' and len('5762719') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5890389.eng.nhs.uk' else '5890389.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5890389%' and len('5890389') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315975.eng.nhs.uk' else '4315975.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315975%' and len('4315975') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762752.eng.nhs.uk' else '5762752.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762752%' and len('5762752') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223568.eng.nhs.uk' else '4223568.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223568%' and len('4223568') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112854.eng.nhs.uk' else '4112854.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112854%' and len('4112854') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762724.eng.nhs.uk' else '5762724.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762724%' and len('5762724') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904443.eng.nhs.uk' else '5904443.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904443%' and len('5904443') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762698.eng.nhs.uk' else '5762698.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762698%' and len('5762698') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67423658.eng.nhs.uk' else '67423658.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67423658%' and len('67423658') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762771.eng.nhs.uk' else '5762771.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762771%' and len('5762771') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415136.eng.nhs.uk' else '67415136.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415136%' and len('67415136') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112318.eng.nhs.uk' else '4112318.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112318%' and len('4112318') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ccsnh.edu' else 'ccsnh.edu' end WHERE [vchAthensOrgId] like '%70203743%' and len('70203743') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ccs.spokane.edu' else 'ccs.spokane.edu' end WHERE [vchAthensOrgId] like '%70211889%' and len('70211889') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',competition-commission.gsi.gov.uk' else 'competition-commission.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%7116800%' and len('7116800') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hgya.saludcastillayleon.es' else 'hgya.saludcastillayleon.es' end WHERE [vchAthensOrgId] like '%68056524%' and len('68056524') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hleo.saludcastillayleon.es' else 'hleo.saludcastillayleon.es' end WHERE [vchAthensOrgId] like '%68056532%' and len('68056532') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',husa.saludcastillayleon.es' else 'husa.saludcastillayleon.es' end WHERE [vchAthensOrgId] like '%68056536%' and len('68056536') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hvcn.saludcastillayleon.es' else 'hvcn.saludcastillayleon.es' end WHERE [vchAthensOrgId] like '%68056549%' and len('68056549') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',concordmh.nsw.gov.au' else 'concordmh.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54483679%' and len('54483679') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sswahs.nsw.gov.au' else 'sswahs.nsw.gov.au' end WHERE [vchAthensOrgId] like '%37602332%' and len('37602332') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',concytec.gob.pe' else 'concytec.gob.pe' end WHERE [vchAthensOrgId] like '%69374855%' and len('69374855') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',conacyt.gov.py' else 'conacyt.gov.py' end WHERE [vchAthensOrgId] like '%68707459%' and len('68707459') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67121026.gencat.cat' else '67121026.gencat.cat' end WHERE [vchAthensOrgId] like '%67121026%' and len('67121026') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514922.bibliosalut.com' else '54514922.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514922%' and len('54514922') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cdd.ac.uk' else 'cdd.ac.uk' end WHERE [vchAthensOrgId] like '%12660575%' and len('12660575') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',chguv.san.gva.es' else 'chguv.san.gva.es' end WHERE [vchAthensOrgId] like '%67481933%' and len('67481933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cshnyc.org' else 'cshnyc.org' end WHERE [vchAthensOrgId] like '%70106082%' and len('70106082') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',communitylibrary.net' else 'communitylibrary.net' end WHERE [vchAthensOrgId] like '%69730972%' and len('69730972') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66845136.sjog.ie' else '66845136.sjog.ie' end WHERE [vchAthensOrgId] like '%66845136%' and len('66845136') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',copyright.com' else 'copyright.com' end WHERE [vchAthensOrgId] like '%69956161%' and len('69956161') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69948852%' and len('69948852') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69948829%' and len('69948829') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762713.eng.nhs.uk' else '5762713.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762713%' and len('5762713') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118189.akzonobel.com' else '66118189.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118189%' and len('66118189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dcs.nsw.gov.au' else 'dcs.nsw.gov.au' end WHERE [vchAthensOrgId] like '%66021425%' and len('66021425') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044137.eng.nhs.uk' else '4044137.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044137%' and len('4044137') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058648.eng.nhs.uk' else '4058648.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058648%' and len('4058648') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',courtauld.ac.uk' else 'courtauld.ac.uk' end WHERE [vchAthensOrgId] like '%114345%' and len('114345') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',courtauld.ac.uk' else 'courtauld.ac.uk' end WHERE [vchAthensOrgId] like '%114345%' and len('114345') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',13432817.eng.nhs.uk' else '13432817.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%13432817%' and len('13432817') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',coventry.ac.uk' else 'coventry.ac.uk' end WHERE [vchAthensOrgId] like '%70148057%' and len('70148057') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cced.cranfield.ac.uk' else 'cced.cranfield.ac.uk' end WHERE [vchAthensOrgId] like '%5583307%' and len('5583307') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',craven-college.ac.uk' else 'craven-college.ac.uk' end WHERE [vchAthensOrgId] like '%12494876%' and len('12494876') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',credoreference.com' else 'credoreference.com' end WHERE [vchAthensOrgId] like '%1795076%' and len('1795076') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523097.swanlibraries.net' else '69523097.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523097%' and len('69523097') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523098.swanlibraries.net' else '69523098.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523098%' and len('69523098') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',crouse.org' else 'crouse.org' end WHERE [vchAthensOrgId] like '%48664771%' and len('48664771') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',croydon.ac.uk' else 'croydon.ac.uk' end WHERE [vchAthensOrgId] like '%1764264%' and len('1764264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223726.eng.nhs.uk' else '4223726.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223726%' and len('4223726') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68408523.ctksfc.ac.uk' else '68408523.ctksfc.ac.uk' end WHERE [vchAthensOrgId] like '%68408523%' and len('68408523') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044095.eng.nhs.uk' else '4044095.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044095%' and len('4044095') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70059764.oaebsco.com' else '70059764.oaebsco.com' end WHERE [vchAthensOrgId] like '%70059764%' and len('70059764') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7445849.wales.nhs.uk' else '7445849.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7445849%' and len('7445849') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7459034.wales.nhs.uk' else '7459034.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7459034%' and len('7459034') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cybermed.edu.my' else 'cybermed.edu.my' end WHERE [vchAthensOrgId] like '%68545381%' and len('68545381') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',daffodilvarsity.edu.bd' else 'daffodilvarsity.edu.bd' end WHERE [vchAthensOrgId] like '%67542980%' and len('67542980') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dsi.com' else 'dsi.com' end WHERE [vchAthensOrgId] like '%69745957%' and len('69745957') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',darlington.ac.uk' else 'darlington.ac.uk' end WHERE [vchAthensOrgId] like '%69706947%' and len('69706947') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223613.eng.nhs.uk' else '4223613.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223613%' and len('4223613') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3685151.sjog.ie' else '3685151.sjog.ie' end WHERE [vchAthensOrgId] like '%3685151%' and len('3685151') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69896116.oaebsco.com' else '69896116.oaebsco.com' end WHERE [vchAthensOrgId] like '%69896116%' and len('69896116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dawsonbooks.co.uk' else 'dawsonbooks.co.uk' end WHERE [vchAthensOrgId] like '%13466505%' and len('13466505') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924217.va.gov' else '54924217.va.gov' end WHERE [vchAthensOrgId] like '%54924217%' and len('54924217') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',law.ac.uk' else 'law.ac.uk' end WHERE [vchAthensOrgId] like '%70237431%' and len('70237431') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dlshsi.edu.ph' else 'dlshsi.edu.ph' end WHERE [vchAthensOrgId] like '%68694443%' and len('68694443') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dlsud.edu.ph' else 'dlsud.edu.ph' end WHERE [vchAthensOrgId] like '%69606352%' and len('69606352') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dmu.ac.uk' else 'dmu.ac.uk' end WHERE [vchAthensOrgId] like '%107%' and len('107') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118168.akzonobel.com' else '66118168.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118168%' and len('66118168') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',deepmind.com' else 'deepmind.com' end WHERE [vchAthensOrgId] like '%70006304%' and len('70006304') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',deeside.ac.uk' else 'deeside.ac.uk' end WHERE [vchAthensOrgId] like '%2617892%' and len('2617892') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dsto.defence.gov.au' else 'dsto.defence.gov.au' end WHERE [vchAthensOrgId] like '%68066426%' and len('68066426') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67478173.us.army.mil' else '67478173.us.army.mil' end WHERE [vchAthensOrgId] like '%67478173%' and len('67478173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tma.osd.mil' else 'tma.osd.mil' end WHERE [vchAthensOrgId] like '%66841250%' and len('66841250') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',state.de.us' else 'state.de.us' end WHERE [vchAthensOrgId] like '%69647222%' and len('69647222') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38557496.dhss.delaware.gov' else '38557496.dhss.delaware.gov' end WHERE [vchAthensOrgId] like '%38557496%' and len('38557496') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',delval.edu' else 'delval.edu' end WHERE [vchAthensOrgId] like '%70051888%' and len('70051888') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ej-gv.es' else 'ej-gv.es' end WHERE [vchAthensOrgId] like '%66889212%' and len('66889212') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bis.gsi.gov.uk' else 'bis.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%7316520%' and len('7316520') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dfc.sa.gov.au' else 'dfc.sa.gov.au' end WHERE [vchAthensOrgId] like '%66395171%' and len('66395171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',defra.gsi.gov.uk' else 'defra.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%5975201%' and len('5975201') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dh.gsi.gov.uk' else 'dh.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%7182364%' and len('7182364') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',13456979.eng.nhs.uk' else '13456979.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%13456979%' and len('13456979') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',55314370.hse.ie' else '55314370.hse.ie' end WHERE [vchAthensOrgId] like '%55314370%' and len('55314370') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38296233.va.gov' else '38296233.va.gov' end WHERE [vchAthensOrgId] like '%38296233%' and len('38296233') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315973.eng.nhs.uk' else '4315973.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315973%' and len('4315973') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315937.eng.nhs.uk' else '4315937.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315937%' and len('4315937') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315946.eng.nhs.uk' else '4315946.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315946%' and len('4315946') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',desertregional.com' else 'desertregional.com' end WHERE [vchAthensOrgId] like '%69447783%' and len('69447783') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dhpol.de' else 'dhpol.de' end WHERE [vchAthensOrgId] like '%70245100%' and len('70245100') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dzne.de' else 'dzne.de' end WHERE [vchAthensOrgId] like '%70202659%' and len('70202659') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762716.eng.nhs.uk' else '5762716.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762716%' and len('5762716') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',diakonsyk.no' else 'diakonsyk.no' end WHERE [vchAthensOrgId] like '%68678729%' and len('68678729') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620181.ahsl.arizona.edu' else '12620181.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620181%' and len('12620181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415147.eng.nhs.uk' else '67415147.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415147%' and len('67415147') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531175.eng.nhs.uk' else '5531175.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531203.eng.nhs.uk' else '5531203.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531183.eng.nhs.uk' else '5531183.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223730.eng.nhs.uk' else '4223730.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531212.eng.nhs.uk' else '5531212.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223731.eng.nhs.uk' else '4223731.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531184.eng.nhs.uk' else '5531184.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223733.eng.nhs.uk' else '4223733.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223713.eng.nhs.uk' else '4223713.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223735.eng.nhs.uk' else '4223735.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223732.eng.nhs.uk' else '4223732.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315960.eng.nhs.uk' else '4315960.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315960%' and len('4315960') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523099.swanlibraries.net' else '69523099.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523099%' and len('69523099') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315909.eng.nhs.uk' else '4315909.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315909%' and len('4315909') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',don.ac.uk' else 'don.ac.uk' end WHERE [vchAthensOrgId] like '%70004338%' and len('70004338') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762742.eng.nhs.uk' else '5762742.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762742%' and len('5762742') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762735.eng.nhs.uk' else '5762735.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762735%' and len('5762735') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523100.swanlibraries.net' else '69523100.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523100%' and len('69523100') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dsm.com' else 'dsm.com' end WHERE [vchAthensOrgId] like '%68242740%' and len('68242740') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dstl.gov.uk' else 'dstl.gov.uk' end WHERE [vchAthensOrgId] like '%6966682%' and len('6966682') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dewa.gov.ae' else 'dewa.gov.ae' end WHERE [vchAthensOrgId] like '%68015562%' and len('68015562') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dha.gov.ae' else 'dha.gov.ae' end WHERE [vchAthensOrgId] like '%38272071%' and len('38272071') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dental.tcd.ie' else 'dental.tcd.ie' end WHERE [vchAthensOrgId] like '%898906%' and len('898906') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54235923.eng.nhs.uk' else '54235923.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%54235923%' and len('54235923') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195484.eng.nhs.uk' else '5195484.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195484%' and len('5195484') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dudleycol.ac.uk' else 'dudleycol.ac.uk' end WHERE [vchAthensOrgId] like '%234%' and len('234') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195500.eng.nhs.uk' else '5195500.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195500%' and len('5195500') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dumgal.ac.uk' else 'dumgal.ac.uk' end WHERE [vchAthensOrgId] like '%1805318%' and len('1805318') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dpu.edu.tr' else 'dpu.edu.tr' end WHERE [vchAthensOrgId] like '%68627320%' and len('68627320') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iadt.ie' else 'iadt.ie' end WHERE [vchAthensOrgId] like '%54459859%' and len('54459859') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iadt.ie' else 'iadt.ie' end WHERE [vchAthensOrgId] like '%38537924%' and len('38537924') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dupont.com' else 'dupont.com' end WHERE [vchAthensOrgId] like '%69534765%' and len('69534765') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70208385.dupont.com' else '70208385.dupont.com' end WHERE [vchAthensOrgId] like '%70208385%' and len('70208385') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514898.va.gov' else '54514898.va.gov' end WHERE [vchAthensOrgId] like '%54514898%' and len('54514898') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531197.eng.nhs.uk' else '5531197.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531197%' and len('5531197') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wlc.ac.uk' else 'wlc.ac.uk' end WHERE [vchAthensOrgId] like '%1903475%' and len('1903475') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073183.eng.nhs.uk' else '4073183.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073183%' and len('4073183') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044144.eng.nhs.uk' else '4044144.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044144%' and len('4044144') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073224.eng.nhs.uk' else '4073224.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073224%' and len('4073224') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eastdurham.ac.uk' else 'eastdurham.ac.uk' end WHERE [vchAthensOrgId] like '%66174674%' and len('66174674') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223614.eng.nhs.uk' else '4223614.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223614%' and len('4223614') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044104.eng.nhs.uk' else '4044104.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044104%' and len('4044104') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531186.eng.nhs.uk' else '5531186.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531186%' and len('5531186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4310218.eng.nhs.uk' else '4310218.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4310218%' and len('4310218') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315954.eng.nhs.uk' else '4315954.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315954%' and len('4315954') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',enorf.ac.uk' else 'enorf.ac.uk' end WHERE [vchAthensOrgId] like '%2698464%' and len('2698464') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058911.eng.nhs.uk' else '4058911.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058911%' and len('4058911') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4374677.eng.nhs.uk' else '4374677.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4374677%' and len('4374677') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eastridingcollege.ac.uk' else 'eastridingcollege.ac.uk' end WHERE [vchAthensOrgId] like '%54638116%' and len('54638116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223615.eng.nhs.uk' else '4223615.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223615%' and len('4223615') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',etsu.edu' else 'etsu.edu' end WHERE [vchAthensOrgId] like '%70100212%' and len('70100212') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ewubd.edu' else 'ewubd.edu' end WHERE [vchAthensOrgId] like '%66145500%' and len('66145500') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',echn.org' else 'echn.org' end WHERE [vchAthensOrgId] like '%54363074%' and len('54363074') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',easternuni.edu.bd' else 'easternuni.edu.bd' end WHERE [vchAthensOrgId] like '%68256622%' and len('68256622') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eastleigh.ac.uk' else 'eastleigh.ac.uk' end WHERE [vchAthensOrgId] like '%1724458%' and len('1724458') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eastonotley.ac.uk' else 'eastonotley.ac.uk' end WHERE [vchAthensOrgId] like '%54507859%' and len('54507859') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70059766.oaebsco.com' else '70059766.oaebsco.com' end WHERE [vchAthensOrgId] like '%70059766%' and len('70059766') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ebrary.com' else 'ebrary.com' end WHERE [vchAthensOrgId] like '%6859971%' and len('6859971') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',idp.ebscohost.com' else 'idp.ebscohost.com' end WHERE [vchAthensOrgId] like '%882546%' and len('882546') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ebsco.com' else 'ebsco.com' end WHERE [vchAthensOrgId] like '%67377156%' and len('67377156') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66925191.ebsco.com' else '66925191.ebsco.com' end WHERE [vchAthensOrgId] like '%66925191%' and len('66925191') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eis.com' else 'eis.com' end WHERE [vchAthensOrgId] like '%68558569%' and len('68558569') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ebscoic.com' else 'ebscoic.com' end WHERE [vchAthensOrgId] like '%70152173%' and len('70152173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',azimpremjifoundation.org' else 'azimpremjifoundation.org' end WHERE [vchAthensOrgId] like '%66903114%' and len('66903114') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38536838.ebsco.com' else '38536838.ebsco.com' end WHERE [vchAthensOrgId] like '%38536838%' and len('38536838') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ebsco.org' else 'ebsco.org' end WHERE [vchAthensOrgId] like '%69986178%' and len('69986178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',oaebsco.com' else 'oaebsco.com' end WHERE [vchAthensOrgId] like '%69519194%' and len('69519194') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ebsco.co.uk' else 'ebsco.co.uk' end WHERE [vchAthensOrgId] like '%69986174%' and len('69986174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69973242.oaebsco.com' else '69973242.oaebsco.com' end WHERE [vchAthensOrgId] like '%69973242%' and len('69973242') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70143251.ebsco.com' else '70143251.ebsco.com' end WHERE [vchAthensOrgId] like '%70143251%' and len('70143251') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139937.dtf.vic.gov.au' else '68139937.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139937%' and len('68139937') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38537211.eng.nhs.uk' else '38537211.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66669751%' and len('66669751') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',euppublishing.com' else 'euppublishing.com' end WHERE [vchAthensOrgId] like '%70113308%' and len('70113308') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924170.va.gov' else '54924170.va.gov' end WHERE [vchAthensOrgId] like '%54924170%' and len('54924170') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69863869%' and len('69863869') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',e-elgar.co.uk' else 'e-elgar.co.uk' end WHERE [vchAthensOrgId] like '%67729420%' and len('67729420') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924226.va.gov' else '54924226.va.gov' end WHERE [vchAthensOrgId] like '%54924226%' and len('54924226') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597590.houstontx.gov' else '69597590.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597590%' and len('69597590') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523101.swanlibraries.net' else '69523101.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523101%' and len('69523101') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',colmich.edu.mx' else 'colmich.edu.mx' end WHERE [vchAthensOrgId] like '%69964996%' and len('69964996') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',epcc.edu' else 'epcc.edu' end WHERE [vchAthensOrgId] like '%70124912%' and len('70124912') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924239.va.gov' else '54924239.va.gov' end WHERE [vchAthensOrgId] like '%54924239%' and len('54924239') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69898231.oaebsco.com' else '69898231.oaebsco.com' end WHERE [vchAthensOrgId] like '%69898231%' and len('69898231') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68879788.swanlibraries.net' else '68879788.swanlibraries.net' end WHERE [vchAthensOrgId] like '%68879788%' and len('68879788') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70198047.elsevier.com' else '70198047.elsevier.com' end WHERE [vchAthensOrgId] like '%2424553%' and len('2424553') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',elsevierhealthscience.com' else 'elsevierhealthscience.com' end WHERE [vchAthensOrgId] like '%65475690%' and len('65475690') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',elsevierhealthscience.com' else 'elsevierhealthscience.com' end WHERE [vchAthensOrgId] like '%55225693%' and len('55225693') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp2.elsevier.com' else 'sp2.elsevier.com' end WHERE [vchAthensOrgId] like '%66241815%' and len('66241815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mosbysnursingskills.com' else 'mosbysnursingskills.com' end WHERE [vchAthensOrgId] like '%67239361%' and len('67239361') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12754932.elsevier.com' else '12754932.elsevier.com' end WHERE [vchAthensOrgId] like '%12754932%' and len('12754932') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5348902.elsevier.com' else '5348902.elsevier.com' end WHERE [vchAthensOrgId] like '%5348902%' and len('5348902') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',2424553.elsevier.com' else '2424553.elsevier.com' end WHERE [vchAthensOrgId] like '%2424553%' and len('2424553') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044353.elsevier.com' else '4044353.elsevier.com' end WHERE [vchAthensOrgId] like '%4044353%' and len('4044353') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',idp.elctest.org.uk' else 'idp.elctest.org.uk' end WHERE [vchAthensOrgId] like '%4034620%' and len('4034620') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp1.elsevier.com' else 'sp1.elsevier.com' end WHERE [vchAthensOrgId] like '%38270485%' and len('38270485') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',emeraldinsight.com' else 'emeraldinsight.com' end WHERE [vchAthensOrgId] like '%1047743%' and len('1047743') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',emiratesacademy.edu' else 'emiratesacademy.edu' end WHERE [vchAthensOrgId] like '%54551057%' and len('54551057') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eis2win.co.uk' else 'eis2win.co.uk' end WHERE [vchAthensOrgId] like '%8286934%' and len('8286934') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',environment-agency.gov.uk' else 'environment-agency.gov.uk' end WHERE [vchAthensOrgId] like '%2857965%' and len('2857965') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139933.dtf.vic.gov.au' else '68139933.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139933%' and len('68139933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139930.dtf.vic.gov.au' else '68139930.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139930%' and len('68139930') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223724.eng.nhs.uk' else '4223724.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223724%' and len('4223724') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70026451.oaebsco.com' else '70026451.oaebsco.com' end WHERE [vchAthensOrgId] like '%70026451%' and len('70026451') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69865026.oaebsco.com' else '69865026.oaebsco.com' end WHERE [vchAthensOrgId] like '%69865026%' and len('69865026') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924187.va.gov' else '54924187.va.gov' end WHERE [vchAthensOrgId] like '%54924187%' and len('54924187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bigsmoke.com' else 'bigsmoke.com' end WHERE [vchAthensOrgId] like '%965594%' and len('965594') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',esher.ac.uk' else 'esher.ac.uk' end WHERE [vchAthensOrgId] like '%1691540%' and len('1691540') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073153.eng.nhs.uk' else '4073153.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073153%' and len('4073153') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eurofound.europa.eu' else 'eurofound.europa.eu' end WHERE [vchAthensOrgId] like '%69991341%' and len('69991341') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',euromonitor.com' else 'euromonitor.com' end WHERE [vchAthensOrgId] like '%5534241%' and len('5534241') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',euromonitor.com' else 'euromonitor.com' end WHERE [vchAthensOrgId] like '%38445217%' and len('38445217') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ersj.org.uk' else 'ersj.org.uk' end WHERE [vchAthensOrgId] like '%68965410%' and len('68965410') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66987636.evans.amedd.army.mil' else '66987636.evans.amedd.army.mil' end WHERE [vchAthensOrgId] like '%66987636%' and len('66987636') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523102.swanlibraries.net' else '69523102.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523102%' and len('69523102') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',exlibrisgroup.com' else 'exlibrisgroup.com' end WHERE [vchAthensOrgId] like '%70020139%' and len('70020139') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',roche.ch' else 'roche.ch' end WHERE [vchAthensOrgId] like '%68655255%' and len('68655255') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',facets.org' else 'facets.org' end WHERE [vchAthensOrgId] like '%70150654%' and len('70150654') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fepar.edu.br' else 'fepar.edu.br' end WHERE [vchAthensOrgId] like '%70147677%' and len('70147677') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',faculti.net' else 'faculti.net' end WHERE [vchAthensOrgId] like '%70120750%' and len('70120750') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073175.eng.nhs.uk' else '4073175.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073175%' and len('4073175') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fairfield.nsw.gov.au' else 'fairfield.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54432892%' and len('54432892') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fareham.ac.uk' else 'fareham.ac.uk' end WHERE [vchAthensOrgId] like '%1658445%' and len('1658445') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924272.va.gov' else '54924272.va.gov' end WHERE [vchAthensOrgId] like '%54924272%' and len('54924272') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073172.eng.nhs.uk' else '4073172.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073172%' and len('4073172') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',farmington.k12.mi.us' else 'farmington.k12.mi.us' end WHERE [vchAthensOrgId] like '%70239936%' and len('70239936') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fchs.ac.ae' else 'fchs.ac.ae' end WHERE [vchAthensOrgId] like '%69498569%' and len('69498569') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',faulkner.edu' else 'faulkner.edu' end WHERE [vchAthensOrgId] like '%70331947%' and len('70331947') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633427.va.gov' else '43633427.va.gov' end WHERE [vchAthensOrgId] like '%43633427%' and len('43633427') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514897.va.gov' else '54514897.va.gov' end WHERE [vchAthensOrgId] like '%54514897%' and len('54514897') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ferring.com' else 'ferring.com' end WHERE [vchAthensOrgId] like '%7451495%' and len('7451495') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fife.gov.uk' else 'fife.gov.uk' end WHERE [vchAthensOrgId] like '%66875810%' and len('66875810') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ft.com' else 'ft.com' end WHERE [vchAthensOrgId] like '%67991139%' and len('67991139') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',idp2.ft.com' else 'idp2.ft.com' end WHERE [vchAthensOrgId] like '%68185058%' and len('68185058') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fireservicecollege.ac.uk' else 'fireservicecollege.ac.uk' end WHERE [vchAthensOrgId] like '%2723738%' and len('2723738') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',firstpurple.com' else 'firstpurple.com' end WHERE [vchAthensOrgId] like '%70237541%' and len('70237541') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',flhosp.org' else 'flhosp.org' end WHERE [vchAthensOrgId] like '%500%' and len('500') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523103.swanlibraries.net' else '69523103.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523103%' and len('69523103') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',csl.gov.uk' else 'csl.gov.uk' end WHERE [vchAthensOrgId] like '%233%' and len('233') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66329269.eng.nhs.uk' else '66329269.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66329269%' and len('66329269') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',fco.gov.uk' else 'fco.gov.uk' end WHERE [vchAthensOrgId] like '%66158091%' and len('66158091') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523104.swanlibraries.net' else '69523104.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523104%' and len('69523104') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',forestry.gsi.gov.uk' else 'forestry.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%68936835%' and len('68936835') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',forestry.gsi.gov.uk' else 'forestry.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%15952541%' and len('15952541') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',flb.fo' else 'flb.fo' end WHERE [vchAthensOrgId] like '%68179895%' and len('68179895') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',forthvalley.ac.uk' else 'forthvalley.ac.uk' end WHERE [vchAthensOrgId] like '%7912526%' and len('7912526') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523105.swanlibraries.net' else '69523105.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523105%' and len('69523105') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69656436.swanlibraries.net' else '69656436.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69656436%' and len('69656436') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223616.eng.nhs.uk' else '4223616.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223616%' and len('4223616') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118171.akzonobel.com' else '66118171.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118171%' and len('66118171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118172.akzonobel.com' else '66118172.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118172%' and len('66118172') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118173.akzonobel.com' else '66118173.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118173%' and len('66118173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118174.akzonobel.com' else '66118174.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118174%' and len('66118174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118177.akzonobel.com' else '66118177.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118177%' and len('66118177') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',barcelo.edu.ar' else 'barcelo.edu.ar' end WHERE [vchAthensOrgId] like '%70186846%' and len('70186846') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',futuremedicine.com' else 'futuremedicine.com' end WHERE [vchAthensOrgId] like '%15702246%' and len('15702246') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633426.va.gov' else '43633426.va.gov' end WHERE [vchAthensOrgId] like '%43633426%' and len('43633426') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cengage.co.uk' else 'cengage.co.uk' end WHERE [vchAthensOrgId] like '%68560468%' and len('68560468') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514916.bibliosalut.com' else '54514916.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514916%' and len('54514916') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',garnetvalley.org' else 'garnetvalley.org' end WHERE [vchAthensOrgId] like '%69834293%' and len('69834293') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gateshead.ac.uk' else 'gateshead.ac.uk' end WHERE [vchAthensOrgId] like '%2613215%' and len('2613215') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058628.eng.nhs.uk' else '4058628.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058628%' and len('4058628') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',medphys.genesiscare.com.au' else 'medphys.genesiscare.com.au' end WHERE [vchAthensOrgId] like '%68629761%' and len('68629761') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',genesiscare.com.au' else 'genesiscare.com.au' end WHERE [vchAthensOrgId] like '%67503787%' and len('67503787') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',geolsoc.org.uk' else 'geolsoc.org.uk' end WHERE [vchAthensOrgId] like '%15773120%' and len('15773120') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.geolsoc.org.uk' else 'sp.geolsoc.org.uk' end WHERE [vchAthensOrgId] like '%69383652%' and len('69383652') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thieme.de' else 'thieme.de' end WHERE [vchAthensOrgId] like '%67263357%' and len('67263357') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195521.eng.nhs.uk' else '5195521.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195521%' and len('5195521') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',georgetown.edu' else 'georgetown.edu' end WHERE [vchAthensOrgId] like '%69579368%' and len('69579368') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gatech.edu' else 'gatech.edu' end WHERE [vchAthensOrgId] like '%70210378%' and len('70210378') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',geoscienceworld.org' else 'geoscienceworld.org' end WHERE [vchAthensOrgId] like '%6914526%' and len('6914526') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514914.bibliosalut.com' else '54514914.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514914%' and len('54514914') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gilead.com' else 'gilead.com' end WHERE [vchAthensOrgId] like '%68167585%' and len('68167585') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',glasgowkelvin.ac.uk' else 'glasgowkelvin.ac.uk' end WHERE [vchAthensOrgId] like '%68154546%' and len('68154546') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523106.swanlibraries.net' else '69523106.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523106%' and len('69523106') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',progressivemediagroup.com' else 'progressivemediagroup.com' end WHERE [vchAthensOrgId] like '%69334631%' and len('69334631') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gcls.org' else 'gcls.org' end WHERE [vchAthensOrgId] like '%69699443%' and len('69699443') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762744.eng.nhs.uk' else '5762744.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762744%' and len('5762744') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762747.eng.nhs.uk' else '5762747.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762747%' and len('5762747') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',glyndwr.ac.uk' else 'glyndwr.ac.uk' end WHERE [vchAthensOrgId] like '%168%' and len('168') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',glyndwr.ac.uk' else 'glyndwr.ac.uk' end WHERE [vchAthensOrgId] like '%168%' and len('168') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173560.scot.nhs.uk' else '4173560.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173560%' and len('4173560') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',barnesjewishcollege.edu' else 'barnesjewishcollege.edu' end WHERE [vchAthensOrgId] like '%70233934%' and len('70233934') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',google.org' else 'google.org' end WHERE [vchAthensOrgId] like '%68065391%' and len('68065391') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',goshenhealth.com' else 'goshenhealth.com' end WHERE [vchAthensOrgId] like '%70067845%' and len('70067845') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',goulburnvalleyhealth.gov.au' else 'goulburnvalleyhealth.gov.au' end WHERE [vchAthensOrgId] like '%55177848%' and len('55177848') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762685.eng.nhs.uk' else '5762685.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762685%' and len('5762685') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223550.eng.nhs.uk' else '4223550.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223550%' and len('4223550') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112856.eng.nhs.uk' else '4112856.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112856%' and len('4112856') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762694.eng.nhs.uk' else '5762694.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762694%' and len('5762694') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762714.eng.nhs.uk' else '5762714.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762714%' and len('5762714') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762740.eng.nhs.uk' else '5762740.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762740%' and len('5762740') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223547.eng.nhs.uk' else '4223547.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223547%' and len('4223547') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762751.eng.nhs.uk' else '5762751.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762751%' and len('5762751') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044159.eng.nhs.uk' else '4044159.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044159%' and len('4044159') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223570.eng.nhs.uk' else '4223570.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223570%' and len('4223570') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044106.eng.nhs.uk' else '4044106.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044106%' and len('4044106') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223542.eng.nhs.uk' else '4223542.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223542%' and len('4223542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223545.eng.nhs.uk' else '4223545.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223545%' and len('4223545') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762723.eng.nhs.uk' else '5762723.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762723%' and len('5762723') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762764.eng.nhs.uk' else '5762764.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762764%' and len('5762764') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762738.eng.nhs.uk' else '5762738.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762738%' and len('5762738') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223546.eng.nhs.uk' else '4223546.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223546%' and len('4223546') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762774.eng.nhs.uk' else '5762774.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762774%' and len('5762774') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058680.eng.nhs.uk' else '4058680.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058680%' and len('4058680') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058690.eng.nhs.uk' else '4058690.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058690%' and len('4058690') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058655.eng.nhs.uk' else '4058655.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058655%' and len('4058655') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67798259.eng.nhs.uk' else '67798259.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67798259%' and len('67798259') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073157.eng.nhs.uk' else '4073157.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073157%' and len('4073157') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058640.eng.nhs.uk' else '4058640.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058640%' and len('4058640') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058683.eng.nhs.uk' else '4058683.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058683%' and len('4058683') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058646.eng.nhs.uk' else '4058646.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058646%' and len('4058646') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058711.eng.nhs.uk' else '4058711.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058711%' and len('4058711') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195535.eng.nhs.uk' else '5195535.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195535%' and len('5195535') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058714.eng.nhs.uk' else '4058714.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058714%' and len('4058714') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058636.eng.nhs.uk' else '4058636.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058636%' and len('4058636') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315923.eng.nhs.uk' else '4315923.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315923%' and len('4315923') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315930.eng.nhs.uk' else '4315930.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315930%' and len('4315930') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195549.eng.nhs.uk' else '5195549.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195549%' and len('5195549') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058661.eng.nhs.uk' else '4058661.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058661%' and len('4058661') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073180.eng.nhs.uk' else '4073180.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073180%' and len('4073180') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073205.eng.nhs.uk' else '4073205.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073205%' and len('4073205') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67419980.eng.nhs.uk' else '67419980.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67419980%' and len('67419980') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315938.eng.nhs.uk' else '4315938.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315938%' and len('4315938') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223647.eng.nhs.uk' else '4223647.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223647%' and len('4223647') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195478.eng.nhs.uk' else '5195478.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195478%' and len('5195478') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073190.eng.nhs.uk' else '4073190.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073190%' and len('4073190') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315881.eng.nhs.uk' else '4315881.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315881%' and len('4315881') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397777.eng.nhs.uk' else '67397777.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397777%' and len('67397777') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223658.eng.nhs.uk' else '4223658.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223658%' and len('4223658') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531188.eng.nhs.uk' else '5531188.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531188%' and len('5531188') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531156.eng.nhs.uk' else '5531156.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531156%' and len('5531156') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223717.eng.nhs.uk' else '4223717.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223717%' and len('4223717') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531218.eng.nhs.uk' else '5531218.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531218%' and len('5531218') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223718.eng.nhs.uk' else '4223718.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223718%' and len('4223718') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531157.eng.nhs.uk' else '5531157.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531157%' and len('5531157') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531225.eng.nhs.uk' else '5531225.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531225%' and len('5531225') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531189.eng.nhs.uk' else '5531189.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531189%' and len('5531189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223739.eng.nhs.uk' else '4223739.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223739%' and len('4223739') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531219.eng.nhs.uk' else '5531219.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531219%' and len('5531219') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531158.eng.nhs.uk' else '5531158.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531158%' and len('5531158') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223719.eng.nhs.uk' else '4223719.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223719%' and len('4223719') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531209.eng.nhs.uk' else '5531209.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531209%' and len('5531209') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531159.eng.nhs.uk' else '5531159.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531159%' and len('5531159') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531221.eng.nhs.uk' else '5531221.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531221%' and len('5531221') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531190.eng.nhs.uk' else '5531190.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531190%' and len('5531190') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531222.eng.nhs.uk' else '5531222.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531222%' and len('5531222') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531223.eng.nhs.uk' else '5531223.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531223%' and len('5531223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531169.eng.nhs.uk' else '5531169.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531169%' and len('5531169') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223740.eng.nhs.uk' else '4223740.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223740%' and len('4223740') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223720.eng.nhs.uk' else '4223720.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223720%' and len('4223720') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223721.eng.nhs.uk' else '4223721.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223721%' and len('4223721') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223734.eng.nhs.uk' else '4223734.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223734%' and len('4223734') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531191.eng.nhs.uk' else '5531191.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531191%' and len('5531191') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531193.eng.nhs.uk' else '5531193.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531193%' and len('5531193') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223741.eng.nhs.uk' else '4223741.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223741%' and len('4223741') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223722.eng.nhs.uk' else '4223722.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223722%' and len('4223722') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223742.eng.nhs.uk' else '4223742.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223742%' and len('4223742') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531192.eng.nhs.uk' else '5531192.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531192%' and len('5531192') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531194.eng.nhs.uk' else '5531194.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531194%' and len('5531194') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223743.eng.nhs.uk' else '4223743.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223743%' and len('4223743') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531213.eng.nhs.uk' else '5531213.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531213%' and len('5531213') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5339524.eng.nhs.uk' else '5339524.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5339524%' and len('5339524') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315895.eng.nhs.uk' else '4315895.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315895%' and len('4315895') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397792.eng.nhs.uk' else '67397792.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397792%' and len('67397792') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315970.eng.nhs.uk' else '4315970.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315970%' and len('4315970') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195561.eng.nhs.uk' else '5195561.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195561%' and len('5195561') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073209.eng.nhs.uk' else '4073209.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073209%' and len('4073209') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223661.eng.nhs.uk' else '4223661.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223661%' and len('4223661') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223628.eng.nhs.uk' else '4223628.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223628%' and len('4223628') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195564.eng.nhs.uk' else '5195564.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223649.eng.nhs.uk' else '4223649.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223649%' and len('4223649') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195472.eng.nhs.uk' else '5195472.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195472%' and len('5195472') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044089.eng.nhs.uk' else '4044089.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044089%' and len('4044089') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924247.va.gov' else '54924247.va.gov' end WHERE [vchAthensOrgId] like '%54924247%' and len('54924247') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523107.swanlibraries.net' else '69523107.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523107%' and len('69523107') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',grantham.ac.uk' else 'grantham.ac.uk' end WHERE [vchAthensOrgId] like '%2712564%' and len('2712564') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226675.ccsnh.edu' else '70226675.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226675%' and len('70226675') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531515.eng.nhs.uk' else '5531515.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531515%' and len('5531515') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762769.eng.nhs.uk' else '5762769.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762769%' and len('5762769') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gyc.ac.uk' else 'gyc.ac.uk' end WHERE [vchAthensOrgId] like '%2789936%' and len('2789936') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gbmc.org' else 'gbmc.org' end WHERE [vchAthensOrgId] like '%68141843%' and len('68141843') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044182.eng.nhs.uk' else '4044182.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044182%' and len('4044182') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38484817.ramsayhealth.com.au' else '38484817.ramsayhealth.com.au' end WHERE [vchAthensOrgId] like '%38484817%' and len('38484817') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ghs.org' else 'ghs.org' end WHERE [vchAthensOrgId] like '%65739306%' and len('65739306') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gcc.ac.uk' else 'gcc.ac.uk' end WHERE [vchAthensOrgId] like '%2669593%' and len('2669593') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',greenwichhospital.org' else 'greenwichhospital.org' end WHERE [vchAthensOrgId] like '%65581934%' and len('65581934') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',llandrillo.ac.uk' else 'llandrillo.ac.uk' end WHERE [vchAthensOrgId] like '%589313%' and len('589313') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gsm.org.uk' else 'gsm.org.uk' end WHERE [vchAthensOrgId] like '%5691052%' and len('5691052') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gzhmu.edu.cn' else 'gzhmu.edu.cn' end WHERE [vchAthensOrgId] like '%70143129%' and len('70143129') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',guildford.ac.uk' else 'guildford.ac.uk' end WHERE [vchAthensOrgId] like '%1763320%' and len('1763320') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gsmd.ac.uk' else 'gsmd.ac.uk' end WHERE [vchAthensOrgId] like '%12771504%' and len('12771504') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223702.eng.nhs.uk' else '4223702.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223702%' and len('4223702') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hackensackumc.org' else 'hackensackumc.org' end WHERE [vchAthensOrgId] like '%66810554%' and len('66810554') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hackney.ac.uk' else 'hackney.ac.uk' end WHERE [vchAthensOrgId] like '%2461805%' and len('2461805') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',haifa.muni.il' else 'haifa.muni.il' end WHERE [vchAthensOrgId] like '%68680118%' and len('68680118') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',halozyme.com' else 'halozyme.com' end WHERE [vchAthensOrgId] like '%70124943%' and len('70124943') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68843845.hmc.org.qa' else '68843845.hmc.org.qa' end WHERE [vchAthensOrgId] like '%68843845%' and len('68843845') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hmc.org.qa' else 'hmc.org.qa' end WHERE [vchAthensOrgId] like '%55322600%' and len('55322600') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223567.eng.nhs.uk' else '4223567.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223567%' and len('4223567') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38508092.va.gov' else '38508092.va.gov' end WHERE [vchAthensOrgId] like '%38508092%' and len('38508092') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hust.edu.vn' else 'hust.edu.vn' end WHERE [vchAthensOrgId] like '%70216537%' and len('70216537') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hmh.net' else 'hmh.net' end WHERE [vchAthensOrgId] like '%67860720%' and len('67860720') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',harper-adams.ac.uk' else 'harper-adams.ac.uk' end WHERE [vchAthensOrgId] like '%152%' and len('152') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',harper-adams.ac.uk' else 'harper-adams.ac.uk' end WHERE [vchAthensOrgId] like '%152%' and len('152') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058701.eng.nhs.uk' else '4058701.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058701%' and len('4058701') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924232.va.gov' else '54924232.va.gov' end WHERE [vchAthensOrgId] like '%54924232%' and len('54924232') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hartlepoolfe.ac.uk' else 'hartlepoolfe.ac.uk' end WHERE [vchAthensOrgId] like '%2367475%' and len('2367475') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',harvard.edu' else 'harvard.edu' end WHERE [vchAthensOrgId] like '%67854978%' and len('67854978') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523108.swanlibraries.net' else '69523108.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523108%' and len('69523108') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',havant.ac.uk' else 'havant.ac.uk' end WHERE [vchAthensOrgId] like '%2445359%' and len('2445359') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',havering-college.ac.uk' else 'havering-college.ac.uk' end WHERE [vchAthensOrgId] like '%4214452%' and len('4214452') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',havering-sfc.ac.uk' else 'havering-sfc.ac.uk' end WHERE [vchAthensOrgId] like '%2387703%' and len('2387703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139932.dtf.vic.gov.au' else '68139932.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139932%' and len('68139932') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4374676.eng.nhs.uk' else '4374676.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4374676%' and len('4374676') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp2.eng.nhs.uk' else 'sp2.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%70004647%' and len('70004647') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66329259.eng.nhs.uk' else '66329259.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66329259%' and len('66329259') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044186.eng.nhs.uk' else '4044186.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044186%' and len('4044186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762702.eng.nhs.uk' else '5762702.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762702%' and len('5762702') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195509.eng.nhs.uk' else '5195509.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195509%' and len('5195509') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5530913.eng.nhs.uk' else '5530913.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5530913%' and len('5530913') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223673.eng.nhs.uk' else '4223673.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223673%' and len('4223673') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531196.eng.nhs.uk' else '5531196.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531196%' and len('5531196') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058647.eng.nhs.uk' else '4058647.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058647%' and len('4058647') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5530887.eng.nhs.uk' else '5530887.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5530887%' and len('5530887') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223440.eng.nhs.uk' else '4223440.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223440%' and len('4223440') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058699.eng.nhs.uk' else '4058699.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058699%' and len('4058699') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',8256071.eng.nhs.uk' else '8256071.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%8256071%' and len('8256071') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12571371.hse.ie' else '12571371.hse.ie' end WHERE [vchAthensOrgId] like '%12571371%' and len('12571371') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hsa.edu.pk' else 'hsa.edu.pk' end WHERE [vchAthensOrgId] like '%69402952%' and len('69402952') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69878982.eng.nhs.uk' else '69878982.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69878982%' and len('69878982') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4176534.scot.nhs.uk' else '4176534.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4176534%' and len('4176534') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5894661.eng.nhs.uk' else '5894661.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5894661%' and len('5894661') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073219.eng.nhs.uk' else '4073219.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073219%' and len('4073219') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531165.eng.nhs.uk' else '5531165.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531165%' and len('5531165') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195497.eng.nhs.uk' else '5195497.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195497%' and len('5195497') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70037677.oaebsco.com' else '70037677.oaebsco.com' end WHERE [vchAthensOrgId] like '%70037677%' and len('70037677') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hca.ac.uk' else 'hca.ac.uk' end WHERE [vchAthensOrgId] like '%1957372%' and len('1957372') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hereford.ac.uk' else 'hereford.ac.uk' end WHERE [vchAthensOrgId] like '%2390323%' and len('2390323') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073186.eng.nhs.uk' else '4073186.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073186%' and len('4073186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073185.eng.nhs.uk' else '4073185.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073185%' and len('4073185') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058703.eng.nhs.uk' else '4058703.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058703%' and len('4058703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',minorityhealth.hhs.gov' else 'minorityhealth.hhs.gov' end WHERE [vchAthensOrgId] like '%66937410%' and len('66937410') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',highwire.stanford.edu' else 'highwire.stanford.edu' end WHERE [vchAthensOrgId] like '%5464311%' and len('5464311') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523109.swanlibraries.net' else '69523109.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523109%' and len('69523109') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523110.swanlibraries.net' else '69523110.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523110%' and len('69523110') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hippodigital.co.uk' else 'hippodigital.co.uk' end WHERE [vchAthensOrgId] like '%70246798%' and len('70246798') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%70185035%' and len('70185035') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523111.swanlibraries.net' else '69523111.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523111%' and len('69523111') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hig.se' else 'hig.se' end WHERE [vchAthensOrgId] like '%70246782%' and len('70246782') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38484818.ramsayhealth.com.au' else '38484818.ramsayhealth.com.au' end WHERE [vchAthensOrgId] like '%38484818%' and len('38484818') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',holyname.org' else 'holyname.org' end WHERE [vchAthensOrgId] like '%66743956%' and len('66743956') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531173.eng.nhs.uk' else '5531173.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531173%' and len('5531173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523112.swanlibraries.net' else '69523112.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523112%' and len('69523112') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849244.ahsl.arizona.edu' else '12849244.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12849244%' and len('12849244') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69539899.ahsl.arizona.edu' else '69539899.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%69539899%' and len('69539899') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620189.ahsl.arizona.edu' else '12620189.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620189%' and len('12620189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',horacegreeley.org' else 'horacegreeley.org' end WHERE [vchAthensOrgId] like '%70299702%' and len('70299702') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4233257.eng.nhs.uk' else '4233257.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4233257%' and len('4233257') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073207.eng.nhs.uk' else '4073207.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073207%' and len('4073207') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4233265.eng.nhs.uk' else '4233265.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4233265%' and len('4233265') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073220.eng.nhs.uk' else '4073220.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073220%' and len('4073220') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',huc.min-saude.pt' else 'huc.min-saude.pt' end WHERE [vchAthensOrgId] like '%69988538%' and len('69988538') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',oc.mde.es' else 'oc.mde.es' end WHERE [vchAthensOrgId] like '%68081438%' and len('68081438') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hcu.saludcastillayleon.es' else 'hcu.saludcastillayleon.es' end WHERE [vchAthensOrgId] like '%68056539%' and len('68056539') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514918.bibliosalut.com' else '54514918.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514918%' and len('54514918') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514917.bibliosalut.com' else '54514917.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514917%' and len('54514917') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',riojasalud.es' else 'riojasalud.es' end WHERE [vchAthensOrgId] like '%66120402%' and len('66120402') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hshs.org' else 'hshs.org' end WHERE [vchAthensOrgId] like '%68945117%' and len('68945117') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54392683.bibliosalut.com' else '54392683.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54392683%' and len('54392683') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gva.es' else 'gva.es' end WHERE [vchAthensOrgId] like '%67367055%' and len('67367055') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54339861.bibliosalut.com' else '54339861.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54339861%' and len('54339861') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hurh.saludcastillayleon.es' else 'hurh.saludcastillayleon.es' end WHERE [vchAthensOrgId] like '%68056545%' and len('68056545') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66181630.eng.nhs.uk' else '66181630.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66181630%' and len('66181630') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',parliament.uk' else 'parliament.uk' end WHERE [vchAthensOrgId] like '%12731654%' and len('12731654') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',houstontx.gov' else 'houstontx.gov' end WHERE [vchAthensOrgId] like '%69556834%' and len('69556834') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597591.houstontx.gov' else '69597591.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597591%' and len('69597591') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hrwallingford.com' else 'hrwallingford.com' end WHERE [vchAthensOrgId] like '%68711333%' and len('68711333') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12803728.hse.ie' else '12803728.hse.ie' end WHERE [vchAthensOrgId] like '%12803728%' and len('12803728') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12831794.hse.ie' else '12831794.hse.ie' end WHERE [vchAthensOrgId] like '%12831794%' and len('12831794') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12803731.hse.ie' else '12803731.hse.ie' end WHERE [vchAthensOrgId] like '%12803731%' and len('12803731') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12803732.hse.ie' else '12803732.hse.ie' end WHERE [vchAthensOrgId] like '%12803732%' and len('12803732') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12803733.hse.ie' else '12803733.hse.ie' end WHERE [vchAthensOrgId] like '%12803733%' and len('12803733') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12803730.hse.ie' else '12803730.hse.ie' end WHERE [vchAthensOrgId] like '%12803730%' and len('12803730') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12635441.hse.ie' else '12635441.hse.ie' end WHERE [vchAthensOrgId] like '%12635441%' and len('12635441') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12635454.hse.ie' else '12635454.hse.ie' end WHERE [vchAthensOrgId] like '%12635454%' and len('12635454') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68961845.hshs.org' else '68961845.hshs.org' end WHERE [vchAthensOrgId] like '%68961845%' and len('68961845') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68967050.hshs.org' else '68967050.hshs.org' end WHERE [vchAthensOrgId] like '%68967050%' and len('68967050') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38306288.va.gov' else '38306288.va.gov' end WHERE [vchAthensOrgId] like '%38306288%' and len('38306288') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hughbaird.ac.uk' else 'hughbaird.ac.uk' end WHERE [vchAthensOrgId] like '%3775511%' and len('3775511') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',huntingtonhospital.com' else 'huntingtonhospital.com' end WHERE [vchAthensOrgId] like '%69019352%' and len('69019352') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924207.va.gov' else '54924207.va.gov' end WHERE [vchAthensOrgId] like '%54924207%' and len('54924207') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597595.houstontx.gov' else '69597595.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597595%' and len('69597595') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hurleymc.com' else 'hurleymc.com' end WHERE [vchAthensOrgId] like '%67491552%' and len('67491552') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7445989.wales.nhs.uk' else '7445989.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7445989%' and len('7445989') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54514921.bibliosalut.com' else '54514921.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54514921%' and len('54514921') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ibisworld.com' else 'ibisworld.com' end WHERE [vchAthensOrgId] like '%69717683%' and len('69717683') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',icddrb.org' else 'icddrb.org' end WHERE [vchAthensOrgId] like '%60432046%' and len('60432046') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70195301.pfizer.com' else '70195301.pfizer.com' end WHERE [vchAthensOrgId] like '%70195301%' and len('70195301') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019663.communitylibrary.net' else '70019663.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019663%' and len('70019663') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019599.communitylibrary.net' else '70019599.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019599%' and len('70019599') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ieee.org' else 'ieee.org' end WHERE [vchAthensOrgId] like '%6946512%' and len('6946512') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iet.org' else 'iet.org' end WHERE [vchAthensOrgId] like '%68858871%' and len('68858871') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ihs.com' else 'ihs.com' end WHERE [vchAthensOrgId] like '%5855431%' and len('5855431') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iicp.ie' else 'iicp.ie' end WHERE [vchAthensOrgId] like '%67225170%' and len('67225170') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iindemo.com' else 'iindemo.com' end WHERE [vchAthensOrgId] like '%68008690%' and len('68008690') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iingroups.com' else 'iingroups.com' end WHERE [vchAthensOrgId] like '%38419651%' and len('38419651') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',redilumno.com' else 'redilumno.com' end WHERE [vchAthensOrgId] like '%70245048%' and len('70245048') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531198.eng.nhs.uk' else '5531198.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531198%' and len('5531198') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcnpublishing.co.uk' else 'rcnpublishing.co.uk' end WHERE [vchAthensOrgId] like '%69528059%' and len('69528059') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',inacap.cl' else 'inacap.cl' end WHERE [vchAthensOrgId] like '%70006147%' and len('70006147') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',independentcolleges.ie' else 'independentcolleges.ie' end WHERE [vchAthensOrgId] like '%43642209%' and len('43642209') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iub.edu.bd' else 'iub.edu.bd' end WHERE [vchAthensOrgId] like '%66158262%' and len('66158262') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iiml.ac.in' else 'iiml.ac.in' end WHERE [vchAthensOrgId] like '%67559124%' and len('67559124') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iimkashipur.ac.in' else 'iimkashipur.ac.in' end WHERE [vchAthensOrgId] like '%68919520%' and len('68919520') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iimu.ac.in' else 'iimu.ac.in' end WHERE [vchAthensOrgId] like '%67539943%' and len('67539943') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iisertvm.ac.in' else 'iisertvm.ac.in' end WHERE [vchAthensOrgId] like '%70094656%' and len('70094656') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523113.swanlibraries.net' else '69523113.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523113%' and len('69523113') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118179.akzonobel.com' else '66118179.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118179%' and len('66118179') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118181.akzonobel.com' else '66118181.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118181%' and len('66118181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118182.akzonobel.com' else '66118182.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118182%' and len('66118182') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118183.akzonobel.com' else '66118183.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118183%' and len('66118183') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',informa.com' else 'informa.com' end WHERE [vchAthensOrgId] like '%55123462%' and len('55123462') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',infotrieve.com' else 'infotrieve.com' end WHERE [vchAthensOrgId] like '%68195684%' and len('68195684') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',publishingtechnology.com' else 'publishingtechnology.com' end WHERE [vchAthensOrgId] like '%54878806%' and len('54878806') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',inova.com' else 'inova.com' end WHERE [vchAthensOrgId] like '%69542510%' and len('69542510') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gencat1.cat' else 'gencat1.cat' end WHERE [vchAthensOrgId] like '%68693455%' and len('68693455') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',actuaries.org.uk' else 'actuaries.org.uk' end WHERE [vchAthensOrgId] like '%38434973%' and len('38434973') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ifs.org.uk' else 'ifs.org.uk' end WHERE [vchAthensOrgId] like '%1087432%' and len('1087432') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iba.edu.pk' else 'iba.edu.pk' end WHERE [vchAthensOrgId] like '%66797159%' and len('66797159') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ids.ac.uk' else 'ids.ac.uk' end WHERE [vchAthensOrgId] like '%4279710%' and len('4279710') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iop.org' else 'iop.org' end WHERE [vchAthensOrgId] like '%67341392%' and len('67341392') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iop.com' else 'iop.com' end WHERE [vchAthensOrgId] like '%4165275%' and len('4165275') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ice.org.uk' else 'ice.org.uk' end WHERE [vchAthensOrgId] like '%69041001%' and len('69041001') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iner.salud.gob.mx' else 'iner.salud.gob.mx' end WHERE [vchAthensOrgId] like '%68726877%' and len('68726877') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ifactory.com' else 'ifactory.com' end WHERE [vchAthensOrgId] like '%15956713%' and len('15956713') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',icms.edu.au' else 'icms.edu.au' end WHERE [vchAthensOrgId] like '%66639494%' and len('66639494') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69726040%' and len('69726040') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',imu.edu.my' else 'imu.edu.my' end WHERE [vchAthensOrgId] like '%70389005%' and len('70389005') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',into.uk.com' else 'into.uk.com' end WHERE [vchAthensOrgId] like '%68011941%' and len('68011941') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iospress.nl' else 'iospress.nl' end WHERE [vchAthensOrgId] like '%69591306%' and len('69591306') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54950377.va.gov' else '54950377.va.gov' end WHERE [vchAthensOrgId] like '%54950377%' and len('54950377') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eis.com' else 'eis.com' end WHERE [vchAthensOrgId] like '%68609481%' and len('68609481') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073215.eng.nhs.uk' else '4073215.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073215%' and len('4073215') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hospice-foundation.ie' else 'hospice-foundation.ie' end WHERE [vchAthensOrgId] like '%66932838%' and len('66932838') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924227.va.gov' else '54924227.va.gov' end WHERE [vchAthensOrgId] like '%54924227%' and len('54924227') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67352137.iach.amedd.army.mil' else '67352137.iach.amedd.army.mil' end WHERE [vchAthensOrgId] like '%67352137%' and len('67352137') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',iut-dhaka.edu' else 'iut-dhaka.edu' end WHERE [vchAthensOrgId] like '%66650345%' and len('66650345') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223573.eng.nhs.uk' else '4223573.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223573%' and len('4223573') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',police.gov.il' else 'police.gov.il' end WHERE [vchAthensOrgId] like '%69642429%' and len('69642429') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dental.org.il' else 'dental.org.il' end WHERE [vchAthensOrgId] like '%67632405%' and len('67632405') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ithaka.org' else 'ithaka.org' end WHERE [vchAthensOrgId] like '%6992294%' and len('6992294') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66188667.iuhealth.org' else '66188667.iuhealth.org' end WHERE [vchAthensOrgId] like '%66188667%' and len('66188667') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633434.va.gov' else '43633434.va.gov' end WHERE [vchAthensOrgId] like '%43633434%' and len('43633434') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54407891.va.gov' else '54407891.va.gov' end WHERE [vchAthensOrgId] like '%54407891%' and len('54407891') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924183.va.gov' else '54924183.va.gov' end WHERE [vchAthensOrgId] like '%54924183%' and len('54924183') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',macaulay.ac.uk' else 'macaulay.ac.uk' end WHERE [vchAthensOrgId] like '%904316%' and len('904316') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',scri.ac.uk' else 'scri.ac.uk' end WHERE [vchAthensOrgId] like '%904299%' and len('904299') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jmu.edu' else 'jmu.edu' end WHERE [vchAthensOrgId] like '%70412774%' and len('70412774') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073227.eng.nhs.uk' else '4073227.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073227%' and len('4073227') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jmi.ac.in' else 'jmi.ac.in' end WHERE [vchAthensOrgId] like '%68020877%' and len('68020877') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jnu.ac.in' else 'jnu.ac.in' end WHERE [vchAthensOrgId] like '%68589014%' and len('68589014') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jchs.edu' else 'jchs.edu' end WHERE [vchAthensOrgId] like '%67608561%' and len('67608561') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jamd.ac.il' else 'jamd.ac.il' end WHERE [vchAthensOrgId] like '%38132047%' and len('38132047') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924225.va.gov' else '54924225.va.gov' end WHERE [vchAthensOrgId] like '%54924225%' and len('54924225') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70026460.oaebsco.com' else '70026460.oaebsco.com' end WHERE [vchAthensOrgId] like '%70026460%' and len('70026460') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924221.va.gov' else '54924221.va.gov' end WHERE [vchAthensOrgId] like '%54924221%' and len('54924221') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54950353.va.gov' else '54950353.va.gov' end WHERE [vchAthensOrgId] like '%54950353%' and len('54950353') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',johnruskin.ac.uk' else 'johnruskin.ac.uk' end WHERE [vchAthensOrgId] like '%43644573%' and len('43644573') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',johnruskin.ac.uk' else 'johnruskin.ac.uk' end WHERE [vchAthensOrgId] like '%1759758%' and len('1759758') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wiley.com' else 'wiley.com' end WHERE [vchAthensOrgId] like '%3013472%' and len('3013472') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wiley-blackwell.com' else 'wiley-blackwell.com' end WHERE [vchAthensOrgId] like '%3803208%' and len('3803208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69694061.med.navy.mil' else '69694061.med.navy.mil' end WHERE [vchAthensOrgId] like '%69694061%' and len('69694061') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',da.mod.uk' else 'da.mod.uk' end WHERE [vchAthensOrgId] like '%69380609%' and len('69380609') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924257.va.gov' else '54924257.va.gov' end WHERE [vchAthensOrgId] like '%54924257%' and len('54924257') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',15894437.ramsayhealth.com.au' else '15894437.ramsayhealth.com.au' end WHERE [vchAthensOrgId] like '%15894437%' and len('15894437') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thejns.org' else 'thejns.org' end WHERE [vchAthensOrgId] like '%68473931%' and len('68473931') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jospt.org' else 'jospt.org' end WHERE [vchAthensOrgId] like '%66870469%' and len('66870469') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',idp.jove.com' else 'idp.jove.com' end WHERE [vchAthensOrgId] like '%66925336%' and len('66925336') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139936.dtf.vic.gov.au' else '68139936.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139936%' and len('68139936') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523114.swanlibraries.net' else '69523114.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523114%' and len('69523114') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',justis.com' else 'justis.com' end WHERE [vchAthensOrgId] like '%1084684%' and len('1084684') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924234.va.gov' else '54924234.va.gov' end WHERE [vchAthensOrgId] like '%54924234%' and len('54924234') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',kaplan.edu.au' else 'kaplan.edu.au' end WHERE [vchAthensOrgId] like '%68511264%' and len('68511264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',kensingtoncoll.ac.uk' else 'kensingtoncoll.ac.uk' end WHERE [vchAthensOrgId] like '%67260739%' and len('67260739') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223634.eng.nhs.uk' else '4223634.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223634%' and len('4223634') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223637.eng.nhs.uk' else '4223637.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223637%' and len('4223637') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315887.eng.nhs.uk' else '4315887.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315887%' and len('4315887') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',keynote.co.uk' else 'keynote.co.uk' end WHERE [vchAthensOrgId] like '%3578926%' and len('3578926') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',keytravel.com' else 'keytravel.com' end WHERE [vchAthensOrgId] like '%69449648%' and len('69449648') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',kacst.edu.sa' else 'kacst.edu.sa' end WHERE [vchAthensOrgId] like '%54465542%' and len('54465542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223703.eng.nhs.uk' else '4223703.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223703%' and len('4223703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7269855.eng.nhs.uk' else '7269855.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%7269855%' and len('7269855') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223725.eng.nhs.uk' else '4223725.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223725%' and len('4223725') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',kmc.ac.uk' else 'kmc.ac.uk' end WHERE [vchAthensOrgId] like '%2699087%' and len('2699087') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',kluwerlaw.com' else 'kluwerlaw.com' end WHERE [vchAthensOrgId] like '%68951635%' and len('68951635') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38505843.knovel.com' else '38505843.knovel.com' end WHERE [vchAthensOrgId] like '%38505843%' and len('38505843') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',knovel.com' else 'knovel.com' end WHERE [vchAthensOrgId] like '%38392376%' and len('38392376') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',knowledgee.com' else 'knowledgee.com' end WHERE [vchAthensOrgId] like '%69677801%' and len('69677801') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',kotar-rishon-lezion.org.il' else 'kotar-rishon-lezion.org.il' end WHERE [vchAthensOrgId] like '%70224819%' and len('70224819') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54333409.bibliosalut.com' else '54333409.bibliosalut.com' end WHERE [vchAthensOrgId] like '%54530842%' and len('54530842') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',virtuskuwait.com' else 'virtuskuwait.com' end WHERE [vchAthensOrgId] like '%67512385%' and len('67512385') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523115.swanlibraries.net' else '69523115.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523115%' and len('69523115') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523116.swanlibraries.net' else '69523116.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523116%' and len('69523116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lahey.org' else 'lahey.org' end WHERE [vchAthensOrgId] like '%70415481%' and len('70415481') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lakehealth.org' else 'lakehealth.org' end WHERE [vchAthensOrgId] like '%67976911%' and len('67976911') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lcwc.ac.uk' else 'lcwc.ac.uk' end WHERE [vchAthensOrgId] like '%2375135%' and len('2375135') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226678.ccsnh.edu' else '70226678.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226678%' and len('70226678') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044110.eng.nhs.uk' else '4044110.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044110%' and len('4044110') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112312.eng.nhs.uk' else '4112312.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112312%' and len('4112312') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044109.eng.nhs.uk' else '4044109.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044109%' and len('4044109') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lancasterseminary.edu' else 'lancasterseminary.edu' end WHERE [vchAthensOrgId] like '%69954008%' and len('69954008') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597596.houstontx.gov' else '69597596.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597596%' and len('69597596') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lvn.se' else 'lvn.se' end WHERE [vchAthensOrgId] like '%70110706%' and len('70110706') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69656394.swanlibraries.net' else '69656394.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69656394%' and len('69656394') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',laredo.edu' else 'laredo.edu' end WHERE [vchAthensOrgId] like '%70004444%' and len('70004444') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ularkin.org' else 'ularkin.org' end WHERE [vchAthensOrgId] like '%70217125%' and len('70217125') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lrh.com.au' else 'lrh.com.au' end WHERE [vchAthensOrgId] like '%70252279%' and len('70252279') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bufvc.ac.uk' else 'bufvc.ac.uk' end WHERE [vchAthensOrgId] like '%4302654%' and len('4302654') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924188.va.gov' else '54924188.va.gov' end WHERE [vchAthensOrgId] like '%54924188%' and len('54924188') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058675.eng.nhs.uk' else '4058675.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058675%' and len('4058675') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leeds-art.ac.uk' else 'leeds-art.ac.uk' end WHERE [vchAthensOrgId] like '%898742%' and len('898742') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leedsmet.ac.uk' else 'leedsmet.ac.uk' end WHERE [vchAthensOrgId] like '%160%' and len('160') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66169401.eng.nhs.uk' else '66169401.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66169401%' and len('66169401') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058676.eng.nhs.uk' else '4058676.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058676%' and len('4058676') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leedstrinity.ac.uk' else 'leedstrinity.ac.uk' end WHERE [vchAthensOrgId] like '%136490%' and len('136490') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leememorial.org' else 'leememorial.org' end WHERE [vchAthensOrgId] like '%66146852%' and len('66146852') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315888.eng.nhs.uk' else '4315888.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315888%' and len('4315888') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leo-pharma.com' else 'leo-pharma.com' end WHERE [vchAthensOrgId] like '%69593165%' and len('69593165') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223705.eng.nhs.uk' else '4223705.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223705%' and len('4223705') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924208.va.gov' else '54924208.va.gov' end WHERE [vchAthensOrgId] like '%54924208%' and len('54924208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%7467571%' and len('7467571') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%12556623%' and len('12556623') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%12661674%' and len('12661674') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%12661676%' and len('12661676') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%1437477%' and len('1437477') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%7467452%' and len('7467452') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens.idp.shib.lexisnexis.co.uk' else 'athens.idp.shib.lexisnexis.co.uk' end WHERE [vchAthensOrgId] like '%1070859%' and len('1070859') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leyton.ac.uk' else 'leyton.ac.uk' end WHERE [vchAthensOrgId] like '%1797815%' and len('1797815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019584.communitylibrary.net' else '70019584.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019584%' and len('70019584') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lighting.com' else 'lighting.com' end WHERE [vchAthensOrgId] like '%69971234%' and len('69971234') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315956.eng.nhs.uk' else '4315956.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315956%' and len('4315956') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315955.eng.nhs.uk' else '4315955.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315955%' and len('4315955') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lipscomb.edu' else 'lipscomb.edu' end WHERE [vchAthensOrgId] like '%70190318%' and len('70190318') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54520576.ncahs.health.nsw.gov.au' else '54520576.ncahs.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54520576%' and len('54520576') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gardners.com' else 'gardners.com' end WHERE [vchAthensOrgId] like '%67131786%' and len('67131786') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044187.eng.nhs.uk' else '4044187.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044187%' and len('4044187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044122.eng.nhs.uk' else '4044122.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044122%' and len('4044122') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',liverpool.nsw.gov.au' else 'liverpool.nsw.gov.au' end WHERE [vchAthensOrgId] like '%48701831%' and len('48701831') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lipa.ac.uk' else 'lipa.ac.uk' end WHERE [vchAthensOrgId] like '%136468%' and len('136468') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ljmu.ac.uk' else 'ljmu.ac.uk' end WHERE [vchAthensOrgId] like '%203%' and len('203') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4160571.eng.nhs.uk' else '4160571.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4160571%' and len('4160571') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',liverpooluniversitypress.co.uk' else 'liverpooluniversitypress.co.uk' end WHERE [vchAthensOrgId] like '%69996718%' and len('69996718') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044128.eng.nhs.uk' else '4044128.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044128%' and len('4044128') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762726.eng.nhs.uk' else '5762726.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762726%' and len('5762726') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67418604.eng.nhs.uk' else '67418604.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67418604%' and len('67418604') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',60431785.logicalimages.com' else '60431785.logicalimages.com' end WHERE [vchAthensOrgId] like '%60431785%' and len('60431785') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5526937.eng.nhs.uk' else '5526937.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5526937%' and len('5526937') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5610890.eng.nhs.uk' else '5610890.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5610890%' and len('5610890') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5610878.eng.nhs.uk' else '5610878.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5610878%' and len('5610878') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',london.edu' else 'london.edu' end WHERE [vchAthensOrgId] like '%137688%' and len('137688') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531200.eng.nhs.uk' else '5531200.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531200%' and len('5531200') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531201.eng.nhs.uk' else '5531201.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531201%' and len('5531201') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lsec.ac.uk' else 'lsec.ac.uk' end WHERE [vchAthensOrgId] like '%70264303%' and len('70264303') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',longleypark.ac.uk' else 'longleypark.ac.uk' end WHERE [vchAthensOrgId] like '%12490375%' and len('12490375') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67584946.eng.nhs.uk' else '67584946.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67584946%' and len('67584946') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ph.lacounty.gov' else 'ph.lacounty.gov' end WHERE [vchAthensOrgId] like '%48663208%' and len('48663208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lboro.ac.uk' else 'lboro.ac.uk' end WHERE [vchAthensOrgId] like '%163%' and len('163') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924185.va.gov' else '54924185.va.gov' end WHERE [vchAthensOrgId] like '%54924185%' and len('54924185') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924215.va.gov' else '54924215.va.gov' end WHERE [vchAthensOrgId] like '%54924215%' and len('54924215') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924209.va.gov' else '54924209.va.gov' end WHERE [vchAthensOrgId] like '%54924209%' and len('54924209') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69427106.ctksfc.ac.uk' else '69427106.ctksfc.ac.uk' end WHERE [vchAthensOrgId] like '%69427106%' and len('69427106') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849238.ahsl.arizona.edu' else '12849238.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12849238%' and len('12849238') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lbs.edu' else 'lbs.edu' end WHERE [vchAthensOrgId] like '%69656757%' and len('69656757') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073178.eng.nhs.uk' else '4073178.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073178%' and len('4073178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073182.eng.nhs.uk' else '4073182.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073182%' and len('4073182') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lutonsfc.ac.uk' else 'lutonsfc.ac.uk' end WHERE [vchAthensOrgId] like '%1808201%' and len('1808201') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523117.swanlibraries.net' else '69523117.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523117%' and len('69523117') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7209469.markallengroup.com' else '7209469.markallengroup.com' end WHERE [vchAthensOrgId] like '%7209469%' and len('7209469') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',macalester.edu' else 'macalester.edu' end WHERE [vchAthensOrgId] like '%70262485%' and len('70262485') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68928465.ckn.qldhealth.com' else '68928465.ckn.qldhealth.com' end WHERE [vchAthensOrgId] like '%68928465%' and len('68928465') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5246208.eng.nhs.uk' else '5246208.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5246208%' and len('5246208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70340402.oaebsco.com' else '70340402.oaebsco.com' end WHERE [vchAthensOrgId] like '%70340402%' and len('70340402') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mahidol.ac.th' else 'mahidol.ac.th' end WHERE [vchAthensOrgId] like '%70157041%' and len('70157041') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223618.eng.nhs.uk' else '4223618.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223618%' and len('4223618') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',meca.edu' else 'meca.edu' end WHERE [vchAthensOrgId] like '%69579530%' and len('69579530') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.eng.nhs.uk' else 'sp.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69962500%' and len('69962500') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',1407223.eng.nhs.uk' else '1407223.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67460952%' and len('67460952') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',msn.com' else 'msn.com' end WHERE [vchAthensOrgId] like '%68232501%' and len('68232501') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',msu.edu.my' else 'msu.edu.my' end WHERE [vchAthensOrgId] like '%69507096%' and len('69507096') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',manasquanschools.org' else 'manasquanschools.org' end WHERE [vchAthensOrgId] like '%70011362%' and len('70011362') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226679.ccsnh.edu' else '70226679.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226679%' and len('70226679') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112324.eng.nhs.uk' else '4112324.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112324%' and len('4112324') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924173.va.gov' else '54924173.va.gov' end WHERE [vchAthensOrgId] like '%54924173%' and len('54924173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mut.ac.za' else 'mut.ac.za' end WHERE [vchAthensOrgId] like '%68721391%' and len('68721391') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226361.oaebsco.com' else '70226361.oaebsco.com' end WHERE [vchAthensOrgId] like '%70226361%' and len('70226361') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',marian.edu' else 'marian.edu' end WHERE [vchAthensOrgId] like '%68261155%' and len('68261155') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620192.ahsl.arizona.edu' else '12620192.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620192%' and len('12620192') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',55325748.eng.nhs.uk' else '55325748.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%55325748%' and len('55325748') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69722852.eng.nhs.uk' else '69722852.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69722852%' and len('69722852') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ma.org' else 'ma.org' end WHERE [vchAthensOrgId] like '%70077406%' and len('70077406') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118184.akzonobel.com' else '66118184.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118184%' and len('66118184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',scotland.gsi.gov.uk' else 'scotland.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%15979958%' and len('15979958') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597592.houstontx.gov' else '69597592.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597592%' and len('69597592') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54950352.va.gov' else '54950352.va.gov' end WHERE [vchAthensOrgId] like '%54950352%' and len('54950352') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68794678.markallengroup.com' else '68794678.markallengroup.com' end WHERE [vchAthensOrgId] like '%68794678%' and len('68794678') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523118.swanlibraries.net' else '69523118.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523118%' and len('69523118') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43660214.va.gov' else '43660214.va.gov' end WHERE [vchAthensOrgId] like '%43660214%' and len('43660214') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',masan.ac.kr' else 'masan.ac.kr' end WHERE [vchAthensOrgId] like '%70196056%' and len('70196056') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69866198.nejm.org' else '69866198.nejm.org' end WHERE [vchAthensOrgId] like '%69866198%' and len('69866198') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nejm.org' else 'nejm.org' end WHERE [vchAthensOrgId] like '%7093755%' and len('7093755') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mater.ie' else 'mater.ie' end WHERE [vchAthensOrgId] like '%6717576%' and len('6717576') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mathematica-mpr.com' else 'mathematica-mpr.com' end WHERE [vchAthensOrgId] like '%70109876%' and len('70109876') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523119.swanlibraries.net' else '69523119.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523119%' and len('69523119') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mamc.ac.in' else 'mamc.ac.in' end WHERE [vchAthensOrgId] like '%69224034%' and len('69224034') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69995071.oaebsco.com' else '69995071.oaebsco.com' end WHERE [vchAthensOrgId] like '%69995071%' and len('69995071') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523120.swanlibraries.net' else '69523120.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523120%' and len('69523120') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mccallie.org' else 'mccallie.org' end WHERE [vchAthensOrgId] like '%69975652%' and len('69975652') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523121.swanlibraries.net' else '69523121.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523121%' and len('69523121') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',milliman.com' else 'milliman.com' end WHERE [vchAthensOrgId] like '%66913752%' and len('66913752') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mcgraw-hill.com' else 'mcgraw-hill.com' end WHERE [vchAthensOrgId] like '%65497401%' and len('65497401') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mclean.harvard.edu' else 'mclean.harvard.edu' end WHERE [vchAthensOrgId] like '%67958620%' and len('67958620') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',medhand.com' else 'medhand.com' end WHERE [vchAthensOrgId] like '%70121755%' and len('70121755') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70278329.medhand.com' else '70278329.medhand.com' end WHERE [vchAthensOrgId] like '%70278329%' and len('70278329') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69963856.ps.openathens.net' else '69963856.ps.openathens.net' end WHERE [vchAthensOrgId] like '%69963856%' and len('69963856') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%208%' and len('208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70099561.oaebsco.com' else '70099561.oaebsco.com' end WHERE [vchAthensOrgId] like '%70099561%' and len('70099561') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66298438.eng.nhs.uk' else '66298438.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66298438%' and len('66298438') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54528662.medstar.net' else '54528662.medstar.net' end WHERE [vchAthensOrgId] like '%54528662%' and len('54528662') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53834600.medstar.net' else '53834600.medstar.net' end WHERE [vchAthensOrgId] like '%53834600%' and len('53834600') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',48664370.medstar.net' else '48664370.medstar.net' end WHERE [vchAthensOrgId] like '%48664370%' and len('48664370') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',60416005.medstar.net' else '60416005.medstar.net' end WHERE [vchAthensOrgId] like '%60416005%' and len('60416005') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223622.eng.nhs.uk' else '4223622.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223622%' and len('4223622') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223619.eng.nhs.uk' else '4223619.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223619%' and len('4223619') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eis.com' else 'eis.com' end WHERE [vchAthensOrgId] like '%68609480%' and len('68609480') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523122.swanlibraries.net' else '69523122.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523122%' and len('69523122') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',musowls.org' else 'musowls.org' end WHERE [vchAthensOrgId] like '%68957526%' and len('68957526') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924210.va.gov' else '54924210.va.gov' end WHERE [vchAthensOrgId] like '%54924210%' and len('54924210') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3636760.sjog.ie' else '3636760.sjog.ie' end WHERE [vchAthensOrgId] like '%69294178%' and len('69294178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69032262.wslhd.health.nsw.gov.au' else '69032262.wslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%69032262%' and len('69032262') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercy.net' else 'mercy.net' end WHERE [vchAthensOrgId] like '%66203442%' and len('66203442') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercyhealth.org' else 'mercyhealth.org' end WHERE [vchAthensOrgId] like '%68275685%' and len('68275685') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercyhealth1.org' else 'mercyhealth1.org' end WHERE [vchAthensOrgId] like '%68680105%' and len('68680105') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercy.net' else 'mercy.net' end WHERE [vchAthensOrgId] like '%66656542%' and len('66656542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',merial.com' else 'merial.com' end WHERE [vchAthensOrgId] like '%69988501%' and len('69988501') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044117.eng.nhs.uk' else '4044117.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044117%' and len('4044117') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',methodistcol.edu' else 'methodistcol.edu' end WHERE [vchAthensOrgId] like '%70086875%' and len('70086875') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924203.va.gov' else '54924203.va.gov' end WHERE [vchAthensOrgId] like '%54924203%' and len('54924203') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70026462.oaebsco.com' else '70026462.oaebsco.com' end WHERE [vchAthensOrgId] like '%70026462%' and len('70026462') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633429.va.gov' else '43633429.va.gov' end WHERE [vchAthensOrgId] like '%43633429%' and len('43633429') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',microform.co.uk' else 'microform.co.uk' end WHERE [vchAthensOrgId] like '%66716767%' and len('66716767') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',microsoft.com' else 'microsoft.com' end WHERE [vchAthensOrgId] like '%70283356%' and len('70283356') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044146.eng.nhs.uk' else '4044146.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044146%' and len('4044146') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073150.eng.nhs.uk' else '4073150.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073150%' and len('4073150') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195545.eng.nhs.uk' else '5195545.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195545%' and len('5195545') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058677.eng.nhs.uk' else '4058677.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058677%' and len('4058677') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mbro.ac.uk' else 'mbro.ac.uk' end WHERE [vchAthensOrgId] like '%1768248%' and len('1768248') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mdx.ac.uk' else 'mdx.ac.uk' end WHERE [vchAthensOrgId] like '%164%' and len('164') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523123.swanlibraries.net' else '69523123.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523123%' and len('69523123') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',midirs.org' else 'midirs.org' end WHERE [vchAthensOrgId] like '%68201828%' and len('68201828') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',millersville.edu' else 'millersville.edu' end WHERE [vchAthensOrgId] like '%70050428%' and len('70050428') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mkcollege.ac.uk' else 'mkcollege.ac.uk' end WHERE [vchAthensOrgId] like '%54607683%' and len('54607683') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223521.eng.nhs.uk' else '4223521.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223521%' and len('4223521') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54723861.mims.com.au' else '54723861.mims.com.au' end WHERE [vchAthensOrgId] like '%54723861%' and len('54723861') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ontario.ministry.children.ca' else 'ontario.ministry.children.ca' end WHERE [vchAthensOrgId] like '%67470959%' and len('67470959') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mod.uk' else 'mod.uk' end WHERE [vchAthensOrgId] like '%6832944%' and len('6832944') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',moe.gov.sg' else 'moe.gov.sg' end WHERE [vchAthensOrgId] like '%68815595%' and len('68815595') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',moh.gov.om' else 'moh.gov.om' end WHERE [vchAthensOrgId] like '%68151907%' and len('68151907') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924273.va.gov' else '54924273.va.gov' end WHERE [vchAthensOrgId] like '%54924273%' and len('54924273') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',msstate.edu' else 'msstate.edu' end WHERE [vchAthensOrgId] like '%70185259%' and len('70185259') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mvsu.edu' else 'mvsu.edu' end WHERE [vchAthensOrgId] like '%70215741%' and len('70215741') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',48678897.eng.nhs.uk' else '48678897.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%48678897%' and len('48678897') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69111742.moe.gov.sg' else '69111742.moe.gov.sg' end WHERE [vchAthensOrgId] like '%69111742%' and len('69111742') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',monsanto.com' else 'monsanto.com' end WHERE [vchAthensOrgId] like '%69988486%' and len('69988486') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531516.eng.nhs.uk' else '5531516.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531516%' and len('5531516') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',moredun.ac.uk' else 'moredun.ac.uk' end WHERE [vchAthensOrgId] like '%904311%' and len('904311') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',morleycollege.ac.uk' else 'morleycollege.ac.uk' end WHERE [vchAthensOrgId] like '%15962872%' and len('15962872') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',morlingcollege.com' else 'morlingcollege.com' end WHERE [vchAthensOrgId] like '%70289563%' and len('70289563') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54764162.atlantichealth.org' else '54764162.atlantichealth.org' end WHERE [vchAthensOrgId] like '%54764162%' and len('54764162') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523124.swanlibraries.net' else '69523124.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523124%' and len('69523124') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mosesbrown.org' else 'mosesbrown.org' end WHERE [vchAthensOrgId] like '%70211430%' and len('70211430') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924211.va.gov' else '54924211.va.gov' end WHERE [vchAthensOrgId] like '%54924211%' and len('54924211') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mountainsidehosp.com' else 'mountainsidehosp.com' end WHERE [vchAthensOrgId] like '%55278777%' and len('55278777') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcnpublishing.co.uk' else 'rcnpublishing.co.uk' end WHERE [vchAthensOrgId] like '%70135879%' and len('70135879') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%2619318%' and len('2619318') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%189112%' and len('189112') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%4299305%' and len('4299305') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%169021%' and len('169021') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%170094%' and len('170094') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%170170%' and len('170170') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%6793083%' and len('6793083') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%2782662%' and len('2782662') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%267893%' and len('267893') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%168907%' and len('168907') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%153130%' and len('153130') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%271042%' and len('271042') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%169827%' and len('169827') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%453579%' and len('453579') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%176300%' and len('176300') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%176318%' and len('176318') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%43634574%' and len('43634574') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%172465%' and len('172465') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%233008%' and len('233008') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%907486%' and len('907486') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%232993%' and len('232993') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%283497%' and len('283497') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%153144%' and len('153144') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%65850738%' and len('65850738') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%263313%' and len('263313') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%180542%' and len('180542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%1439886%' and len('1439886') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.gm' else 'mrc.gm' end WHERE [vchAthensOrgId] like '%1033165%' and len('1033165') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mrc.ac.uk' else 'mrc.ac.uk' end WHERE [vchAthensOrgId] like '%216542%' and len('216542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3682609.sjog.ie' else '3682609.sjog.ie' end WHERE [vchAthensOrgId] like '%3682609%' and len('3682609') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',munciepubliclibrary.org' else 'munciepubliclibrary.org' end WHERE [vchAthensOrgId] like '%70068139%' and len('70068139') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ingramcontent.com' else 'ingramcontent.com' end WHERE [vchAthensOrgId] like '%12664633%' and len('12664633') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ingramcontent.com' else 'ingramcontent.com' end WHERE [vchAthensOrgId] like '%12664634%' and len('12664634') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ingramcontent.com' else 'ingramcontent.com' end WHERE [vchAthensOrgId] like '%6375750%' and len('6375750') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523125.swanlibraries.net' else '69523125.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523125%' and len('69523125') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nu.ac.th' else 'nu.ac.th' end WHERE [vchAthensOrgId] like '%69834047%' and len('69834047') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226681.ccsnh.edu' else '70226681.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226681%' and len('70226681') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',natcen.ac.uk' else 'natcen.ac.uk' end WHERE [vchAthensOrgId] like '%80914%' and len('80914') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',damanhealth.ae' else 'damanhealth.ae' end WHERE [vchAthensOrgId] like '%60433554%' and len('60433554') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5649731.eng.nhs.uk' else '5649731.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5649731%' and len('5649731') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66236564.eng.nhs.uk' else '66236564.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66236564%' and len('66236564') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7438025.eng.nhs.uk' else '7438025.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%7438025%' and len('7438025') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nias.res.in' else 'nias.res.in' end WHERE [vchAthensOrgId] like '%66987617%' and len('66987617') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nid.edu' else 'nid.edu' end WHERE [vchAthensOrgId] like '%68288826%' and len('68288826') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',niesr.ac.uk' else 'niesr.ac.uk' end WHERE [vchAthensOrgId] like '%169%' and len('169') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nioi.gov.il' else 'nioi.gov.il' end WHERE [vchAthensOrgId] like '%60410821%' and len('60410821') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nls.uk' else 'nls.uk' end WHERE [vchAthensOrgId] like '%5807572%' and len('5807572') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nms.ac.uk' else 'nms.ac.uk' end WHERE [vchAthensOrgId] like '%2529444%' and len('2529444') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4131205.eng.nhs.uk' else '4131205.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4131205%' and len('4131205') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173566.scot.nhs.uk' else '4173566.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173566%' and len('4173566') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',icssr.org' else 'icssr.org' end WHERE [vchAthensOrgId] like '%69426490%' and len('69426490') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ne.defra.gsi.gov.uk' else 'ne.defra.gsi.gov.uk' end WHERE [vchAthensOrgId] like '%69745673%' and len('69745673') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cyfoethnaturiolcymru.gov.uk' else 'cyfoethnaturiolcymru.gov.uk' end WHERE [vchAthensOrgId] like '%67513207%' and len('67513207') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',macmillan.com' else 'macmillan.com' end WHERE [vchAthensOrgId] like '%12478960%' and len('12478960') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nawc.navy.mil' else 'nawc.navy.mil' end WHERE [vchAthensOrgId] like '%69487767%' and len('69487767') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',idp.wblnhrc.med.navy.mil' else 'idp.wblnhrc.med.navy.mil' end WHERE [vchAthensOrgId] like '%68164207%' and len('68164207') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nmcp.med.navy.mil' else 'nmcp.med.navy.mil' end WHERE [vchAthensOrgId] like '%54600664%' and len('54600664') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nmcsd.med.navy.mil' else 'nmcsd.med.navy.mil' end WHERE [vchAthensOrgId] like '%12988560%' and len('12988560') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nmel.med.navy.mil' else 'nmel.med.navy.mil' end WHERE [vchAthensOrgId] like '%66238467%' and len('66238467') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66193995.med.navy.mil' else '66193995.med.navy.mil' end WHERE [vchAthensOrgId] like '%66193995%' and len('66193995') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nmotc.navy.mil' else 'nmotc.navy.mil' end WHERE [vchAthensOrgId] like '%68959428%' and len('68959428') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercyhealth2.org' else 'mercyhealth2.org' end WHERE [vchAthensOrgId] like '%70211867%' and len('70211867') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ncad.ie' else 'ncad.ie' end WHERE [vchAthensOrgId] like '%54803643%' and len('54803643') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nelson.ac.uk' else 'nelson.ac.uk' end WHERE [vchAthensOrgId] like '%54346425%' and len('54346425') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69057460.wslhd.health.nsw.gov.au' else '69057460.wslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%69057460%' and len('69057460') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nescot.ac.uk' else 'nescot.ac.uk' end WHERE [vchAthensOrgId] like '%1825598%' and len('1825598') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',netanya.ac.il' else 'netanya.ac.il' end WHERE [vchAthensOrgId] like '%55198806%' and len('55198806') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69744248%' and len('69744248') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ncps-k12.org' else 'ncps-k12.org' end WHERE [vchAthensOrgId] like '%69647252%' and len('69647252') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',newdur.ac.uk' else 'newdur.ac.uk' end WHERE [vchAthensOrgId] like '%898970%' and len('898970') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',newcollege.ac.uk' else 'newcollege.ac.uk' end WHERE [vchAthensOrgId] like '%2287156%' and len('2287156') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38310072.va.gov' else '38310072.va.gov' end WHERE [vchAthensOrgId] like '%38310072%' and len('38310072') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43658416.va.gov' else '43658416.va.gov' end WHERE [vchAthensOrgId] like '%43658416%' and len('43658416') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68917597.newscientist.com' else '68917597.newscientist.com' end WHERE [vchAthensOrgId] like '%68917597%' and len('68917597') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',6878591.newscientist.com' else '6878591.newscientist.com' end WHERE [vchAthensOrgId] like '%6878591%' and len('6878591') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70305449.newscientist.com' else '70305449.newscientist.com' end WHERE [vchAthensOrgId] like '%70305449%' and len('70305449') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70305455.newscientist.com' else '70305455.newscientist.com' end WHERE [vchAthensOrgId] like '%70305455%' and len('70305455') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118162.akzonobel.com' else '66118162.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118162%' and len('66118162') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38306280.va.gov' else '38306280.va.gov' end WHERE [vchAthensOrgId] like '%38306280%' and len('38306280') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nbi.barnabashealth.org' else 'nbi.barnabashealth.org' end WHERE [vchAthensOrgId] like '%69348186%' and len('69348186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',newbold.ac.uk' else 'newbold.ac.uk' end WHERE [vchAthensOrgId] like '%7244033%' and len('7244033') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058629.eng.nhs.uk' else '4058629.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058629%' and len('4058629') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nulc.ac.uk' else 'nulc.ac.uk' end WHERE [vchAthensOrgId] like '%54609834%' and len('54609834') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',newman.ac.uk' else 'newman.ac.uk' end WHERE [vchAthensOrgId] like '%202%' and len('202') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',newsbank.com' else 'newsbank.com' end WHERE [vchAthensOrgId] like '%3993540%' and len('3993540') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70067379.xperthr.co.uk' else '70067379.xperthr.co.uk' end WHERE [vchAthensOrgId] like '%70067379%' and len('70067379') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66852913.atlantichealth.org' else '66852913.atlantichealth.org' end WHERE [vchAthensOrgId] like '%66852913%' and len('66852913') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',np.edu.sg' else 'np.edu.sg' end WHERE [vchAthensOrgId] like '%70186860%' and len('70186860') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5343859.eng.nhs.uk' else '5343859.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5343859%' and len('5343859') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130215.scot.nhs.uk' else '4130215.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130215%' and len('4130215') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5258557.eng.nhs.uk' else '5258557.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5258557%' and len('5258557') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130216.scot.nhs.uk' else '4130216.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130216%' and len('4130216') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762660.eng.nhs.uk' else '5762660.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762660%' and len('5762660') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66373279.eng.nhs.uk' else '66373279.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66373279%' and len('66373279') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66317622.eng.nhs.uk' else '66317622.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66317622%' and len('66317622') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762662.eng.nhs.uk' else '5762662.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762662%' and len('5762662') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762663.eng.nhs.uk' else '5762663.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762663%' and len('5762663') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43629789.eng.nhs.uk' else '43629789.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%43629789%' and len('43629789') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220870.eng.nhs.uk' else '38220870.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220870%' and len('38220870') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220871.eng.nhs.uk' else '38220871.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220871%' and len('38220871') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220873.eng.nhs.uk' else '38220873.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220873%' and len('38220873') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220866.eng.nhs.uk' else '38220866.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220866%' and len('38220866') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220867.eng.nhs.uk' else '38220867.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220867%' and len('38220867') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220874.eng.nhs.uk' else '38220874.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220874%' and len('38220874') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220875.eng.nhs.uk' else '38220875.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220875%' and len('38220875') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220872.eng.nhs.uk' else '38220872.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220872%' and len('38220872') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220869.eng.nhs.uk' else '38220869.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220869%' and len('38220869') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38220868.eng.nhs.uk' else '38220868.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38220868%' and len('38220868') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130217.scot.nhs.uk' else '4130217.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130217%' and len('4130217') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173559.scot.nhs.uk' else '4173559.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173559%' and len('4173559') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',1407223.eng.nhs.uk' else '1407223.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5634889.eng.nhs.uk' else '5634889.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5634889%' and len('5634889') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073160.eng.nhs.uk' else '4073160.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073160%' and len('4073160') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',1407227.eng.nhs.uk' else '1407227.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407227%' and len('1407227') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130218.scot.nhs.uk' else '4130218.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130218%' and len('4130218') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130219.scot.nhs.uk' else '4130219.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130219%' and len('4130219') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38537211.eng.nhs.uk' else '38537211.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%38537211%' and len('38537211') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762665.eng.nhs.uk' else '5762665.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762665%' and len('5762665') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130221.scot.nhs.uk' else '4130221.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130221%' and len('4130221') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130220.scot.nhs.uk' else '4130220.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130220%' and len('4130220') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173575.scot.nhs.uk' else '4173575.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173575%' and len('4173575') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130222.scot.nhs.uk' else '4130222.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130222%' and len('4130222') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69965228.eng.nhs.uk' else '69965228.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69965228%' and len('69965228') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5884685.eng.nhs.uk' else '5884685.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5884685%' and len('5884685') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112579.eng.nhs.uk' else '4112579.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112579%' and len('4112579') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5668916.eng.nhs.uk' else '5668916.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5668916%' and len('5668916') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130223.scot.nhs.uk' else '4130223.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130223%' and len('4130223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130224.scot.nhs.uk' else '4130224.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130224%' and len('4130224') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762757.eng.nhs.uk' else '5762757.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130225.scot.nhs.uk' else '4130225.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130225%' and len('4130225') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130230.scot.nhs.uk' else '4130230.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130230%' and len('4130230') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69983747.eng.nhs.uk' else '69983747.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69983747%' and len('69983747') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3201184.scot.nhs.uk' else '3201184.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%3201184%' and len('3201184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nes.scot.nhs.uk' else 'nes.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%8117009%' and len('8117009') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130226.scot.nhs.uk' else '4130226.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130226%' and len('4130226') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762666.eng.nhs.uk' else '5762666.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762666%' and len('5762666') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762760.eng.nhs.uk' else '5762760.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762661.eng.nhs.uk' else '5762661.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762661%' and len('5762661') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762770.eng.nhs.uk' else '5762770.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130227.scot.nhs.uk' else '4130227.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130227%' and len('4130227') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',2406727.wales.nhs.uk' else '2406727.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%2406727%' and len('2406727') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184700.eng.nhs.uk' else '5184700.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184700%' and len('5184700') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184701.eng.nhs.uk' else '5184701.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184701%' and len('5184701') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184699.eng.nhs.uk' else '5184699.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184699%' and len('5184699') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184703.eng.nhs.uk' else '5184703.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184703%' and len('5184703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184702.eng.nhs.uk' else '5184702.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184702%' and len('5184702') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184698.eng.nhs.uk' else '5184698.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184698%' and len('5184698') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130228.scot.nhs.uk' else '4130228.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130228%' and len('4130228') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762667.eng.nhs.uk' else '5762667.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762667%' and len('5762667') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173565.scot.nhs.uk' else '4173565.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173565%' and len('4173565') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226683.ccsnh.edu' else '70226683.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226683%' and len('70226683') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66217917.eng.nhs.uk' else '66217917.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66217917%' and len('66217917') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4131219.eng.nhs.uk' else '4131219.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4131219%' and len('4131219') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70085082.eng.nhs.uk' else '70085082.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%70085082%' and len('70085082') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',niva.no' else 'niva.no' end WHERE [vchAthensOrgId] like '%66949307%' and len('66949307') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7453808.eng.nhs.uk' else '7453808.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%7453808%' and len('7453808') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67433068.eng.nhs.uk' else '67433068.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67433068%' and len('67433068') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7329919.eng.nhs.uk' else '7329919.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%7329919%' and len('7329919') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223435.eng.nhs.uk' else '4223435.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223435%' and len('4223435') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67419982.eng.nhs.uk' else '67419982.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67419982%' and len('67419982') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762717.eng.nhs.uk' else '5762717.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762717%' and len('5762717') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762728.eng.nhs.uk' else '5762728.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762728%' and len('5762728') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762743.eng.nhs.uk' else '5762743.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762743%' and len('5762743') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762749.eng.nhs.uk' else '5762749.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762749%' and len('5762749') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415149.eng.nhs.uk' else '67415149.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415149%' and len('67415149') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058644.eng.nhs.uk' else '4058644.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058644%' and len('4058644') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195479.eng.nhs.uk' else '5195479.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195479%' and len('5195479') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223672.eng.nhs.uk' else '4223672.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223672%' and len('4223672') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531178.eng.nhs.uk' else '5531178.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531178%' and len('5531178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531207.eng.nhs.uk' else '5531207.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531207%' and len('5531207') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195563.eng.nhs.uk' else '5195563.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195563%' and len('5195563') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762761.eng.nhs.uk' else '5762761.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762761%' and len('5762761') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223708.eng.nhs.uk' else '4223708.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223708%' and len('4223708') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195554.eng.nhs.uk' else '5195554.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195554%' and len('5195554') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762705.eng.nhs.uk' else '5762705.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762705%' and len('5762705') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762766.eng.nhs.uk' else '5762766.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762766%' and len('5762766') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5908188.eng.nhs.uk' else '5908188.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5908188%' and len('5908188') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223537.eng.nhs.uk' else '4223537.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223537%' and len('4223537') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',6157505.eng.nhs.uk' else '6157505.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%6157505%' and len('6157505') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073230.eng.nhs.uk' else '4073230.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073230%' and len('4073230') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073229.eng.nhs.uk' else '4073229.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073229%' and len('4073229') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073223.eng.nhs.uk' else '4073223.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073223%' and len('4073223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073231.eng.nhs.uk' else '4073231.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073231%' and len('4073231') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',noroff.no' else 'noroff.no' end WHERE [vchAthensOrgId] like '%68784627%' and len('68784627') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762688.eng.nhs.uk' else '5762688.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762688%' and len('5762688') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54520548.ncahs.health.nsw.gov.au' else '54520548.ncahs.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54520548%' and len('54520548') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044094.eng.nhs.uk' else '4044094.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044094%' and len('4044094') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058642.eng.nhs.uk' else '4058642.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058642%' and len('4058642') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531187.eng.nhs.uk' else '5531187.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531187%' and len('5531187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4043844.eng.nhs.uk' else '4043844.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4043844%' and len('4043844') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ne-worcs.ac.uk' else 'ne-worcs.ac.uk' end WHERE [vchAthensOrgId] like '%2625612%' and len('2625612') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',northlindsey.ac.uk' else 'northlindsey.ac.uk' end WHERE [vchAthensOrgId] like '%2573922%' and len('2573922') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531517.eng.nhs.uk' else '5531517.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531517%' and len('5531517') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523126.swanlibraries.net' else '69523126.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523126%' and len('69523126') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',13393594.northsouth.edu' else '13393594.northsouth.edu' end WHERE [vchAthensOrgId] like '%13393594%' and len('13393594') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058650.eng.nhs.uk' else '4058650.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058650%' and len('4058650') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4043843.eng.nhs.uk' else '4043843.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4043843%' and len('4043843') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044188.eng.nhs.uk' else '4044188.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044188%' and len('4044188') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073204.eng.nhs.uk' else '4073204.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073204%' and len('4073204') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044132.eng.nhs.uk' else '4044132.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044132%' and len('4044132') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044143.eng.nhs.uk' else '4044143.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044143%' and len('4044143') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112325.eng.nhs.uk' else '4112325.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112325%' and len('4112325') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315894.eng.nhs.uk' else '4315894.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315894%' and len('4315894') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315896.eng.nhs.uk' else '4315896.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315896%' and len('4315896') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nbcol.ac.uk' else 'nbcol.ac.uk' end WHERE [vchAthensOrgId] like '%1713969%' and len('1713969') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',northeasthealthwangaratta.gov.au' else 'northeasthealthwangaratta.gov.au' end WHERE [vchAthensOrgId] like '%55177849%' and len('55177849') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924241.va.gov' else '54924241.va.gov' end WHERE [vchAthensOrgId] like '%54924241%' and len('54924241') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',northern.ac.uk' else 'northern.ac.uk' end WHERE [vchAthensOrgId] like '%1319686%' and len('1319686') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762722.eng.nhs.uk' else '5762722.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762722%' and len('5762722') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',northerntrust.hscni.net' else 'northerntrust.hscni.net' end WHERE [vchAthensOrgId] like '%66256448%' and len('66256448') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058704.eng.nhs.uk' else '4058704.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058704%' and len('4058704') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523127.swanlibraries.net' else '69523127.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523127%' and len('69523127') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38306283.va.gov' else '38306283.va.gov' end WHERE [vchAthensOrgId] like '%38306283%' and len('38306283') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',northland.ac.uk' else 'northland.ac.uk' end WHERE [vchAthensOrgId] like '%1759728%' and len('1759728') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058630.eng.nhs.uk' else '4058630.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058630%' and len('4058630') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058632.eng.nhs.uk' else '4058632.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058632%' and len('4058632') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nuca.ac.uk' else 'nuca.ac.uk' end WHERE [vchAthensOrgId] like '%1689854%' and len('1689854') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058702.eng.nhs.uk' else '4058702.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315921.eng.nhs.uk' else '4315921.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118194.akzonobel.com' else '66118194.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118194%' and len('66118194') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66121305.akzonobel.com' else '66121305.akzonobel.com' end WHERE [vchAthensOrgId] like '%66121305%' and len('66121305') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315963.eng.nhs.uk' else '4315963.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315963%' and len('4315963') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ntu.ac.uk' else 'ntu.ac.uk' end WHERE [vchAthensOrgId] like '%215%' and len('215') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315962.eng.nhs.uk' else '4315962.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315962%' and len('4315962') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315967.eng.nhs.uk' else '4315967.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315967%' and len('4315967') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',novanthealth.org' else 'novanthealth.org' end WHERE [vchAthensOrgId] like '%68655300%' and len('68655300') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68106166.eng.nhs.uk' else '68106166.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%68106166%' and len('68106166') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66972080.eng.nhs.uk' else '66972080.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66972080%' and len('66972080') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67595071.eng.nhs.uk' else '67595071.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67595071%' and len('67595071') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67265280.eng.nhs.uk' else '67265280.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67265280%' and len('67265280') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67544476.eng.nhs.uk' else '67544476.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67544476%' and len('67544476') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69578597.cyfoethnaturiolcymru.gov.uk' else '69578597.cyfoethnaturiolcymru.gov.uk' end WHERE [vchAthensOrgId] like '%69578597%' and len('69578597') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67488817.health.nsw.gov.au' else '67488817.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%67488817%' and len('67488817') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',health.nsw.gov.au' else 'health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%70114464%' and len('70114464') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',nutricia.com' else 'nutricia.com' end WHERE [vchAthensOrgId] like '%67691124%' and len('67691124') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69699959.knovel.com' else '69699959.knovel.com' end WHERE [vchAthensOrgId] like '%69699959%' and len('69699959') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69699967.knovel.com' else '69699967.knovel.com' end WHERE [vchAthensOrgId] like '%69699967%' and len('69699967') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523128.swanlibraries.net' else '69523128.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523128%' and len('69523128') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523129.swanlibraries.net' else '69523129.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523129%' and len('69523129') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69959091.sd.openathens.net' else '69959091.sd.openathens.net' end WHERE [vchAthensOrgId] like '%69959076%' and len('69959076') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sd.openathens.net' else 'sd.openathens.net' end WHERE [vchAthensOrgId] like '%69959076%' and len('69959076') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',oclc.org' else 'oclc.org' end WHERE [vchAthensOrgId] like '%1035441%' and len('1035441') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',statistics.gov.uk' else 'statistics.gov.uk' end WHERE [vchAthensOrgId] like '%6949046%' and len('6949046') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ohiohealth.com' else 'ohiohealth.com' end WHERE [vchAthensOrgId] like '%67499780%' and len('67499780') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633432.va.gov' else '43633432.va.gov' end WHERE [vchAthensOrgId] like '%43633432%' and len('43633432') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223437.eng.nhs.uk' else '4223437.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',oldstacktest.org' else 'oldstacktest.org' end WHERE [vchAthensOrgId] like '%70044956%' and len('70044956') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',omsb.org' else 'omsb.org' end WHERE [vchAthensOrgId] like '%67544593%' and len('67544593') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ontario.ca' else 'ontario.ca' end WHERE [vchAthensOrgId] like '%67057071%' and len('67057071') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',open.ac.uk' else 'open.ac.uk' end WHERE [vchAthensOrgId] like '%170%' and len('170') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ouc.ac.cy' else 'ouc.ac.cy' end WHERE [vchAthensOrgId] like '%12783270%' and len('12783270') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%70405194%' and len('70405194') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',marketing.openathens.net' else 'marketing.openathens.net' end WHERE [vchAthensOrgId] like '%70216488%' and len('70216488') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ps.openathens.net' else 'ps.openathens.net' end WHERE [vchAthensOrgId] like '%69394691%' and len('69394691') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',1407227.eng.nhs.uk' else '1407227.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%68063045%' and len('68063045') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',1407223.eng.nhs.uk' else '1407223.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69036815%' and len('69036815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%7221129%' and len('7221129') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',orange.com' else 'orange.com' end WHERE [vchAthensOrgId] like '%70305175%' and len('70305175') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',oes.edu' else 'oes.edu' end WHERE [vchAthensOrgId] like '%70211449%' and len('70211449') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924206.va.gov' else '54924206.va.gov' end WHERE [vchAthensOrgId] like '%54924206%' and len('54924206') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',braude.ac.il' else 'braude.ac.il' end WHERE [vchAthensOrgId] like '%43581016%' and len('43581016') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66888711.osakidetza.net' else '66888711.osakidetza.net' end WHERE [vchAthensOrgId] like '%66888711%' and len('66888711') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',osazzz.osakidetza.net' else 'osazzz.osakidetza.net' end WHERE [vchAthensOrgId] like '%54628452%' and len('54628452') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',osahuat.osakidetza.net' else 'osahuat.osakidetza.net' end WHERE [vchAthensOrgId] like '%67499667%' and len('67499667') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',osahdb.osakidetza.net' else 'osahdb.osakidetza.net' end WHERE [vchAthensOrgId] like '%67499663%' and len('67499663') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',osachd.osakidetza.net' else 'osachd.osakidetza.net' end WHERE [vchAthensOrgId] like '%66888726%' and len('66888726') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',osakhc.osakidetza.net' else 'osakhc.osakidetza.net' end WHERE [vchAthensOrgId] like '%66888725%' and len('66888725') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67433084.eng.nhs.uk' else '67433084.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67433084%' and len('67433084') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67419991.eng.nhs.uk' else '67419991.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67419991%' and len('67419991') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058666.eng.nhs.uk' else '4058666.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058666%' and len('4058666') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223644.eng.nhs.uk' else '4223644.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223644%' and len('4223644') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531180.eng.nhs.uk' else '5531180.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531180%' and len('5531180') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531210.eng.nhs.uk' else '5531210.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531210%' and len('5531210') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415919.eng.nhs.uk' else '67415919.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415919%' and len('67415919') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223711.eng.nhs.uk' else '4223711.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223711%' and len('4223711') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',8372016.eng.nhs.uk' else '8372016.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%8372016%' and len('8372016') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073171.eng.nhs.uk' else '4073171.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073171%' and len('4073171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195469.eng.nhs.uk' else '5195469.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195469%' and len('5195469') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5015565.eng.nhs.uk' else '5015565.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5015565%' and len('5015565') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531181.eng.nhs.uk' else '5531181.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531181%' and len('5531181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531211.eng.nhs.uk' else '5531211.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531211%' and len('5531211') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223712.eng.nhs.uk' else '4223712.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223712%' and len('4223712') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',65672815.eng.nhs.uk' else '65672815.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%65672815%' and len('65672815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67420009.eng.nhs.uk' else '67420009.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67420009%' and len('67420009') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904441.eng.nhs.uk' else '5904441.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904441%' and len('5904441') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315893.eng.nhs.uk' else '4315893.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315893%' and len('4315893') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223564.eng.nhs.uk' else '4223564.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223564%' and len('4223564') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195481.eng.nhs.uk' else '5195481.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195481%' and len('5195481') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223629.eng.nhs.uk' else '4223629.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223629%' and len('4223629') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415920.eng.nhs.uk' else '67415920.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415920%' and len('67415920') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',6891098.eng.nhs.uk' else '6891098.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%6891098%' and len('6891098') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762699.eng.nhs.uk' else '5762699.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762699%' and len('5762699') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',17516637.eng.nhs.uk' else '17516637.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%17516637%' and len('17516637') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4077653.eng.nhs.uk' else '4077653.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4077653%' and len('4077653') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044189.eng.nhs.uk' else '4044189.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044189%' and len('4044189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67416939.eng.nhs.uk' else '67416939.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67416939%' and len('67416939') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415117.eng.nhs.uk' else '67415117.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415117%' and len('67415117') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073177.eng.nhs.uk' else '4073177.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073177%' and len('4073177') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058638.eng.nhs.uk' else '4058638.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058638%' and len('4058638') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',otsuka-us.com' else 'otsuka-us.com' end WHERE [vchAthensOrgId] like '%70234874%' and len('70234874') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710689.bshsi.org' else '53710689.bshsi.org' end WHERE [vchAthensOrgId] like '%53710689%' and len('53710689') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',olchc.ie' else 'olchc.ie' end WHERE [vchAthensOrgId] like '%8337703%' and len('8337703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54764156.atlantichealth.org' else '54764156.atlantichealth.org' end WHERE [vchAthensOrgId] like '%54764156%' and len('54764156') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',odi.org.uk' else 'odi.org.uk' end WHERE [vchAthensOrgId] like '%214200%' and len('214200') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633433.va.gov' else '43633433.va.gov' end WHERE [vchAthensOrgId] like '%43633433%' and len('43633433') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12680184.ovid.com' else '12680184.ovid.com' end WHERE [vchAthensOrgId] like '%12680184%' and len('12680184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12680187.ovid.com' else '12680187.ovid.com' end WHERE [vchAthensOrgId] like '%12680187%' and len('12680187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223538.eng.nhs.uk' else '4223538.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223538%' and len('4223538') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223523.eng.nhs.uk' else '4223523.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223523%' and len('4223523') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp1.oup.com' else 'sp1.oup.com' end WHERE [vchAthensOrgId] like '%1066325%' and len('1066325') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223716.eng.nhs.uk' else '4223716.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223716%' and len('4223716') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',paceacademy.org' else 'paceacademy.org' end WHERE [vchAthensOrgId] like '%69696392%' and len('69696392') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',palmers.ac.uk' else 'palmers.ac.uk' end WHERE [vchAthensOrgId] like '%2711761%' and len('2711761') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523130.swanlibraries.net' else '69523130.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523130%' and len('69523130') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523131.swanlibraries.net' else '69523131.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523131%' and len('69523131') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lankenau.mainlinehealth.org' else 'lankenau.mainlinehealth.org' end WHERE [vchAthensOrgId] like '%68102010%' and len('68102010') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073202.eng.nhs.uk' else '4073202.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073202%' and len('4073202') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523132.swanlibraries.net' else '69523132.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523132%' and len('69523132') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66301914.eng.nhs.uk' else '66301914.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66301914%' and len('66301914') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597593.houstontx.gov' else '69597593.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597593%' and len('69597593') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',olmgroup.com' else 'olmgroup.com' end WHERE [vchAthensOrgId] like '%38335122%' and len('38335122') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pdf.net' else 'pdf.net' end WHERE [vchAthensOrgId] like '%66299565%' and len('66299565') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',peacehealth.org' else 'peacehealth.org' end WHERE [vchAthensOrgId] like '%70286764%' and len('70286764') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pearsoncollege.com' else 'pearsoncollege.com' end WHERE [vchAthensOrgId] like '%66954498%' and len('66954498') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pumc.edu.cn' else 'pumc.edu.cn' end WHERE [vchAthensOrgId] like '%70120761%' and len('70120761') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bjmu.edu.cn' else 'bjmu.edu.cn' end WHERE [vchAthensOrgId] like '%70124904%' and len('70124904') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019587.communitylibrary.net' else '70019587.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019587%' and len('70019587') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058667.eng.nhs.uk' else '4058667.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058667%' and len('4058667') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',peninsulahealth.gov.au' else 'peninsulahealth.gov.au' end WHERE [vchAthensOrgId] like '%54714372%' and len('54714372') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044162.eng.nhs.uk' else '4044162.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044162%' and len('4044162') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044181.eng.nhs.uk' else '4044181.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044181%' and len('4044181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pacollege.edu' else 'pacollege.edu' end WHERE [vchAthensOrgId] like '%69739447%' and len('69739447') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',peterborough.ac.uk/' else 'peterborough.ac.uk/' end WHERE [vchAthensOrgId] like '%66834449%' and len('66834449') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ndevon.ac.uk' else 'ndevon.ac.uk' end WHERE [vchAthensOrgId] like '%2349854%' and len('2349854') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',55272703.shell.com' else '55272703.shell.com' end WHERE [vchAthensOrgId] like '%55272703%' and len('55272703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924189.va.gov' else '54924189.va.gov' end WHERE [vchAthensOrgId] like '%54924189%' and len('54924189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',15771988.philips.com' else '15771988.philips.com' end WHERE [vchAthensOrgId] like '%15771988%' and len('15771988') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38216924.philips.com' else '38216924.philips.com' end WHERE [vchAthensOrgId] like '%38216924%' and len('38216924') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38216952.philips.com' else '38216952.philips.com' end WHERE [vchAthensOrgId] like '%38216952%' and len('38216952') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620198.ahsl.arizona.edu' else '12620198.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620198%' and len('12620198') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12849241.ahsl.arizona.edu' else '12849241.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12849241%' and len('12849241') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',piedmontu.edu' else 'piedmontu.edu' end WHERE [vchAthensOrgId] like '%70124933%' and len('70124933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12606376.ahsl.arizona.edu' else '12606376.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%68522155%' and len('68522155') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pirbright.bbsrc.ac.uk' else 'pirbright.bbsrc.ac.uk' end WHERE [vchAthensOrgId] like '%85836%' and len('85836') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',plannedparenthood.org' else 'plannedparenthood.org' end WHERE [vchAthensOrgId] like '%67850916%' and len('67850916') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',plumpton.ac.uk' else 'plumpton.ac.uk' end WHERE [vchAthensOrgId] like '%66805983%' and len('66805983') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762725.eng.nhs.uk' else '5762725.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762725%' and len('5762725') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12798590.wales.nhs.uk' else '12798590.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%12798590%' and len('12798590') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118178.akzonobel.com' else '66118178.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118178%' and len('66118178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118175.akzonobel.com' else '66118175.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118175%' and len('66118175') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',psm.edu' else 'psm.edu' end WHERE [vchAthensOrgId] like '%54320384%' and len('54320384') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762737.eng.nhs.uk' else '5762737.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762737%' and len('5762737') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69597594.houstontx.gov' else '69597594.houstontx.gov' end WHERE [vchAthensOrgId] like '%69597594%' and len('69597594') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pehrc.co.za' else 'pehrc.co.za' end WHERE [vchAthensOrgId] like '%67537932%' and len('67537932') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43557855.ncahs.health.nsw.gov.au' else '43557855.ncahs.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%43557855%' and len('43557855') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924253.va.gov' else '54924253.va.gov' end WHERE [vchAthensOrgId] like '%54924253%' and len('54924253') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223577.eng.nhs.uk' else '4223577.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223577%' and len('4223577') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118186.akzonobel.com' else '66118186.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118186%' and len('66118186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7459022.wales.nhs.uk' else '7459022.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7459022%' and len('7459022') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',prairiestate.edu' else 'prairiestate.edu' end WHERE [vchAthensOrgId] like '%70287526%' and len('70287526') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523133.swanlibraries.net' else '69523133.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523133%' and len('69523133') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523134.swanlibraries.net' else '69523134.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523134%' and len('69523134') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69983440.68139900.dtf.vic.gov.au' else '69983440.68139900.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%69983440%' and len('69983440') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',miamivalleyhospital.org' else 'miamivalleyhospital.org' end WHERE [vchAthensOrgId] like '%70053305%' and len('70053305') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',phs.org' else 'phs.org' end WHERE [vchAthensOrgId] like '%69988518%' and len('69988518') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',presencehealth.org' else 'presencehealth.org' end WHERE [vchAthensOrgId] like '%67942557%' and len('67942557') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',resurrection.presencehealth.org' else 'resurrection.presencehealth.org' end WHERE [vchAthensOrgId] like '%67569564%' and len('67569564') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stjo.presencehealth.org' else 'stjo.presencehealth.org' end WHERE [vchAthensOrgId] like '%67750008%' and len('67750008') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',reshealthcare.org' else 'reshealthcare.org' end WHERE [vchAthensOrgId] like '%55060015%' and len('55060015') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',smemc.reshealth.org' else 'smemc.reshealth.org' end WHERE [vchAthensOrgId] like '%67625725%' and len('67625725') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',pressreader.com' else 'pressreader.com' end WHERE [vchAthensOrgId] like '%66753319%' and len('66753319') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',primalpictures.com' else 'primalpictures.com' end WHERE [vchAthensOrgId] like '%2113971%' and len('2113971') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',phcc.gov.qa' else 'phcc.gov.qa' end WHERE [vchAthensOrgId] like '%69650951%' and len('69650951') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073152.eng.nhs.uk' else '4073152.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073152%' and len('4073152') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223651.eng.nhs.uk' else '4223651.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223651%' and len('4223651') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',phs.princetonk12.org' else 'phs.princetonk12.org' end WHERE [vchAthensOrgId] like '%70217138%' and len('70217138') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12661257.proquest.com' else '12661257.proquest.com' end WHERE [vchAthensOrgId] like '%12661257%' and len('12661257') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12661261.proquest.com' else '12661261.proquest.com' end WHERE [vchAthensOrgId] like '%12661261%' and len('12661261') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',1855355.proquest.com' else '1855355.proquest.com' end WHERE [vchAthensOrgId] like '%1855355%' and len('1855355') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',eblib.com' else 'eblib.com' end WHERE [vchAthensOrgId] like '%6195309%' and len('6195309') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',csa.com' else 'csa.com' end WHERE [vchAthensOrgId] like '%631403%' and len('631403') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',provadis-hochschule.de' else 'provadis-hochschule.de' end WHERE [vchAthensOrgId] like '%69999584%' and len('69999584') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073158.eng.nhs.uk' else '4073158.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073158%' and len('4073158') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',providence.org' else 'providence.org' end WHERE [vchAthensOrgId] like '%70169876%' and len('70169876') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924175.va.gov' else '54924175.va.gov' end WHERE [vchAthensOrgId] like '%54924175%' and len('54924175') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66096408.ascensionhealth.org' else '66096408.ascensionhealth.org' end WHERE [vchAthensOrgId] like '%66096408%' and len('66096408') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',scilab-inc.com' else 'scilab-inc.com' end WHERE [vchAthensOrgId] like '%17525281%' and len('17525281') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3201184.scot.nhs.uk' else '3201184.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%68788166%' and len('68788166') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415116.eng.nhs.uk' else '67415116.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415116%' and len('67415116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68049538.eng.nhs.uk' else '68049538.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%68049538%' and len('68049538') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12580108.eng.nhs.uk' else '12580108.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%12580108%' and len('12580108') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044184.eng.nhs.uk' else '4044184.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044184%' and len('4044184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058664.eng.nhs.uk' else '4058664.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058664%' and len('4058664') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904431.eng.nhs.uk' else '5904431.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904431%' and len('5904431') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415132.eng.nhs.uk' else '67415132.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415132%' and len('67415132') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073199.eng.nhs.uk' else '4073199.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073199%' and len('4073199') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058663.eng.nhs.uk' else '4058663.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058663%' and len('4058663') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904437.eng.nhs.uk' else '5904437.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904437%' and len('5904437') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315941.eng.nhs.uk' else '4315941.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315941%' and len('4315941') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762732.eng.nhs.uk' else '5762732.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762732%' and len('5762732') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073155.eng.nhs.uk' else '4073155.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073155%' and len('4073155') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058635.eng.nhs.uk' else '4058635.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058635%' and len('4058635') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5905900.eng.nhs.uk' else '5905900.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5905900%' and len('5905900') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5894662.eng.nhs.uk' else '5894662.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5894662%' and len('5894662') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058723.eng.nhs.uk' else '4058723.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058723%' and len('4058723') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531182.eng.nhs.uk' else '5531182.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531182%' and len('5531182') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415123.eng.nhs.uk' else '67415123.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415123%' and len('67415123') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315882.eng.nhs.uk' else '4315882.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315882%' and len('4315882') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397811.eng.nhs.uk' else '67397811.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397811%' and len('67397811') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058641.eng.nhs.uk' else '4058641.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058641%' and len('4058641') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531160.eng.nhs.uk' else '5531160.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531160%' and len('5531160') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531208.eng.nhs.uk' else '5531208.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531208%' and len('5531208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058709.eng.nhs.uk' else '4058709.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058709%' and len('4058709') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315902.eng.nhs.uk' else '4315902.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315902%' and len('4315902') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058639.eng.nhs.uk' else '4058639.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058639%' and len('4058639') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397822.eng.nhs.uk' else '67397822.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397822%' and len('67397822') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397844.eng.nhs.uk' else '67397844.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397844%' and len('67397844') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531177.eng.nhs.uk' else '5531177.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531177%' and len('5531177') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415144.eng.nhs.uk' else '67415144.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415144%' and len('67415144') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904444.eng.nhs.uk' else '5904444.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904444%' and len('5904444') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223709.eng.nhs.uk' else '4223709.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223709%' and len('4223709') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315907.eng.nhs.uk' else '4315907.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315907%' and len('4315907') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904448.eng.nhs.uk' else '5904448.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904448%' and len('5904448') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112322.eng.nhs.uk' else '4112322.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112322%' and len('4112322') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904447.eng.nhs.uk' else '5904447.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904447%' and len('5904447') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67433064.eng.nhs.uk' else '67433064.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67433064%' and len('67433064') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67419958.eng.nhs.uk' else '67419958.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67419958%' and len('67419958') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904440.eng.nhs.uk' else '5904440.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904440%' and len('5904440') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5858353.eng.nhs.uk' else '5858353.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5858353%' and len('5858353') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195480.eng.nhs.uk' else '5195480.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195480%' and len('5195480') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53986334.eng.nhs.uk' else '53986334.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%53986334%' and len('53986334') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415910.eng.nhs.uk' else '67415910.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415910%' and len('67415910') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195555.eng.nhs.uk' else '5195555.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195555%' and len('5195555') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223636.eng.nhs.uk' else '4223636.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223636%' and len('4223636') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223645.eng.nhs.uk' else '4223645.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223645%' and len('4223645') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223530.eng.nhs.uk' else '4223530.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223530%' and len('4223530') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195476.eng.nhs.uk' else '5195476.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195476%' and len('5195476') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70114697.eng.nhs.uk' else '70114697.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%70114697%' and len('70114697') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073217.eng.nhs.uk' else '4073217.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073217%' and len('4073217') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7472901.wales.nhs.uk' else '7472901.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%7472901%' and len('7472901') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',karger.ch' else 'karger.ch' end WHERE [vchAthensOrgId] like '%68290024%' and len('68290024') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118192.akzonobel.com' else '66118192.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118192%' and len('66118192') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118187.akzonobel.com' else '66118187.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118187%' and len('66118187') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ptu.ac.in' else 'ptu.ac.in' end WHERE [vchAthensOrgId] like '%68467580%' and len('68467580') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',purduepharma.com' else 'purduepharma.com' end WHERE [vchAthensOrgId] like '%70270384%' and len('70270384') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073228.eng.nhs.uk' else '4073228.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073228%' and len('4073228') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qmc.ac.uk' else 'qmc.ac.uk' end WHERE [vchAthensOrgId] like '%2620160%' and len('2620160') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223620.eng.nhs.uk' else '4223620.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223620%' and len('4223620') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',quinnipiac.edu' else 'quinnipiac.edu' end WHERE [vchAthensOrgId] like '%66744718%' and len('66744718') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ra21.org' else 'ra21.org' end WHERE [vchAthensOrgId] like '%70247806%' and len('70247806') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rsna.org' else 'rsna.org' end WHERE [vchAthensOrgId] like '%67673208%' and len('67673208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924199.va.gov' else '54924199.va.gov' end WHERE [vchAthensOrgId] like '%54924199%' and len('54924199') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38484815.ramsayhealth.com.au' else '38484815.ramsayhealth.com.au' end WHERE [vchAthensOrgId] like '%38484815%' and len('38484815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38479388.ramsayhealth.com.au' else '38479388.ramsayhealth.com.au' end WHERE [vchAthensOrgId] like '%38479388%' and len('38479388') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rave.ac.uk' else 'rave.ac.uk' end WHERE [vchAthensOrgId] like '%127%' and len('127') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',raytheon.com' else 'raytheon.com' end WHERE [vchAthensOrgId] like '%70149830%' and len('70149830') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',www.raytheon.com' else 'www.raytheon.com' end WHERE [vchAthensOrgId] like '%70267428%' and len('70267428') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',xperthr.co.uk' else 'xperthr.co.uk' end WHERE [vchAthensOrgId] like '%2558542%' and len('2558542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcnpublishing.co.uk' else 'rcnpublishing.co.uk' end WHERE [vchAthensOrgId] like '%67981687%' and len('67981687') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcsi-mub.com' else 'rcsi-mub.com' end WHERE [vchAthensOrgId] like '%12467713%' and len('12467713') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118190.akzonobel.com' else '66118190.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118190%' and len('66118190') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118191.akzonobel.com' else '66118191.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118191%' and len('66118191') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118193.akzonobel.com' else '66118193.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118193%' and len('66118193') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',reaseheath.ac.uk' else 'reaseheath.ac.uk' end WHERE [vchAthensOrgId] like '%69987993%' and len('69987993') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cleveland.ac.uk' else 'cleveland.ac.uk' end WHERE [vchAthensOrgId] like '%1801348%' and len('1801348') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',redcliffe.ac.uk' else 'redcliffe.ac.uk' end WHERE [vchAthensOrgId] like '%69584315%' and len('69584315') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',regents.ac.uk' else 'regents.ac.uk' end WHERE [vchAthensOrgId] like '%15971122%' and len('15971122') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',skane.se' else 'skane.se' end WHERE [vchAthensOrgId] like '%67386336%' and len('67386336') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',healthpartners.com' else 'healthpartners.com' end WHERE [vchAthensOrgId] like '%66969286%' and len('66969286') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397799.eng.nhs.uk' else '67397799.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315919.eng.nhs.uk' else '4315919.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315919%' and len('4315919') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',reprintsdesk.com' else 'reprintsdesk.com' end WHERE [vchAthensOrgId] like '%68715640%' and len('68715640') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',evidenceinformedpractice.org' else 'evidenceinformedpractice.org' end WHERE [vchAthensOrgId] like '%66941740%' and len('66941740') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',repoa.or.tz' else 'repoa.or.tz' end WHERE [vchAthensOrgId] like '%55223414%' and len('55223414') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924222.va.gov' else '54924222.va.gov' end WHERE [vchAthensOrgId] like '%54924222%' and len('54924222') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',richardtaunton.ac.uk' else 'richardtaunton.ac.uk' end WHERE [vchAthensOrgId] like '%54568656%' and len('54568656') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38508089.va.gov' else '38508089.va.gov' end WHERE [vchAthensOrgId] like '%38508089%' and len('38508089') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523135.swanlibraries.net' else '69523135.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523135%' and len('69523135') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',raa.se' else 'raa.se' end WHERE [vchAthensOrgId] like '%70106039%' and len('70106039') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rittenhouse.com' else 'rittenhouse.com' end WHERE [vchAthensOrgId] like '%15908606%' and len('15908606') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523136.swanlibraries.net' else '69523136.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523136%' and len('69523136') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523137.swanlibraries.net' else '69523137.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523137%' and len('69523137') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226685.ccsnh.edu' else '70226685.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226685%' and len('70226685') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523138.swanlibraries.net' else '69523138.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523138%' and len('69523138') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523139.swanlibraries.net' else '69523139.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523139%' and len('69523139') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924236.va.gov' else '54924236.va.gov' end WHERE [vchAthensOrgId] like '%54924236%' and len('54924236') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195557.eng.nhs.uk' else '5195557.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195557%' and len('5195557') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rwjuh.edu' else 'rwjuh.edu' end WHERE [vchAthensOrgId] like '%68525891%' and len('68525891') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',roche.com' else 'roche.com' end WHERE [vchAthensOrgId] like '%66246128%' and len('66246128') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rmuohp.edu' else 'rmuohp.edu' end WHERE [vchAthensOrgId] like '%69957609%' and len('69957609') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',roosevelt.edu' else 'roosevelt.edu' end WHERE [vchAthensOrgId] like '%70106377%' and len('70106377') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bruford.ac.uk' else 'bruford.ac.uk' end WHERE [vchAthensOrgId] like '%3637138%' and len('3637138') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',bruford.ac.uk' else 'bruford.ac.uk' end WHERE [vchAthensOrgId] like '%3637138%' and len('3637138') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rothamsted.bbsrc.ac.uk' else 'rothamsted.bbsrc.ac.uk' end WHERE [vchAthensOrgId] like '%85838%' and len('85838') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315913.eng.nhs.uk' else '4315913.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315913%' and len('4315913') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315922.eng.nhs.uk' else '4315922.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315922%' and len('4315922') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rotunda.ie' else 'rotunda.ie' end WHERE [vchAthensOrgId] like '%7374871%' and len('7374871') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rad.org.uk' else 'rad.org.uk' end WHERE [vchAthensOrgId] like '%54442887%' and len('54442887') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ram.ac.uk' else 'ram.ac.uk' end WHERE [vchAthensOrgId] like '%1445420%' and len('1445420') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rac.ac.uk' else 'rac.ac.uk' end WHERE [vchAthensOrgId] like '%2335850%' and len('2335850') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',webplace.com.au' else 'webplace.com.au' end WHERE [vchAthensOrgId] like '%60457078%' and len('60457078') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223525.eng.nhs.uk' else '4223525.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223525%' and len('4223525') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762739.eng.nhs.uk' else '5762739.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762739%' and len('5762739') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5633824.eng.nhs.uk' else '5633824.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5633824%' and len('5633824') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cssd.ac.uk' else 'cssd.ac.uk' end WHERE [vchAthensOrgId] like '%1352949%' and len('1352949') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rca.ac.uk' else 'rca.ac.uk' end WHERE [vchAthensOrgId] like '%177%' and len('177') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.rcgp.org.uk' else 'sp.rcgp.org.uk' end WHERE [vchAthensOrgId] like '%68168260%' and len('68168260') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcgp.org.uk' else 'rcgp.org.uk' end WHERE [vchAthensOrgId] like '%68800486%' and len('68800486') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcm.ac.uk' else 'rcm.ac.uk' end WHERE [vchAthensOrgId] like '%5783604%' and len('5783604') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp1.rcplondon.ac.uk' else 'sp1.rcplondon.ac.uk' end WHERE [vchAthensOrgId] like '%67598713%' and len('67598713') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcplondon.ac.uk' else 'rcplondon.ac.uk' end WHERE [vchAthensOrgId] like '%1019339%' and len('1019339') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcpsych.ac.uk' else 'rcpsych.ac.uk' end WHERE [vchAthensOrgId] like '%68002976%' and len('68002976') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.rcpsych.ac.uk' else 'sp.rcpsych.ac.uk' end WHERE [vchAthensOrgId] like '%7222153%' and len('7222153') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rcseng.ac.uk' else 'rcseng.ac.uk' end WHERE [vchAthensOrgId] like '%1717598%' and len('1717598') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rsceng.ac.uk' else 'rsceng.ac.uk' end WHERE [vchAthensOrgId] like '%69509609%' and len('69509609') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',knowledge.rcvs.org.uk' else 'knowledge.rcvs.org.uk' end WHERE [vchAthensOrgId] like '%68028189%' and len('68028189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762711.eng.nhs.uk' else '5762711.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762711%' and len('5762711') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762727.eng.nhs.uk' else '5762727.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762727%' and len('5762727') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54828315.shell.com' else '54828315.shell.com' end WHERE [vchAthensOrgId] like '%54828315%' and len('54828315') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531150.eng.nhs.uk' else '5531150.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531150%' and len('5531150') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rhul.ac.uk' else 'rhul.ac.uk' end WHERE [vchAthensOrgId] like '%121%' and len('121') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rhs.org.uk' else 'rhs.org.uk' end WHERE [vchAthensOrgId] like '%69346232%' and len('69346232') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',riam.ie' else 'riam.ie' end WHERE [vchAthensOrgId] like '%38411421%' and len('38411421') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044124.eng.nhs.uk' else '4044124.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044124%' and len('4044124') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5587600.eng.nhs.uk' else '5587600.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5587600%' and len('5587600') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531151.eng.nhs.uk' else '5531151.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531151%' and len('5531151') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rncm.ac.uk' else 'rncm.ac.uk' end WHERE [vchAthensOrgId] like '%1320365%' and len('1320365') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195503.eng.nhs.uk' else '5195503.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195503%' and len('5195503') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rpsgb.org.uk' else 'rpsgb.org.uk' end WHERE [vchAthensOrgId] like '%3639919%' and len('3639919') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',royalprincealfred.sswahs.nsw.gov.au' else 'royalprincealfred.sswahs.nsw.gov.au' end WHERE [vchAthensOrgId] like '%43595571%' and len('43595571') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rsc.org' else 'rsc.org' end WHERE [vchAthensOrgId] like '%5859809%' and len('5859809') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223621.eng.nhs.uk' else '4223621.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223621%' and len('4223621') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762689.eng.nhs.uk' else '5762689.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762689%' and len('5762689') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rvc.ac.uk' else 'rvc.ac.uk' end WHERE [vchAthensOrgId] like '%153142%' and len('153142') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195504.eng.nhs.uk' else '5195504.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195504%' and len('5195504') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rspb.org.uk' else 'rspb.org.uk' end WHERE [vchAthensOrgId] like '%1826707%' and len('1826707') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',rti.org' else 'rti.org' end WHERE [vchAthensOrgId] like '%70166384%' and len('70166384') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',karger.ch' else 'karger.ch' end WHERE [vchAthensOrgId] like '%66123796%' and len('66123796') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jcyl.es' else 'jcyl.es' end WHERE [vchAthensOrgId] like '%68056493%' and len('68056493') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sae.org' else 'sae.org' end WHERE [vchAthensOrgId] like '%68394956%' and len('68394956') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sagepub.com' else 'sagepub.com' end WHERE [vchAthensOrgId] like '%5950748%' and len('5950748') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sagerx.com' else 'sagerx.com' end WHERE [vchAthensOrgId] like '%70267374%' and len('70267374') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sbmc.barnabashealth.org' else 'sbmc.barnabashealth.org' end WHERE [vchAthensOrgId] like '%69354485%' and len('69354485') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stchristophershospital.com' else 'stchristophershospital.com' end WHERE [vchAthensOrgId] like '%69415245%' and len('69415245') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68841264.saintlukeshealthsystem.org' else '68841264.saintlukeshealthsystem.org' end WHERE [vchAthensOrgId] like '%68841264%' and len('68841264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',saintlukeshealthsystem.org' else 'saintlukeshealthsystem.org' end WHERE [vchAthensOrgId] like '%68785934%' and len('68785934') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stthomas.org' else 'stthomas.org' end WHERE [vchAthensOrgId] like '%12846847%' and len('12846847') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',55272714.shell.com' else '55272714.shell.com' end WHERE [vchAthensOrgId] like '%55272714%' and len('55272714') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',salemhealth.org' else 'salemhealth.org' end WHERE [vchAthensOrgId] like '%69797589%' and len('69797589') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54444472.va.gov' else '54444472.va.gov' end WHERE [vchAthensOrgId] like '%54444472%' and len('54444472') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044164.eng.nhs.uk' else '4044164.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044164%' and len('4044164') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762767.eng.nhs.uk' else '5762767.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762767%' and len('5762767') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54444473.va.gov' else '54444473.va.gov' end WHERE [vchAthensOrgId] like '%54444473%' and len('54444473') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924178.va.gov' else '54924178.va.gov' end WHERE [vchAthensOrgId] like '%54924178%' and len('54924178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66270378.us.af.mil' else '66270378.us.af.mil' end WHERE [vchAthensOrgId] like '%66270378%' and len('66270378') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924264.va.gov' else '54924264.va.gov' end WHERE [vchAthensOrgId] like '%54924264%' and len('54924264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924204.va.gov' else '54924204.va.gov' end WHERE [vchAthensOrgId] like '%54924204%' and len('54924204') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195489.eng.nhs.uk' else '5195489.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195489%' and len('5195489') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195505.eng.nhs.uk' else '5195505.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195505%' and len('5195505') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',smh.com' else 'smh.com' end WHERE [vchAthensOrgId] like '%8314280%' and len('8314280') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sardegnaricerche.it' else 'sardegnaricerche.it' end WHERE [vchAthensOrgId] like '%69478140%' and len('69478140') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69557087.openathenstrials.net' else '69557087.openathenstrials.net' end WHERE [vchAthensOrgId] like '%69557087%' and len('69557087') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53719689.bshsi.org' else '53719689.bshsi.org' end WHERE [vchAthensOrgId] like '%53719689%' and len('53719689') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523140.swanlibraries.net' else '69523140.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523140%' and len('69523140') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315924.eng.nhs.uk' else '4315924.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315924%' and len('4315924') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70051529.knowledgee.com' else '70051529.knowledgee.com' end WHERE [vchAthensOrgId] like '%70051529%' and len('70051529') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stfc.ac.uk' else 'stfc.ac.uk' end WHERE [vchAthensOrgId] like '%232%' and len('232') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173563.scot.nhs.uk' else '4173563.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173563%' and len('4173563') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4173567.scot.nhs.uk' else '4173567.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4173567%' and len('4173567') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',scrlc.org' else 'scrlc.org' end WHERE [vchAthensOrgId] like '%70226854%' and len('70226854') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',seattlegenetics.com' else 'seattlegenetics.com' end WHERE [vchAthensOrgId] like '%70042256%' and len('70042256') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',secardiologia.es' else 'secardiologia.es' end WHERE [vchAthensOrgId] like '%68223092%' and len('68223092') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp1.oup.com' else 'sp1.oup.com' end WHERE [vchAthensOrgId] like '%66795617%' and len('66795617') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',semantico.com' else 'semantico.com' end WHERE [vchAthensOrgId] like '%66796667%' and len('66796667') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sentara.com' else 'sentara.com' end WHERE [vchAthensOrgId] like '%69362056%' and len('69362056') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp1.proquest.com' else 'sp1.proquest.com' end WHERE [vchAthensOrgId] like '%68074799%' and len('68074799') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70269969.wales.nhs.uk' else '70269969.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%70269969%' and len('70269969') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315926.eng.nhs.uk' else '4315926.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315926%' and len('4315926') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315925.eng.nhs.uk' else '4315925.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315925%' and len('4315925') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315929.eng.nhs.uk' else '4315929.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315929%' and len('4315929') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54828315.shell.com' else '54828315.shell.com' end WHERE [vchAthensOrgId] like '%69331727%' and len('69331727') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66088399.shell.com' else '66088399.shell.com' end WHERE [vchAthensOrgId] like '%66088399%' and len('66088399') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',skuastkashmir.ac.in' else 'skuastkashmir.ac.in' end WHERE [vchAthensOrgId] like '%69543534%' and len('69543534') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924250.va.gov' else '54924250.va.gov' end WHERE [vchAthensOrgId] like '%54924250%' and len('54924250') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315971.eng.nhs.uk' else '4315971.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315971%' and len('4315971') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66266800.shell.com' else '66266800.shell.com' end WHERE [vchAthensOrgId] like '%66266800%' and len('66266800') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195558.eng.nhs.uk' else '5195558.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195558%' and len('5195558') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195560.eng.nhs.uk' else '5195560.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195560%' and len('5195560') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',signature-healthcare.org' else 'signature-healthcare.org' end WHERE [vchAthensOrgId] like '%38166634%' and len('38166634') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',silverchair.com' else 'silverchair.com' end WHERE [vchAthensOrgId] like '%68274203%' and len('68274203') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',smu.edu.sg' else 'smu.edu.sg' end WHERE [vchAthensOrgId] like '%69678400%' and len('69678400') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%70103099%' and len('70103099') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp.edu.sg' else 'sp.edu.sg' end WHERE [vchAthensOrgId] like '%70156988%' and len('70156988') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69975506%' and len('69975506') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sutd.edu.sg' else 'sutd.edu.sg' end WHERE [vchAthensOrgId] like '%70388872%' and len('70388872') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54950376.va.gov' else '54950376.va.gov' end WHERE [vchAthensOrgId] like '%54950376%' and len('54950376') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sgrh.com' else 'sgrh.com' end WHERE [vchAthensOrgId] like '%67396364%' and len('67396364') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',george-monoux.ac.uk' else 'george-monoux.ac.uk' end WHERE [vchAthensOrgId] like '%5350574%' and len('5350574') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sirsidynix.com' else 'sirsidynix.com' end WHERE [vchAthensOrgId] like '%69593356%' and len('69593356') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',solihullsfc.ac.uk' else 'solihullsfc.ac.uk' end WHERE [vchAthensOrgId] like '%568849%' and len('568849') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',athens-stage.snapwiz.net' else 'athens-stage.snapwiz.net' end WHERE [vchAthensOrgId] like '%65497401%' and len('65497401') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67518363.eng.nhs.uk' else '67518363.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67518363%' and len('67518363') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397824.eng.nhs.uk' else '67397824.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397824%' and len('67397824') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67433081.eng.nhs.uk' else '67433081.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67433081%' and len('67433081') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223631.eng.nhs.uk' else '4223631.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223631%' and len('4223631') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904432.eng.nhs.uk' else '5904432.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904432%' and len('5904432') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058693.eng.nhs.uk' else '4058693.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058693%' and len('4058693') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904434.eng.nhs.uk' else '5904434.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904434%' and len('5904434') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195533.eng.nhs.uk' else '5195533.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195533%' and len('5195533') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397829.eng.nhs.uk' else '67397829.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397829%' and len('67397829') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904435.eng.nhs.uk' else '5904435.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904435%' and len('5904435') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904439.eng.nhs.uk' else '5904439.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904439%' and len('5904439') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223665.eng.nhs.uk' else '4223665.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223665%' and len('4223665') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762754.eng.nhs.uk' else '5762754.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762754%' and len('5762754') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415157.eng.nhs.uk' else '67415157.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415157%' and len('67415157') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058643.eng.nhs.uk' else '4058643.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058643%' and len('4058643') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5804074.eng.nhs.uk' else '5804074.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5804074%' and len('5804074') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058682.eng.nhs.uk' else '4058682.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058682%' and len('4058682') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397849.eng.nhs.uk' else '67397849.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397849%' and len('67397849') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397833.eng.nhs.uk' else '67397833.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397833%' and len('67397833') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531179.eng.nhs.uk' else '5531179.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531179%' and len('5531179') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531214.eng.nhs.uk' else '5531214.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531214%' and len('5531214') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66243642.eng.nhs.uk' else '66243642.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66243642%' and len('66243642') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315903.eng.nhs.uk' else '4315903.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315903%' and len('4315903') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67397838.eng.nhs.uk' else '67397838.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67397838%' and len('67397838') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67415146.eng.nhs.uk' else '67415146.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67415146%' and len('67415146') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195559.eng.nhs.uk' else '5195559.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195559%' and len('5195559') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904445.eng.nhs.uk' else '5904445.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904445%' and len('5904445') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223710.eng.nhs.uk' else '4223710.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223710%' and len('4223710') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315914.eng.nhs.uk' else '4315914.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315914%' and len('4315914') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5657043.eng.nhs.uk' else '5657043.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5657043%' and len('5657043') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223630.eng.nhs.uk' else '4223630.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223630%' and len('4223630') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904449.eng.nhs.uk' else '5904449.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904449%' and len('5904449') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223533.eng.nhs.uk' else '4223533.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223533%' and len('4223533') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223671.eng.nhs.uk' else '4223671.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223671%' and len('4223671') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904446.eng.nhs.uk' else '5904446.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904446%' and len('5904446') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195474.eng.nhs.uk' else '5195474.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195474%' and len('5195474') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073170.eng.nhs.uk' else '4073170.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073170%' and len('4073170') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sefm.es' else 'sefm.es' end WHERE [vchAthensOrgId] like '%67480608%' and len('67480608') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sal.org.uk' else 'sal.org.uk' end WHERE [vchAthensOrgId] like '%54617908%' and len('54617908') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sofcot.fr' else 'sofcot.fr' end WHERE [vchAthensOrgId] like '%70215698%' and len('70215698') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223578.eng.nhs.uk' else '4223578.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223578%' and len('4223578') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195491.eng.nhs.uk' else '5195491.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195491%' and len('5195491') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',solvay.com' else 'solvay.com' end WHERE [vchAthensOrgId] like '%70228862%' and len('70228862') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sclsnj.org' else 'sclsnj.org' end WHERE [vchAthensOrgId] like '%70371969%' and len('70371969') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762759.eng.nhs.uk' else '5762759.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762759%' and len('5762759') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223529.eng.nhs.uk' else '4223529.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223529%' and len('4223529') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',s-cheshire.ac.uk' else 's-cheshire.ac.uk' end WHERE [vchAthensOrgId] like '%1437997%' and len('1437997') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5527709.eng.nhs.uk' else '5527709.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5527709%' and len('5527709') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223626.eng.nhs.uk' else '4223626.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223626%' and len('4223626') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',southend.ac.uk' else 'southend.ac.uk' end WHERE [vchAthensOrgId] like '%1644819%' and len('1644819') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523141.swanlibraries.net' else '69523141.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523141%' and len('69523141') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223715.eng.nhs.uk' else '4223715.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223715%' and len('4223715') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195546.eng.nhs.uk' else '5195546.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195546%' and len('5195546') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058651.eng.nhs.uk' else '4058651.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058651%' and len('4058651') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54950378.va.gov' else '54950378.va.gov' end WHERE [vchAthensOrgId] like '%54950378%' and len('54950378') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058634.eng.nhs.uk' else '4058634.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058634%' and len('4058634') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195522.eng.nhs.uk' else '5195522.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195522%' and len('5195522') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5582603.eng.nhs.uk' else '5582603.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5582603%' and len('5582603') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',southwesthealthcare.gov.au' else 'southwesthealthcare.gov.au' end WHERE [vchAthensOrgId] like '%66913146%' and len('66913146') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223737.eng.nhs.uk' else '4223737.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223737%' and len('4223737') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762706.eng.nhs.uk' else '5762706.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762706%' and len('5762706') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058678.eng.nhs.uk' else '4058678.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058678%' and len('4058678') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762703.eng.nhs.uk' else '5762703.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762703%' and len('5762703') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633431.va.gov' else '43633431.va.gov' end WHERE [vchAthensOrgId] like '%43633431%' and len('43633431') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073154.eng.nhs.uk' else '4073154.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073154%' and len('4073154') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924242.va.gov' else '54924242.va.gov' end WHERE [vchAthensOrgId] like '%54924242%' and len('54924242') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223572.eng.nhs.uk' else '4223572.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223572%' and len('4223572') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044120.eng.nhs.uk' else '4044120.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044120%' and len('4044120') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044136.eng.nhs.uk' else '4044136.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112311.eng.nhs.uk' else '4112311.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044121.eng.nhs.uk' else '4044121.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112331.eng.nhs.uk' else '4112331.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112320.eng.nhs.uk' else '4112320.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112313.eng.nhs.uk' else '4112313.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112329.eng.nhs.uk' else '4112329.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4160575.eng.nhs.uk' else '4160575.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112857.eng.nhs.uk' else '4112857.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4112857%' and len('4112857') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',6902986.eng.nhs.uk' else '6902986.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762690.eng.nhs.uk' else '5762690.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762664.eng.nhs.uk' else '5762664.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044085.eng.nhs.uk' else '4044085.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112317.eng.nhs.uk' else '4112317.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044103.eng.nhs.uk' else '4044103.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4160570.eng.nhs.uk' else '4160570.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044141.eng.nhs.uk' else '4044141.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112855.eng.nhs.uk' else '4112855.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044155.eng.nhs.uk' else '4044155.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112858.eng.nhs.uk' else '4112858.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112326.eng.nhs.uk' else '4112326.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112314.eng.nhs.uk' else '4112314.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044108.eng.nhs.uk' else '4044108.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044086.eng.nhs.uk' else '4044086.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044176.eng.nhs.uk' else '4044176.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5677142.eng.nhs.uk' else '5677142.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112330.eng.nhs.uk' else '4112330.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044088.eng.nhs.uk' else '4044088.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044165.eng.nhs.uk' else '4044165.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112853.eng.nhs.uk' else '4112853.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044152.eng.nhs.uk' else '4044152.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4112321.eng.nhs.uk' else '4112321.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4108552.eng.nhs.uk' else '4108552.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044139.eng.nhs.uk' else '4044139.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044087.eng.nhs.uk' else '4044087.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sparsholt.ac.uk' else 'sparsholt.ac.uk' end WHERE [vchAthensOrgId] like '%2506264%' and len('2506264') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118185.akzonobel.com' else '66118185.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118185%' and len('66118185') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118188.akzonobel.com' else '66118188.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118188%' and len('66118188') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70253933.ccs.spokane.edu' else '70253933.ccs.spokane.edu' end WHERE [vchAthensOrgId] like '%70253933%' and len('70253933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70253932.ccs.spokane.edu' else '70253932.ccs.spokane.edu' end WHERE [vchAthensOrgId] like '%70253932%' and len('70253932') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924256.va.gov' else '54924256.va.gov' end WHERE [vchAthensOrgId] like '%54924256%' and len('54924256') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp2.springer.com' else 'sp2.springer.com' end WHERE [vchAthensOrgId] like '%55192751%' and len('55192751') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',springer.com' else 'springer.com' end WHERE [vchAthensOrgId] like '%5890544%' and len('5890544') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',samc.com' else 'samc.com' end WHERE [vchAthensOrgId] like '%69592519%' and len('69592519') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315900.eng.nhs.uk' else '4315900.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315900%' and len('4315900') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',youranswerplace.org' else 'youranswerplace.org' end WHERE [vchAthensOrgId] like '%70169883%' and len('70169883') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stchristophers.com' else 'stchristophers.com' end WHERE [vchAthensOrgId] like '%70211928%' and len('70211928') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710693.bshsi.org' else '53710693.bshsi.org' end WHERE [vchAthensOrgId] like '%53710693%' and len('53710693') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53710694.bshsi.org' else '53710694.bshsi.org' end WHERE [vchAthensOrgId] like '%53710694%' and len('53710694') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43610966.dhss.delaware.gov' else '43610966.dhss.delaware.gov' end WHERE [vchAthensOrgId] like '%43610966%' and len('43610966') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sfx.ac.uk' else 'sfx.ac.uk' end WHERE [vchAthensOrgId] like '%2555983%' and len('2555983') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223727.eng.nhs.uk' else '4223727.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223727%' and len('4223727') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044131.eng.nhs.uk' else '4044131.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044131%' and len('4044131') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sjhmc.org' else 'sjhmc.org' end WHERE [vchAthensOrgId] like '%69745941%' and len('69745941') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',dignityhealth.org' else 'dignityhealth.org' end WHERE [vchAthensOrgId] like '%69414233%' and len('69414233') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073174.eng.nhs.uk' else '4073174.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073174%' and len('4073174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stpatricksmarymount.ie' else 'stpatricksmarymount.ie' end WHERE [vchAthensOrgId] like '%17519438%' and len('17519438') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stvincents.com.au' else 'stvincents.com.au' end WHERE [vchAthensOrgId] like '%66025236%' and len('66025236') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stvincents.ie' else 'stvincents.ie' end WHERE [vchAthensOrgId] like '%15745573%' and len('15745573') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',standrews-de.org' else 'standrews-de.org' end WHERE [vchAthensOrgId] like '%69542489%' and len('69542489') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stcharles.ac.uk' else 'stcharles.ac.uk' end WHERE [vchAthensOrgId] like '%1764486%' and len('1764486') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stcharleshealthcare.org' else 'stcharleshealthcare.org' end WHERE [vchAthensOrgId] like '%69656738%' and len('69656738') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5640068.eng.nhs.uk' else '5640068.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5640068%' and len('5640068') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4303035.eng.nhs.uk' else '4303035.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4303035%' and len('4303035') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924275.va.gov' else '54924275.va.gov' end WHERE [vchAthensOrgId] like '%54924275%' and len('54924275') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sfh.ie' else 'sfh.ie' end WHERE [vchAthensOrgId] like '%70233901%' and len('70233901') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stg.hl.gov.au' else 'stg.hl.gov.au' end WHERE [vchAthensOrgId] like '%66722385%' and len('66722385') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073173.eng.nhs.uk' else '4073173.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073173%' and len('4073173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66096401.ascensionhealth.org' else '66096401.ascensionhealth.org' end WHERE [vchAthensOrgId] like '%66096401%' and len('66096401') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66096412.ascensionhealth.org' else '66096412.ascensionhealth.org' end WHERE [vchAthensOrgId] like '%66096412%' and len('66096412') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66096409.ascensionhealth.org' else '66096409.ascensionhealth.org' end WHERE [vchAthensOrgId] like '%66096409%' and len('66096409') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3696806.sjog.ie' else '3696806.sjog.ie' end WHERE [vchAthensOrgId] like '%3696806%' and len('3696806') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3636760.sjog.ie' else '3636760.sjog.ie' end WHERE [vchAthensOrgId] like '%3636760%' and len('3636760') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',65914729.ascensionhealth.org' else '65914729.ascensionhealth.org' end WHERE [vchAthensOrgId] like '%65914729%' and len('65914729') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69366509.hshs.org' else '69366509.hshs.org' end WHERE [vchAthensOrgId] like '%69366509%' and len('69366509') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stjoeshealth.org' else 'stjoeshealth.org' end WHERE [vchAthensOrgId] like '%67977293%' and len('67977293') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924235.va.gov' else '54924235.va.gov' end WHERE [vchAthensOrgId] like '%54924235%' and len('54924235') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68842254.saintlukeshealthsystem.org' else '68842254.saintlukeshealthsystem.org' end WHERE [vchAthensOrgId] like '%68842254%' and len('68842254') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',48670263.hse.ie' else '48670263.hse.ie' end WHERE [vchAthensOrgId] like '%48670263%' and len('48670263') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stmaryhealthcare.org' else 'stmaryhealthcare.org' end WHERE [vchAthensOrgId] like '%12776247%' and len('12776247') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ssmhc.com' else 'ssmhc.com' end WHERE [vchAthensOrgId] like '%12776216%' and len('12776216') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3696752.sjog.ie' else '3696752.sjog.ie' end WHERE [vchAthensOrgId] like '%3696752%' and len('3696752') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',svhf.ie' else 'svhf.ie' end WHERE [vchAthensOrgId] like '%60408408%' and len('60408408') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sxccal.edu' else 'sxccal.edu' end WHERE [vchAthensOrgId] like '%70004292%' and len('70004292') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69555239.oaebsco.com' else '69555239.oaebsco.com' end WHERE [vchAthensOrgId] like '%69555239%' and len('69555239') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70026542.oaebsco.com' else '70026542.oaebsco.com' end WHERE [vchAthensOrgId] like '%70026542%' and len('70026542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stacksdiscovery.com' else 'stacksdiscovery.com' end WHERE [vchAthensOrgId] like '%70041008%' and len('70041008') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stacks.com' else 'stacks.com' end WHERE [vchAthensOrgId] like '%70357933%' and len('70357933') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195537.eng.nhs.uk' else '5195537.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195537%' and len('5195537') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195541.eng.nhs.uk' else '5195541.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195541%' and len('5195541') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sbp.org.pk' else 'sbp.org.pk' end WHERE [vchAthensOrgId] like '%69390532%' and len('69390532') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4130229.scot.nhs.uk' else '4130229.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%4130229%' and len('4130229') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139934.dtf.vic.gov.au' else '68139934.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139934%' and len('68139934') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523142.swanlibraries.net' else '69523142.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523142%' and len('69523142') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stephensoncoll.ac.uk' else 'stephensoncoll.ac.uk' end WHERE [vchAthensOrgId] like '%2636918%' and len('2636918') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3683206.sjog.ie' else '3683206.sjog.ie' end WHERE [vchAthensOrgId] like '%3683206%' and len('3683206') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523143.swanlibraries.net' else '69523143.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523143%' and len('69523143') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044180.eng.nhs.uk' else '4044180.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044180%' and len('4044180') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stockton.ac.uk' else 'stockton.ac.uk' end WHERE [vchAthensOrgId] like '%1826949%' and len('1826949') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stonegroup.co.uk' else 'stonegroup.co.uk' end WHERE [vchAthensOrgId] like '%70113475%' and len('70113475') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',stonefish.co.uk' else 'stonefish.co.uk' end WHERE [vchAthensOrgId] like '%70085570%' and len('70085570') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',studygroup.com' else 'studygroup.com' end WHERE [vchAthensOrgId] like '%70137964%' and len('70137964') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',snp.com.au' else 'snp.com.au' end WHERE [vchAthensOrgId] like '%38441490%' and len('38441490') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620196.ahsl.arizona.edu' else '12620196.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620196%' and len('12620196') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523144.swanlibraries.net' else '69523144.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523144%' and len('69523144') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118197.akzonobel.com' else '66118197.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118197%' and len('66118197') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118195.akzonobel.com' else '66118195.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118195%' and len('66118195') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118196.akzonobel.com' else '66118196.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118196%' and len('66118196') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223639.eng.nhs.uk' else '4223639.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223639%' and len('4223639') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223623.eng.nhs.uk' else '4223623.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223623%' and len('4223623') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223627.eng.nhs.uk' else '4223627.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223627%' and len('4223627') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223642.eng.nhs.uk' else '4223642.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223642%' and len('4223642') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223641.eng.nhs.uk' else '4223641.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223641%' and len('4223641') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',seslhd.health.nsw.gov.au' else 'seslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%70040821%' and len('70040821') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69862741%' and len('69862741') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762715.eng.nhs.uk' else '5762715.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762715%' and len('5762715') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',swanlibraries.net' else 'swanlibraries.net' end WHERE [vchAthensOrgId] like '%68879752%' and len('68879752') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',swan.ac.uk' else 'swan.ac.uk' end WHERE [vchAthensOrgId] like '%250%' and len('250') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',swslhd.nsw.gov.au' else 'swslhd.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54406029%' and len('54406029') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',schn.health.nsw.gov.au' else 'schn.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%66724929%' and len('66724929') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924181.va.gov' else '54924181.va.gov' end WHERE [vchAthensOrgId] like '%54924181%' and len('54924181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044177.eng.nhs.uk' else '4044177.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044177%' and len('4044177') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tksor.no' else 'tksor.no' end WHERE [vchAthensOrgId] like '%70086110%' and len('70086110') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073232.eng.nhs.uk' else '4073232.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073232%' and len('4073232') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762763.eng.nhs.uk' else '5762763.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762763%' and len('5762763') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531152.eng.nhs.uk' else '5531152.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531152%' and len('5531152') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sp1.tandf.co.uk' else 'sp1.tandf.co.uk' end WHERE [vchAthensOrgId] like '%4180590%' and len('4180590') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',taylorandfrancis.com' else 'taylorandfrancis.com' end WHERE [vchAthensOrgId] like '%12726344%' and len('12726344') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70080256.sp1.tandf.co.uk' else '70080256.sp1.tandf.co.uk' end WHERE [vchAthensOrgId] like '%4180590%' and len('4180590') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',taylors.edu.my' else 'taylors.edu.my' end WHERE [vchAthensOrgId] like '%69422392%' and len('69422392') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tdnet.teldan.com' else 'tdnet.teldan.com' end WHERE [vchAthensOrgId] like '%5953522%' and len('5953522') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058652.eng.nhs.uk' else '4058652.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058652%' and len('4058652') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38213947.teldan.com' else '38213947.teldan.com' end WHERE [vchAthensOrgId] like '%38213947%' and len('38213947') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tcat.ac.uk' else 'tcat.ac.uk' end WHERE [vchAthensOrgId] like '%67611633%' and len('67611633') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924212.va.gov' else '54924212.va.gov' end WHERE [vchAthensOrgId] like '%54924212%' and len('54924212') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5904433.eng.nhs.uk' else '5904433.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5904433%' and len('5904433') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',3201184.scot.nhs.uk' else '3201184.scot.nhs.uk' end WHERE [vchAthensOrgId] like '%68223085%' and len('68223085') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',53724127.tetondata.com' else '53724127.tetondata.com' end WHERE [vchAthensOrgId] like '%53724127%' and len('53724127') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5674997.tetondata.com' else '5674997.tetondata.com' end WHERE [vchAthensOrgId] like '%5674997%' and len('5674997') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66757234.teva.co.il' else '66757234.teva.co.il' end WHERE [vchAthensOrgId] like '%66757234%' and len('66757234') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',library.tmc.edu' else 'library.tmc.edu' end WHERE [vchAthensOrgId] like '%69656786%' and len('69656786') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tsrh.org' else 'tsrh.org' end WHERE [vchAthensOrgId] like '%67629058%' and len('67629058') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12954811.eng.nhs.uk' else '12954811.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%12954811%' and len('12954811') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12906702.eng.nhs.uk' else '12906702.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%12906702%' and len('12906702') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',browning.edu' else 'browning.edu' end WHERE [vchAthensOrgId] like '%70100260%' and len('70100260') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',Cefas.co.uk' else 'Cefas.co.uk' end WHERE [vchAthensOrgId] like '%68798504%' and len('68798504') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thechildrenstrust.org.uk' else 'thechildrenstrust.org.uk' end WHERE [vchAthensOrgId] like '%5319951%' and len('5319951') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044172.eng.nhs.uk' else '4044172.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044172%' and len('4044172') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',scie.org.uk' else 'scie.org.uk' end WHERE [vchAthensOrgId] like '%66658832%' and len('66658832') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',col-westanglia.ac.uk' else 'col-westanglia.ac.uk' end WHERE [vchAthensOrgId] like '%2617346%' and len('2617346') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68189799.eng.nhs.uk' else '68189799.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%68189799%' and len('68189799') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531199.eng.nhs.uk' else '5531199.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531199%' and len('5531199') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',jointcommission.org' else 'jointcommission.org' end WHERE [vchAthensOrgId] like '%69432840%' and len('69432840') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54341675.eng.nhs.uk' else '54341675.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%54341675%' and len('54341675') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',theoncallroom.com' else 'theoncallroom.com' end WHERE [vchAthensOrgId] like '%70143023%' and len('70143023') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',usoncology.com' else 'usoncology.com' end WHERE [vchAthensOrgId] like '%66055542%' and len('66055542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',royalchildrenshospital.gov.au' else 'royalchildrenshospital.gov.au' end WHERE [vchAthensOrgId] like '%54714375%' and len('54714375') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',royalvictorianeyeandear.gov.au' else 'royalvictorianeyeandear.gov.au' end WHERE [vchAthensOrgId] like '%54714374%' and len('54714374') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',colchsfc.ac.uk' else 'colchsfc.ac.uk' end WHERE [vchAthensOrgId] like '%1446311%' and len('1446311') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',farnboroughsfc.ac.uk' else 'farnboroughsfc.ac.uk' end WHERE [vchAthensOrgId] like '%1677682%' and len('1677682') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',farnboroughsfc.ac.uk' else 'farnboroughsfc.ac.uk' end WHERE [vchAthensOrgId] like '%1677682%' and len('1677682') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54520575.ncahs.health.nsw.gov.au' else '54520575.ncahs.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%54520575%' and len('54520575') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',law.ac.uk' else 'law.ac.uk' end WHERE [vchAthensOrgId] like '%70232412%' and len('70232412') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4160572.eng.nhs.uk' else '4160572.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4160572%' and len('4160572') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thermofisher.com' else 'thermofisher.com' end WHERE [vchAthensOrgId] like '%68095292%' and len('68095292') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thieme.com' else 'thieme.com' end WHERE [vchAthensOrgId] like '%67908577%' and len('67908577') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thirdiron.com' else 'thirdiron.com' end WHERE [vchAthensOrgId] like '%69402452%' and len('69402452') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523145.swanlibraries.net' else '69523145.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523145%' and len('69523145') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',cengage.co.uk' else 'cengage.co.uk' end WHERE [vchAthensOrgId] like '%15921999%' and len('15921999') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',thomsonreuters.com' else 'thomsonreuters.com' end WHERE [vchAthensOrgId] like '%1761185%' and len('1761185') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4220505.webofknowledge.com' else '4220505.webofknowledge.com' end WHERE [vchAthensOrgId] like '%4220505%' and len('4220505') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',7954339.webofknowledge.com' else '7954339.webofknowledge.com' end WHERE [vchAthensOrgId] like '%7954339%' and len('7954339') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',32580726.webofknowledge.com' else '32580726.webofknowledge.com' end WHERE [vchAthensOrgId] like '%32580726%' and len('32580726') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43637396.webofknowledge.com' else '43637396.webofknowledge.com' end WHERE [vchAthensOrgId] like '%43637396%' and len('43637396') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523146.swanlibraries.net' else '69523146.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523146%' and len('69523146') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43636392.eng.nhs.uk' else '43636392.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%43636392%' and len('43636392') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70026453.oaebsco.com' else '70026453.oaebsco.com' end WHERE [vchAthensOrgId] like '%70026453%' and len('70026453') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523147.swanlibraries.net' else '69523147.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523147%' and len('69523147') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620206.ahsl.arizona.edu' else '12620206.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620206%' and len('12620206') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924176.va.gov' else '54924176.va.gov' end WHERE [vchAthensOrgId] like '%54924176%' and len('54924176') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',promedica.org' else 'promedica.org' end WHERE [vchAthensOrgId] like '%69475724%' and len('69475724') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924231.va.gov' else '54924231.va.gov' end WHERE [vchAthensOrgId] like '%54924231%' and len('54924231') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762729.eng.nhs.uk' else '5762729.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762729%' and len('5762729') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70220142.totemguard.com' else '70220142.totemguard.com' end WHERE [vchAthensOrgId] like '%70220142%' and len('70220142') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',totemguard.com' else 'totemguard.com' end WHERE [vchAthensOrgId] like '%67068871%' and len('67068871') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70220241.totemguard.com' else '70220241.totemguard.com' end WHERE [vchAthensOrgId] like '%70220241%' and len('70220241') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69504516.totemguard.com' else '69504516.totemguard.com' end WHERE [vchAthensOrgId] like '%69504516%' and len('69504516') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tower.ac.uk' else 'tower.ac.uk' end WHERE [vchAthensOrgId] like '%2723756%' and len('2723756') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqa.openathens.net' else 'qqa.openathens.net' end WHERE [vchAthensOrgId] like '%69880433%' and len('69880433') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqb.openathens.net' else 'qqb.openathens.net' end WHERE [vchAthensOrgId] like '%69880438%' and len('69880438') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqc.openathens.net' else 'qqc.openathens.net' end WHERE [vchAthensOrgId] like '%69880446%' and len('69880446') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqd.openathens.net' else 'qqd.openathens.net' end WHERE [vchAthensOrgId] like '%69880449%' and len('69880449') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqe.openathens.net' else 'qqe.openathens.net' end WHERE [vchAthensOrgId] like '%69880455%' and len('69880455') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqf.openathens.net' else 'qqf.openathens.net' end WHERE [vchAthensOrgId] like '%69880460%' and len('69880460') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqg.openathens.net' else 'qqg.openathens.net' end WHERE [vchAthensOrgId] like '%69880469%' and len('69880469') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqh.openathens.net' else 'qqh.openathens.net' end WHERE [vchAthensOrgId] like '%69880473%' and len('69880473') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqi.openathens.net' else 'qqi.openathens.net' end WHERE [vchAthensOrgId] like '%69880477%' and len('69880477') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqj.openathens.net' else 'qqj.openathens.net' end WHERE [vchAthensOrgId] like '%69880480%' and len('69880480') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qqk.openathens.net' else 'qqk.openathens.net' end WHERE [vchAthensOrgId] like '%69880483%' and len('69880483') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',qql.openathens.net' else 'qql.openathens.net' end WHERE [vchAthensOrgId] like '%69880494%' and len('69880494') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139939.dtf.vic.gov.au' else '68139939.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139939%' and len('68139939') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',trinityhealth.com' else 'trinityhealth.com' end WHERE [vchAthensOrgId] like '%70018632%' and len('70018632') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercyhealth.com' else 'mercyhealth.com' end WHERE [vchAthensOrgId] like '%69575581%' and len('69575581') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sjmed.com' else 'sjmed.com' end WHERE [vchAthensOrgId] like '%70202686%' and len('70202686') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tcm.ac.uk' else 'tcm.ac.uk' end WHERE [vchAthensOrgId] like '%4017296%' and len('4017296') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',mercyhealth2.com' else 'mercyhealth2.com' end WHERE [vchAthensOrgId] like '%69592542%' and len('69592542') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54086849.us.army.mil' else '54086849.us.army.mil' end WHERE [vchAthensOrgId] like '%54086849%' and len('54086849') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',truvenhealth.com' else 'truvenhealth.com' end WHERE [vchAthensOrgId] like '%5398941%' and len('5398941') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68931357.tso.co.uk' else '68931357.tso.co.uk' end WHERE [vchAthensOrgId] like '%68931357%' and len('68931357') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38173183.tso.co.uk' else '38173183.tso.co.uk' end WHERE [vchAthensOrgId] like '%38173183%' and len('38173183') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924200.va.gov' else '54924200.va.gov' end WHERE [vchAthensOrgId] like '%54924200%' and len('54924200') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70181304.hse.ie' else '70181304.hse.ie' end WHERE [vchAthensOrgId] like '%70181304%' and len('70181304') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69884227%' and len('69884227') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',tynemet.ac.uk' else 'tynemet.ac.uk' end WHERE [vchAthensOrgId] like '%7966713%' and len('7966713') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',amedd.army.mil' else 'amedd.army.mil' end WHERE [vchAthensOrgId] like '%66749238%' and len('66749238') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',us.army.research.institute' else 'us.army.research.institute' end WHERE [vchAthensOrgId] like '%43611322%' and len('43611322') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ubm.com' else 'ubm.com' end WHERE [vchAthensOrgId] like '%5510197%' and len('5510197') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70053922.ubm.com' else '70053922.ubm.com' end WHERE [vchAthensOrgId] like '%70053922%' and len('70053922') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ucsiuniversity.edu.my' else 'ucsiuniversity.edu.my' end WHERE [vchAthensOrgId] like '%70110822%' and len('70110822') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uhv.edu' else 'uhv.edu' end WHERE [vchAthensOrgId] like '%70006166%' and len('70006166') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lords.parliament.uk' else 'lords.parliament.uk' end WHERE [vchAthensOrgId] like '%2282399%' and len('2282399') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',umu.se' else 'umu.se' end WHERE [vchAthensOrgId] like '%70267344%' and len('70267344') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',upsem.edu' else 'upsem.edu' end WHERE [vchAthensOrgId] like '%70068102%' and len('70068102') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69758136%' and len('69758136') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315978.eng.nhs.uk' else '4315978.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315978%' and len('4315978') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uwcsea.edu.sg' else 'uwcsea.edu.sg' end WHERE [vchAthensOrgId] like '%69706802%' and len('69706802') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uai.edu.ar' else 'uai.edu.ar' end WHERE [vchAthensOrgId] like '%70226388%' and len('70226388') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69987794%' and len('69987794') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69987784%' and len('69987784') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69987774%' and len('69987774') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uninorte.edu.co' else 'uninorte.edu.co' end WHERE [vchAthensOrgId] like '%70397478%' and len('70397478') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uniquindio.edu.co' else 'uniquindio.edu.co' end WHERE [vchAthensOrgId] like '%70072675%' and len('70072675') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sagrado.edu' else 'sagrado.edu' end WHERE [vchAthensOrgId] like '%70226815%' and len('70226815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ibero.mx' else 'ibero.mx' end WHERE [vchAthensOrgId] like '%70215401%' and len('70215401') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',untref.edu.ar' else 'untref.edu.ar' end WHERE [vchAthensOrgId] like '%69574700%' and len('69574700') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',unphu.edu.do' else 'unphu.edu.do' end WHERE [vchAthensOrgId] like '%70389050%' and len('70389050') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',upeu.edu.pe' else 'upeu.edu.pe' end WHERE [vchAthensOrgId] like '%69579578%' and len('69579578') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ufpr.br' else 'ufpr.br' end WHERE [vchAthensOrgId] like '%70365556%' and len('70365556') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',undip.ac.id' else 'undip.ac.id' end WHERE [vchAthensOrgId] like '%68966996%' and len('68966996') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',unj.ac.id' else 'unj.ac.id' end WHERE [vchAthensOrgId] like '%69498416%' and len('69498416') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',umk.edu.my' else 'umk.edu.my' end WHERE [vchAthensOrgId] like '%66536847%' and len('66536847') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',usm.my' else 'usm.my' end WHERE [vchAthensOrgId] like '%70180632%' and len('70180632') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',petronas.com.my' else 'petronas.com.my' end WHERE [vchAthensOrgId] like '%66672519%' and len('66672519') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ucb.ac.uk' else 'ucb.ac.uk' end WHERE [vchAthensOrgId] like '%2263469%' and len('2263469') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531153.eng.nhs.uk' else '5531153.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531153%' and len('5531153') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ucreative.ac.uk' else 'ucreative.ac.uk' end WHERE [vchAthensOrgId] like '%7496615%' and len('7496615') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044174.eng.nhs.uk' else '4044174.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044174%' and len('4044174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223579.eng.nhs.uk' else '4223579.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223579%' and len('4223579') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195507.eng.nhs.uk' else '5195507.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195507%' and len('5195507') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762691.eng.nhs.uk' else '5762691.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762691%' and len('5762691') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195525.eng.nhs.uk' else '5195525.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195525%' and len('5195525') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4315904.eng.nhs.uk' else '4315904.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4315904%' and len('4315904') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044093.eng.nhs.uk' else '4044093.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044093%' and len('4044093') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195540.eng.nhs.uk' else '5195540.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195540%' and len('5195540') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uasd.edu' else 'uasd.edu' end WHERE [vchAthensOrgId] like '%70191839%' and len('70191839') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',brad.ac.uk' else 'brad.ac.uk' end WHERE [vchAthensOrgId] like '%213%' and len('213') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',brad.ac.uk' else 'brad.ac.uk' end WHERE [vchAthensOrgId] like '%68116506%' and len('68116506') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ubc.ca' else 'ubc.ca' end WHERE [vchAthensOrgId] like '%70388853%' and len('70388853') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',buckingham.ac.uk' else 'buckingham.ac.uk' end WHERE [vchAthensOrgId] like '%226%' and len('226') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',chester.ac.uk' else 'chester.ac.uk' end WHERE [vchAthensOrgId] like '%126%' and len('126') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ud.ac.ae' else 'ud.ac.ae' end WHERE [vchAthensOrgId] like '%54359608%' and len('54359608') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uea.ac.uk' else 'uea.ac.uk' end WHERE [vchAthensOrgId] like '%188%' and len('188') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uel.ac.uk' else 'uel.ac.uk' end WHERE [vchAthensOrgId] like '%189%' and len('189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ueh.edu.vn' else 'ueh.edu.vn' end WHERE [vchAthensOrgId] like '%70226879%' and len('70226879') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',gre.ac.uk' else 'gre.ac.uk' end WHERE [vchAthensOrgId] like '%150%' and len('150') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uog.edu' else 'uog.edu' end WHERE [vchAthensOrgId] like '%70203800%' and len('70203800') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',herts.ac.uk' else 'herts.ac.uk' end WHERE [vchAthensOrgId] like '%225%' and len('225') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',hud.ac.uk' else 'hud.ac.uk' end WHERE [vchAthensOrgId] like '%153%' and len('153') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',keele.ac.uk' else 'keele.ac.uk' end WHERE [vchAthensOrgId] like '%129%' and len('129') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',leeds.ac.uk' else 'leeds.ac.uk' end WHERE [vchAthensOrgId] like '%70118439%' and len('70118439') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',le.ac.uk' else 'le.ac.uk' end WHERE [vchAthensOrgId] like '%124%' and len('124') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ulab.edu.bd' else 'ulab.edu.bd' end WHERE [vchAthensOrgId] like '%69565041%' and len('69565041') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',lincoln.ac.uk' else 'lincoln.ac.uk' end WHERE [vchAthensOrgId] like '%228%' and len('228') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',liverpool.ac.uk' else 'liverpool.ac.uk' end WHERE [vchAthensOrgId] like '%70281202%' and len('70281202') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',londonexternal.ac.uk' else 'londonexternal.ac.uk' end WHERE [vchAthensOrgId] like '%150912%' and len('150912') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',northampton.ac.uk' else 'northampton.ac.uk' end WHERE [vchAthensOrgId] like '%4700697%' and len('4700697') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',oa.port.ac.uk' else 'oa.port.ac.uk' end WHERE [vchAthensOrgId] like '%68552812%' and len('68552812') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',reading.ac.uk' else 'reading.ac.uk' end WHERE [vchAthensOrgId] like '%116%' and len('116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',roehampton.ac.uk' else 'roehampton.ac.uk' end WHERE [vchAthensOrgId] like '%240%' and len('240') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',salford.ac.uk' else 'salford.ac.uk' end WHERE [vchAthensOrgId] like '%131%' and len('131') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sharjah.ac.ae' else 'sharjah.ac.ae' end WHERE [vchAthensOrgId] like '%54300174%' and len('54300174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',sunderland.ac.uk' else 'sunderland.ac.uk' end WHERE [vchAthensOrgId] like '%128%' and len('128') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',surrey.ac.uk' else 'surrey.ac.uk' end WHERE [vchAthensOrgId] like '%184%' and len('184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uwe.ac.uk' else 'uwe.ac.uk' end WHERE [vchAthensOrgId] like '%192%' and len('192') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',uws.ac.uk' else 'uws.ac.uk' end WHERE [vchAthensOrgId] like '%38298747%' and len('38298747') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ulster.ac.uk' else 'ulster.ac.uk' end WHERE [vchAthensOrgId] like '%246%' and len('246') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',smu.ac.uk' else 'smu.ac.uk' end WHERE [vchAthensOrgId] like '%223%' and len('223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',warwick.ac.uk' else 'warwick.ac.uk' end WHERE [vchAthensOrgId] like '%193%' and len('193') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',winchester.ac.uk' else 'winchester.ac.uk' end WHERE [vchAthensOrgId] like '%256%' and len('256') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wlv.ac.uk' else 'wlv.ac.uk' end WHERE [vchAthensOrgId] like '%120%' and len('120') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',worc.ac.uk' else 'worc.ac.uk' end WHERE [vchAthensOrgId] like '%427192%' and len('427192') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38537211.eng.nhs.uk' else '38537211.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%69350443%' and len('69350443') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523148.swanlibraries.net' else '69523148.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523148%' and len('69523148') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66333789.uptodate.com' else '66333789.uptodate.com' end WHERE [vchAthensOrgId] like '%66333789%' and len('66333789') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66619894.us.army.mil' else '66619894.us.army.mil' end WHERE [vchAthensOrgId] like '%66619894%' and len('66619894') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66619925.us.army.mil' else '66619925.us.army.mil' end WHERE [vchAthensOrgId] like '%66619925%' and len('66619925') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66588438.us.army.mil' else '66588438.us.army.mil' end WHERE [vchAthensOrgId] like '%66588438%' and len('66588438') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66636040.us.army.mil' else '66636040.us.army.mil' end WHERE [vchAthensOrgId] like '%66636040%' and len('66636040') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66619934.us.army.mil' else '66619934.us.army.mil' end WHERE [vchAthensOrgId] like '%66619934%' and len('66619934') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66619984.us.army.mil' else '66619984.us.army.mil' end WHERE [vchAthensOrgId] like '%66619984%' and len('66619984') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924218.va.gov' else '54924218.va.gov' end WHERE [vchAthensOrgId] like '%54924218%' and len('54924218') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924270.va.gov' else '54924270.va.gov' end WHERE [vchAthensOrgId] like '%54924270%' and len('54924270') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924259.va.gov' else '54924259.va.gov' end WHERE [vchAthensOrgId] like '%54924259%' and len('54924259') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924271.va.gov' else '54924271.va.gov' end WHERE [vchAthensOrgId] like '%54924271%' and len('54924271') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924172.va.gov' else '54924172.va.gov' end WHERE [vchAthensOrgId] like '%54924172%' and len('54924172') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924246.va.gov' else '54924246.va.gov' end WHERE [vchAthensOrgId] like '%54924246%' and len('54924246') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924233.va.gov' else '54924233.va.gov' end WHERE [vchAthensOrgId] like '%54924233%' and len('54924233') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924269.va.gov' else '54924269.va.gov' end WHERE [vchAthensOrgId] like '%54924269%' and len('54924269') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43633425.va.gov' else '43633425.va.gov' end WHERE [vchAthensOrgId] like '%43633425%' and len('43633425') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924220.va.gov' else '54924220.va.gov' end WHERE [vchAthensOrgId] like '%54924220%' and len('54924220') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924266.va.gov' else '54924266.va.gov' end WHERE [vchAthensOrgId] like '%54924266%' and len('54924266') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924267.va.gov' else '54924267.va.gov' end WHERE [vchAthensOrgId] like '%54924267%' and len('54924267') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43660215.va.gov' else '43660215.va.gov' end WHERE [vchAthensOrgId] like '%43660215%' and len('43660215') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924248.va.gov' else '54924248.va.gov' end WHERE [vchAthensOrgId] like '%54924248%' and len('54924248') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924274.va.gov' else '54924274.va.gov' end WHERE [vchAthensOrgId] like '%54924274%' and len('54924274') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924244.va.gov' else '54924244.va.gov' end WHERE [vchAthensOrgId] like '%54924244%' and len('54924244') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924261.va.gov' else '54924261.va.gov' end WHERE [vchAthensOrgId] like '%54924261%' and len('54924261') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924223.va.gov' else '54924223.va.gov' end WHERE [vchAthensOrgId] like '%54924223%' and len('54924223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924260.va.gov' else '54924260.va.gov' end WHERE [vchAthensOrgId] like '%54924260%' and len('54924260') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924262.va.gov' else '54924262.va.gov' end WHERE [vchAthensOrgId] like '%54924262%' and len('54924262') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924190.va.gov' else '54924190.va.gov' end WHERE [vchAthensOrgId] like '%54924190%' and len('54924190') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924254.va.gov' else '54924254.va.gov' end WHERE [vchAthensOrgId] like '%54924254%' and len('54924254') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924255.va.gov' else '54924255.va.gov' end WHERE [vchAthensOrgId] like '%54924255%' and len('54924255') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924249.va.gov' else '54924249.va.gov' end WHERE [vchAthensOrgId] like '%54924249%' and len('54924249') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924268.va.gov' else '54924268.va.gov' end WHERE [vchAthensOrgId] like '%54924268%' and len('54924268') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924263.va.gov' else '54924263.va.gov' end WHERE [vchAthensOrgId] like '%54924263%' and len('54924263') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924265.va.gov' else '54924265.va.gov' end WHERE [vchAthensOrgId] like '%54924265%' and len('54924265') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924258.va.gov' else '54924258.va.gov' end WHERE [vchAthensOrgId] like '%54924258%' and len('54924258') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924276.va.gov' else '54924276.va.gov' end WHERE [vchAthensOrgId] like '%54924276%' and len('54924276') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66127958.va.gov' else '66127958.va.gov' end WHERE [vchAthensOrgId] like '%66127958%' and len('66127958') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922171.va.gov' else '54922171.va.gov' end WHERE [vchAthensOrgId] like '%54922171%' and len('54922171') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922172.va.gov' else '54922172.va.gov' end WHERE [vchAthensOrgId] like '%54922172%' and len('54922172') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922173.va.gov' else '54922173.va.gov' end WHERE [vchAthensOrgId] like '%54922173%' and len('54922173') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38510882.va.gov' else '38510882.va.gov' end WHERE [vchAthensOrgId] like '%38510882%' and len('38510882') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38507236.va.gov' else '38507236.va.gov' end WHERE [vchAthensOrgId] like '%38507236%' and len('38507236') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922174.va.gov' else '54922174.va.gov' end WHERE [vchAthensOrgId] like '%54922174%' and len('54922174') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54369335.va.gov' else '54369335.va.gov' end WHERE [vchAthensOrgId] like '%54369335%' and len('54369335') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922175.va.gov' else '54922175.va.gov' end WHERE [vchAthensOrgId] like '%54922175%' and len('54922175') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922176.va.gov' else '54922176.va.gov' end WHERE [vchAthensOrgId] like '%54922176%' and len('54922176') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922178.va.gov' else '54922178.va.gov' end WHERE [vchAthensOrgId] like '%54922178%' and len('54922178') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922179.va.gov' else '54922179.va.gov' end WHERE [vchAthensOrgId] like '%54922179%' and len('54922179') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',43630101.va.gov' else '43630101.va.gov' end WHERE [vchAthensOrgId] like '%43630101%' and len('43630101') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922180.va.gov' else '54922180.va.gov' end WHERE [vchAthensOrgId] like '%54922180%' and len('54922180') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922181.va.gov' else '54922181.va.gov' end WHERE [vchAthensOrgId] like '%54922181%' and len('54922181') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922182.va.gov' else '54922182.va.gov' end WHERE [vchAthensOrgId] like '%54922182%' and len('54922182') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922183.va.gov' else '54922183.va.gov' end WHERE [vchAthensOrgId] like '%54922183%' and len('54922183') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922184.va.gov' else '54922184.va.gov' end WHERE [vchAthensOrgId] like '%54922184%' and len('54922184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922185.va.gov' else '54922185.va.gov' end WHERE [vchAthensOrgId] like '%54922185%' and len('54922185') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924182.va.gov' else '54924182.va.gov' end WHERE [vchAthensOrgId] like '%54924182%' and len('54924182') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924202.va.gov' else '54924202.va.gov' end WHERE [vchAthensOrgId] like '%54924202%' and len('54924202') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5415189.wales.nhs.uk' else '5415189.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%5415189%' and len('5415189') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54922186.va.gov' else '54922186.va.gov' end WHERE [vchAthensOrgId] like '%54922186%' and len('54922186') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139938.dtf.vic.gov.au' else '68139938.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139938%' and len('68139938') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139900.dtf.vic.gov.au' else '68139900.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139927%' and len('68139927') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',vtc.vt.edu' else 'vtc.vt.edu' end WHERE [vchAthensOrgId] like '%67608562%' and len('67608562') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',virtua.org' else 'virtua.org' end WHERE [vchAthensOrgId] like '%66158132%' and len('66158132') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',visva-bharati.ac.in' else 'visva-bharati.ac.in' end WHERE [vchAthensOrgId] like '%68655896%' and len('68655896') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wageningenacademic.com' else 'wageningenacademic.com' end WHERE [vchAthensOrgId] like '%69639083%' and len('69639083') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',waikatodhb.health.nz' else 'waikatodhb.health.nz' end WHERE [vchAthensOrgId] like '%15949609%' and len('15949609') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',67432999.eng.nhs.uk' else '67432999.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%67432999%' and len('67432999') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195508.eng.nhs.uk' else '5195508.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195508%' and len('5195508') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',degruyter.com' else 'degruyter.com' end WHERE [vchAthensOrgId] like '%66616654%' and len('66616654') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66619975.us.army.mil' else '66619975.us.army.mil' end WHERE [vchAthensOrgId] like '%66619975%' and len('66619975') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wsu.ac.za' else 'wsu.ac.za' end WHERE [vchAthensOrgId] like '%69640131%' and len('69640131') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',waltham.ac.uk' else 'waltham.ac.uk' end WHERE [vchAthensOrgId] like '%2422452%' and len('2422452') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',londonexternal.ac.uk' else 'londonexternal.ac.uk' end WHERE [vchAthensOrgId] like '%4924926%' and len('4924926') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044145.eng.nhs.uk' else '4044145.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044145%' and len('4044145') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',38519779.va.gov' else '38519779.va.gov' end WHERE [vchAthensOrgId] like '%38519779%' and len('38519779') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',13671011.wales.nhs.uk' else '13671011.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%13671011%' and len('13671011') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12615712.wales.nhs.uk' else '12615712.wales.nhs.uk' end WHERE [vchAthensOrgId] like '%12615712%' and len('12615712') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12954814.eng.nhs.uk' else '12954814.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%12954814%' and len('12954814') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70019630.communitylibrary.net' else '70019630.communitylibrary.net' end WHERE [vchAthensOrgId] like '%70019630%' and len('70019630') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',westcollegescotland.ac.uk' else 'westcollegescotland.ac.uk' end WHERE [vchAthensOrgId] like '%67529802%' and len('67529802') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073184.eng.nhs.uk' else '4073184.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073184%' and len('4073184') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66844205.eng.nhs.uk' else '66844205.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66844205%' and len('66844205') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531217.eng.nhs.uk' else '5531217.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531217%' and len('5531217') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',west-lothian.ac.uk' else 'west-lothian.ac.uk' end WHERE [vchAthensOrgId] like '%1786977%' and len('1786977') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5184096.eng.nhs.uk' else '5184096.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5184096%' and len('5184096') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195520.eng.nhs.uk' else '5195520.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195520%' and len('5195520') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924205.va.gov' else '54924205.va.gov' end WHERE [vchAthensOrgId] like '%54924205%' and len('54924205') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wsc.ac.uk' else 'wsc.ac.uk' end WHERE [vchAthensOrgId] like '%68133642%' and len('68133642') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073214.eng.nhs.uk' else '4073214.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4073214%' and len('4073214') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924238.va.gov' else '54924238.va.gov' end WHERE [vchAthensOrgId] like '%54924238%' and len('54924238') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',west-thames.ac.uk' else 'west-thames.ac.uk' end WHERE [vchAthensOrgId] like '%2330918%' and len('2330918') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523149.swanlibraries.net' else '69523149.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523149%' and len('69523149') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4223624.eng.nhs.uk' else '4223624.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4223624%' and len('4223624') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wslhd.health.nsw.gov.au' else 'wslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%68921605%' and len('68921605') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54555469.thomsonreuters.com' else '54555469.thomsonreuters.com' end WHERE [vchAthensOrgId] like '%54555469%' and len('54555469') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69032261.wslhd.health.nsw.gov.au' else '69032261.wslhd.health.nsw.gov.au' end WHERE [vchAthensOrgId] like '%69032261%' and len('69032261') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523150.swanlibraries.net' else '69523150.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523150%' and len('69523150') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762692.eng.nhs.uk' else '5762692.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762692%' and len('5762692') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',weston.org' else 'weston.org' end WHERE [vchAthensOrgId] like '%70208815%' and len('70208815') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',70226686.ccsnh.edu' else '70226686.ccsnh.edu' end WHERE [vchAthensOrgId] like '%70226686%' and len('70226686') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wphospital.org' else 'wphospital.org' end WHERE [vchAthensOrgId] like '%67492780%' and len('67492780') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924177.va.gov' else '54924177.va.gov' end WHERE [vchAthensOrgId] like '%54924177%' and len('54924177') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',whitefield.waltham.sch.uk' else 'whitefield.waltham.sch.uk' end WHERE [vchAthensOrgId] like '%67073811%' and len('67073811') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5531154.eng.nhs.uk' else '5531154.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5531154%' and len('5531154') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',commonwealthhealth.net' else 'commonwealthhealth.net' end WHERE [vchAthensOrgId] like '%70157026%' and len('70157026') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924191.va.gov' else '54924191.va.gov' end WHERE [vchAthensOrgId] like '%54924191%' and len('54924191') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wmcarey.edu' else 'wmcarey.edu' end WHERE [vchAthensOrgId] like '%70215733%' and len('70215733') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523151.swanlibraries.net' else '69523151.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523151%' and len('69523151') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wmsf.ac.uk' else 'wmsf.ac.uk' end WHERE [vchAthensOrgId] like '%15967864%' and len('15967864') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wshein.com' else 'wshein.com' end WHERE [vchAthensOrgId] like '%7091600%' and len('7091600') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924228.va.gov' else '54924228.va.gov' end WHERE [vchAthensOrgId] like '%54924228%' and len('54924228') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924192.va.gov' else '54924192.va.gov' end WHERE [vchAthensOrgId] like '%54924192%' and len('54924192') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5782995.eng.nhs.uk' else '5782995.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5782995%' and len('5782995') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044150.eng.nhs.uk' else '4044150.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044150%' and len('4044150') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',54924198.va.gov' else '54924198.va.gov' end WHERE [vchAthensOrgId] like '%54924198%' and len('54924198') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',55328343.lexi.com' else '55328343.lexi.com' end WHERE [vchAthensOrgId] like '%55328343%' and len('55328343') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wolterskluwer.silverchair.com' else 'wolterskluwer.silverchair.com' end WHERE [vchAthensOrgId] like '%68163232%' and len('68163232') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',ovid.com' else 'ovid.com' end WHERE [vchAthensOrgId] like '%2413116%' and len('2413116') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wolterskluwer.com' else 'wolterskluwer.com' end WHERE [vchAthensOrgId] like '%66593549%' and len('66593549') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195495.eng.nhs.uk' else '5195495.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195495%' and len('5195495') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66118199.akzonobel.com' else '66118199.akzonobel.com' end WHERE [vchAthensOrgId] like '%66118199%' and len('66118199') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523152.swanlibraries.net' else '69523152.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523152%' and len('69523152') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wgh.on.ca' else 'wgh.on.ca' end WHERE [vchAthensOrgId] like '%38494442%' and len('38494442') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195473.eng.nhs.uk' else '5195473.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195473%' and len('5195473') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195471.eng.nhs.uk' else '5195471.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195471%' and len('5195471') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',wsps.ca' else 'wsps.ca' end WHERE [vchAthensOrgId] like '%69592535%' and len('69592535') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',warc.com' else 'warc.com' end WHERE [vchAthensOrgId] like '%7433617%' and len('7433617') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',worldbankgroup.org' else 'worldbankgroup.org' end WHERE [vchAthensOrgId] like '%68711776%' and len('68711776') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',worldpoliticsreview.com' else 'worldpoliticsreview.com' end WHERE [vchAthensOrgId] like '%70397861%' and len('70397861') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69523153.swanlibraries.net' else '69523153.swanlibraries.net' end WHERE [vchAthensOrgId] like '%69523153%' and len('69523153') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4044156.eng.nhs.uk' else '4044156.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4044156%' and len('4044156') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',writtle.ac.uk' else 'writtle.ac.uk' end WHERE [vchAthensOrgId] like '%194%' and len('194') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5195477.eng.nhs.uk' else '5195477.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5195477%' and len('5195477') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4073218.eng.nhs.uk' else '4073218.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5344632.eng.nhs.uk' else '5344632.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%1407223%' and len('1407223') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',xbhs.com' else 'xbhs.com' end WHERE [vchAthensOrgId] like '%70240796%' and len('70240796') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',xjtu.edu.cn' else 'xjtu.edu.cn' end WHERE [vchAthensOrgId] like '%70113680%' and len('70113680') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',5762755.eng.nhs.uk' else '5762755.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%5762755%' and len('5762755') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',yorksj.ac.uk' else 'yorksj.ac.uk' end WHERE [vchAthensOrgId] like '%118%' and len('118') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058706.eng.nhs.uk' else '4058706.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058706%' and len('4058706') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',4058695.eng.nhs.uk' else '4058695.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%4058695%' and len('4058695') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',13410059.eng.nhs.uk' else '13410059.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%13410059%' and len('13410059') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',66181852.eng.nhs.uk' else '66181852.eng.nhs.uk' end WHERE [vchAthensOrgId] like '%66181852%' and len('66181852') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',openathenstrials.net' else 'openathenstrials.net' end WHERE [vchAthensOrgId] like '%69803356%' and len('69803356') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',12620208.ahsl.arizona.edu' else '12620208.ahsl.arizona.edu' end WHERE [vchAthensOrgId] like '%12620208%' and len('12620208') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',zju.edu.cn' else 'zju.edu.cn' end WHERE [vchAthensOrgId] like '%70143146%' and len('70143146') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',68139929.dtf.vic.gov.au' else '68139929.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%68139929%' and len('68139929') > 6;
 go
UPDATE tInstitution SET vchUpdaterId = 'Athens Update', dtLastUpdate = GETDATE(), vchAthensScopedAffiliation = case when [vchAthensScopedAffiliation] is not null then  vchAthensScopedAffiliation + ',69983442.68139900.dtf.vic.gov.au' else '69983442.68139900.dtf.vic.gov.au' end WHERE [vchAthensOrgId] like '%69983442%' and len('69983442') > 6;
 go
