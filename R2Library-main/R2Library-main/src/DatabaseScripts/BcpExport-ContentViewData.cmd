rem bcp "select * from DEV_R2Reports.dbo.ContentView where contentViewTimestamp between '1/1/2013' and '2/1/2013'" queryout "D:\ClientsNoBackup\Rittenhouse\R2ReportData\rittdb3\ContentView-2013-01-testing.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012
bcp  "[DEV_R2Reports].dbo.ContentViewTemp" in "D:\ClientsNoBackup\Rittenhouse\R2ReportData\rittdb3\ContentView-2012-09.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012
bcp  "[DEV_R2Reports].dbo.ContentViewTemp" in "D:\ClientsNoBackup\Rittenhouse\R2ReportData\rittdb3\ContentView-2012-10.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012
bcp  "[DEV_R2Reports].dbo.ContentViewTemp" in "D:\ClientsNoBackup\Rittenhouse\R2ReportData\rittdb3\ContentView-2012-11.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012
pause
