net use w: \\192.168.1.99\D$ /user:r2library\technotects Techno2008
xcopy ".\R2v2.Web-*.7z" "W:\R2v2\Sites\Deploy" /K /R /D
xcopy ".\R2Utilities-*.7z" "W:\R2v2\R2Utilities\deploy" /K /R /D
xcopy ".\R2V2.WindowsService-*.7z" "W:\R2v2\WindowsService\deploy" /K /R /D
net use w: /DELETE
pause
