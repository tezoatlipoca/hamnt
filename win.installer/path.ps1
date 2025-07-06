refreshenv

Write-Host "USER PATH:" -ForegroundColor Green
[Environment]::GetEnvironmentVariable("PATH", "User") -split ";" | ForEach-Object { "  $_" }

Write-Host "`nSYSTEM PATH:" -ForegroundColor Yellow
[Environment]::GetEnvironmentVariable("PATH", "Machine") -split ";" | ForEach-Object { "  $_" }