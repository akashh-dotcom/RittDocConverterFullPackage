bcp  "[DEV_R2Reports].dbo.ContentView" in "\\technoserv02\d$\R2v2\R2ReportsData\R2Reports\ContentView-2013-6.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012 -t~
pause
bcp  "[DEV_R2Reports].dbo.PageView" in "\\technoserv02\d$\R2v2\R2ReportsData\R2Reports\PageView-2013-6.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012 -t~
pause
bcp  "[DEV_R2Reports].dbo.Search" in "\\technoserv02\d$\R2v2\R2ReportsData\R2Reports\Search-2013-6.dat" -c -UR2ReportsUser -S10.0.0.32 -PR2Reports2012 -t~
pause
