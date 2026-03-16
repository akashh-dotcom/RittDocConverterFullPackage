# PowerShell script to create local development directories for R2Library
# Run this script once to set up your local development environment

Write-Host "Creating R2Library local development directories..." -ForegroundColor Green

$directories = @(
    "C:\Temp\R2Library\Logs",
    "C:\Temp\R2Library\MessageQueueErrors",
    "C:\Temp\R2Library\Content\xml",
    "C:\Temp\R2Library\Content\cache",
    "C:\Temp\R2Library\Content\R2HtmlIndex",
    "C:\Temp\R2Library\_Static\Xsl"
)

foreach ($dir in $directories) {
    if (!(Test-Path -Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        Write-Host "Created: $dir" -ForegroundColor Cyan
    } else {
        Write-Host "Already exists: $dir" -ForegroundColor Yellow
    }
}

Write-Host "`nAll directories created successfully!" -ForegroundColor Green
Write-Host "You can now run the R2Library web application in local development mode." -ForegroundColor Green
