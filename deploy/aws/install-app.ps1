param(
    [Parameter(Mandatory = $true)] [string] $Bucket,
    [Parameter(Mandatory = $true)] [string] $Region,
    [switch] $ReplaceDatabase
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$siteName = 'AtlasPremierProperties'
$sitePath = "C:\inetpub\$siteName"
$dataPath = 'C:\AtlasData'
$dbFile = Join-Path $dataPath 'Cryptonics_DB.accdb'
$work = 'C:\AtlasDeploy'
$aceKey = 'HKLM:\SOFTWARE\Classes\Microsoft.ACE.OLEDB.12.0'

New-Item -ItemType Directory -Force -Path $work, $dataPath, $sitePath | Out-Null

Write-Output 'Checking IIS and ASP.NET 4.8...'
$missing = @('Web-Server', 'Web-Asp-Net45', 'Web-Mgmt-Console' | Where-Object { -not (Get-WindowsFeature -Name $_).Installed })
if ($missing.Count -gt 0) {
    $result = Install-WindowsFeature -Name $missing
    if (-not $result.Success) { throw "Installing IIS features failed: $($result.ExitCode)" }
}
Import-Module WebAdministration

Write-Output 'Checking the Access Database Engine...'
if (-not (Test-Path $aceKey)) {
    $installer = Join-Path $work 'accessdatabaseengine_X64.exe'
    Read-S3Object -BucketName $Bucket -Key 'installers/accessdatabaseengine_X64.exe' -File $installer -Region $Region | Out-Null
    $proc = Start-Process -FilePath $installer -ArgumentList '/quiet' -Wait -PassThru
    if ($proc.ExitCode -notin 0, 3010) { throw "Access Database Engine install failed with exit code $($proc.ExitCode)" }
    if (-not (Test-Path $aceKey)) { throw 'Microsoft.ACE.OLEDB.12.0 is still not registered after installing the Access Database Engine.' }
}

Write-Output 'Downloading the site...'
$zip = Join-Path $work 'site.zip'
$staging = Join-Path $work 'site'
Read-S3Object -BucketName $Bucket -Key 'site/site.zip' -File $zip -Region $Region | Out-Null
if (Test-Path $staging) { Remove-Item $staging -Recurse -Force }
Expand-Archive -Path $zip -DestinationPath $staging

if (Get-Website -Name 'Default Web Site') { Remove-Website -Name 'Default Web Site' }

$pool = "IIS:\AppPools\$siteName"
if (-not (Test-Path $pool)) { New-WebAppPool -Name $siteName | Out-Null }
Set-ItemProperty $pool -Name managedRuntimeVersion -Value 'v4.0'
Set-ItemProperty $pool -Name managedPipelineMode -Value 'Integrated'
Set-ItemProperty $pool -Name enable32BitAppOnWin64 -Value $false

if ((Get-WebAppPoolState -Name $siteName).Value -ne 'Stopped') {
    Stop-WebAppPool -Name $siteName
    for ($i = 0; $i -lt 30 -and (Get-WebAppPoolState -Name $siteName).Value -ne 'Stopped'; $i++) { Start-Sleep -Seconds 1 }
}

Write-Output 'Copying site files...'
robocopy $staging $sitePath /MIR /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw "Copying site files failed (robocopy exit code $LASTEXITCODE)" }

if ($ReplaceDatabase -and (Test-Path $dbFile)) {
    Copy-Item $dbFile "$dbFile.$(Get-Date -Format yyyyMMdd-HHmmss).bak"
}
if ($ReplaceDatabase -or -not (Test-Path $dbFile)) {
    Write-Output 'Downloading the database...'
    Read-S3Object -BucketName $Bucket -Key 'data/Cryptonics_DB.accdb' -File $dbFile -Region $Region | Out-Null
}

if (-not (Get-Website -Name $siteName)) {
    New-Website -Name $siteName -PhysicalPath $sitePath -ApplicationPool $siteName -Port 80 | Out-Null
}
Start-WebAppPool -Name $siteName

# Access writes a .laccdb lock file beside the database, so the app pool needs Modify on the folder, not just the file.
icacls $dataPath /grant "IIS AppPool\${siteName}:(OI)(CI)M" /T /Q | Out-Null
if ($LASTEXITCODE -ne 0) { throw "Granting database folder permissions failed (icacls exit code $LASTEXITCODE)" }

if ((Get-WebsiteState -Name $siteName).Value -ne 'Started') { Start-Website -Name $siteName }

Write-Output 'Checking the site...'
$status = $null
$lastError = $null
for ($i = 0; $i -lt 10 -and $status -ne 200; $i++) {
    try {
        $status = (Invoke-WebRequest 'http://localhost/Account/Login' -UseBasicParsing -TimeoutSec 60).StatusCode
    } catch {
        $lastError = $_.Exception.Message
        Start-Sleep -Seconds 6
    }
}
if ($status -ne 200) { throw "The sign-in page did not load: $lastError. Check the Application event log on the server." }

Write-Output 'Deployed. The sign-in page loaded, which means IIS, ASP.NET, the Access engine and the database all work.'
exit 0
