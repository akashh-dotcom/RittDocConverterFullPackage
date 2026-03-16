ContentSigner\ContentSigner.exe C:\Projects\wwwroot\R2V2\_static C:\Projects\wwwroot\R2V2\_static.dat "css|min.js|r2v2.js"
del C:\Projects\wwwroot\R2V2\bin\System.Data.SQLite.dll
xcopy /Y /E /R C:\Projects\wwwroot\R2V2 C:\Projects\wwwroot\R2V2_cpy
del C:\Projects\wwwroot\R2V2\*.config
del C:\Projects\wwwroot\R2V2\Robots
del R2V2.7z
"C:\Program Files\7-Zip\7z.exe" a R2V2.7z C:\Projects\wwwroot\R2V2





