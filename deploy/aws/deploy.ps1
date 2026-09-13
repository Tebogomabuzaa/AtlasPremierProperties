param(
    [string] $Region = 'af-south-1',
    [string] $StackName = 'atlas-premier-properties',
    [ValidateSet('t3.small', 't3.medium')] [string] $InstanceType = 't3.small',
    [string] $AceInstaller = (Join-Path $env:USERPROFILE 'Downloads\accessdatabaseengine_X64.exe'),
    [string] $Database = (Join-Path $env:USERPROFILE 'Desktop\Cryptonics\Cryptonics_DB.accdb'),
    [switch] $ReplaceDatabase
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$work = Join-Path $env:TEMP 'atlas-deploy'

$aws = (Get-Command aws -ErrorAction SilentlyContinue).Source
if (-not $aws) { $aws = 'C:\Program Files\Amazon\AWSCLIV2\aws.exe' }
if (-not (Test-Path $aws)) { throw 'AWS CLI not found. Install it with: winget install Amazon.AWSCLI' }

function Invoke-Aws {
    $output = & $aws @args --region $Region
    if ($LASTEXITCODE -ne 0) { throw "aws $($args -join ' ') failed with exit code $LASTEXITCODE" }
    return $output
}

# Windows PowerShell 5.1 turns redirected native stderr into terminating errors under 'Stop', so probes relax it locally.
function Test-AwsSucceeds {
    $ErrorActionPreference = 'Continue'
    & $aws @args --region $Region 2>$null | Out-Null
    return ($LASTEXITCODE -eq 0)
}

foreach ($file in $AceInstaller, $Database) {
    if (-not (Test-Path $file)) { throw "Missing file: $file" }
}
$signature = Get-AuthenticodeSignature $AceInstaller
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notlike '*O=Microsoft Corporation*') {
    throw "$AceInstaller is not a valid Microsoft-signed installer."
}

$identity = Invoke-Aws sts get-caller-identity --query Arn --output text
Write-Output "Deploying as $identity to $Region"

$prefixListId = Invoke-Aws ec2 describe-managed-prefix-lists --filters 'Name=prefix-list-name,Values=com.amazonaws.global.cloudfront.origin-facing' --query 'PrefixLists[0].PrefixListId' --output text
$overrides = @("InstanceType=$InstanceType", "CloudFrontPrefixListId=$prefixListId")

if (Test-AwsSucceeds cloudformation describe-stacks --stack-name $StackName) {
    Write-Output 'Updating the stack...'
} else {
    $amiId = Invoke-Aws ssm get-parameter --name /aws/service/ami-windows-latest/Windows_Server-2022-English-Full-Base --query Parameter.Value --output text
    $overrides += "WindowsAmiId=$amiId"
    Write-Output "Creating the stack from $amiId. This takes 10-20 minutes, mostly waiting for CloudFront..."
}

& $aws cloudformation deploy --region $Region --stack-name $StackName --template-file (Join-Path $PSScriptRoot 'atlas-stack.yaml') --capabilities CAPABILITY_IAM --no-fail-on-empty-changeset --parameter-overrides @overrides
if ($LASTEXITCODE -ne 0) { throw 'CloudFormation deploy failed. Check the stack events in the CloudFormation console.' }

$outputs = @{}
((Invoke-Aws cloudformation describe-stacks --stack-name $StackName --query 'Stacks[0].Outputs' --output json) -join "`n" | ConvertFrom-Json) |
    ForEach-Object { $outputs[$_.OutputKey] = $_.OutputValue }
$bucket = $outputs['ArtifactBucketName']
$instanceId = $outputs['InstanceId']

Invoke-Aws ec2 modify-instance-metadata-options --instance-id $instanceId --http-tokens required --http-endpoint enabled | Out-Null

Write-Output 'Publishing the app...'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$msbuild = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) { throw 'MSBuild not found. Install Visual Studio 2022 with the ASP.NET workload.' }

if (Test-Path $work) { Remove-Item $work -Recurse -Force }
New-Item -ItemType Directory -Path $work | Out-Null
$publishDir = Join-Path $work 'site'
$zip = Join-Path $work 'site.zip'
$project = Join-Path $repoRoot 'AtlasPremierProperties.csproj'

& $msbuild $project -t:Restore -p:RestorePackagesConfig=true -v:minimal -nologo
if ($LASTEXITCODE -ne 0) { throw 'NuGet restore failed.' }
& $msbuild $project -p:Configuration=Release -p:DeployOnBuild=true -p:DeployDefaultTarget=WebPublish -p:WebPublishMethod=FileSystem "-p:PublishUrl=$publishDir" -p:DeleteExistingFiles=true -v:minimal -nologo
if ($LASTEXITCODE -ne 0) { throw 'Publishing the app failed.' }
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zip

Write-Output 'Uploading to S3...'
Invoke-Aws s3 cp $zip "s3://$bucket/site/site.zip" --only-show-errors | Out-Null
Invoke-Aws s3 cp (Join-Path $PSScriptRoot 'install-app.ps1') "s3://$bucket/deploy/install-app.ps1" --only-show-errors | Out-Null
if (-not (Test-AwsSucceeds s3api head-object --bucket $bucket --key 'installers/accessdatabaseengine_X64.exe')) {
    Invoke-Aws s3 cp $AceInstaller "s3://$bucket/installers/accessdatabaseengine_X64.exe" --only-show-errors | Out-Null
}
if ($ReplaceDatabase -or -not (Test-AwsSucceeds s3api head-object --bucket $bucket --key 'data/Cryptonics_DB.accdb')) {
    Invoke-Aws s3 cp $Database "s3://$bucket/data/Cryptonics_DB.accdb" --only-show-errors | Out-Null
}

Write-Output 'Waiting for the server to come online in Systems Manager...'
$deadline = (Get-Date).AddMinutes(20)
while ((Invoke-Aws ssm describe-instance-information --filters "Key=InstanceIds,Values=$instanceId" --query 'InstanceInformationList[0].PingStatus' --output text) -ne 'Online') {
    if ((Get-Date) -gt $deadline) { throw 'The server did not come online in Systems Manager within 20 minutes.' }
    Start-Sleep -Seconds 20
}

$flag = if ($ReplaceDatabase) { ' -ReplaceDatabase' } else { '' }
$commands = @(
    'New-Item -ItemType Directory -Force -Path C:\AtlasDeploy | Out-Null',
    "Read-S3Object -BucketName '$bucket' -Key 'deploy/install-app.ps1' -File 'C:\AtlasDeploy\install-app.ps1' -Region '$Region' | Out-Null",
    "& 'C:\AtlasDeploy\install-app.ps1' -Bucket '$bucket' -Region '$Region'$flag"
)
$paramsFile = Join-Path $work 'ssm-params.json'
# The AWS CLI rejects JSON files with a byte-order mark, which Set-Content -Encoding UTF8 adds in PowerShell 5.1.
[IO.File]::WriteAllText($paramsFile, (@{ commands = $commands; executionTimeout = @('3600') } | ConvertTo-Json))

$commandId = Invoke-Aws ssm send-command --instance-ids $instanceId --document-name AWS-RunPowerShellScript --parameters "file://$paramsFile" --output-s3-bucket-name $bucket --output-s3-key-prefix ssm-logs --query Command.CommandId --output text
Write-Output "Installing on the server (command $commandId). The first run takes about 5-10 minutes..."

$invocation = $null
$deadline = (Get-Date).AddMinutes(60)
do {
    if ((Get-Date) -gt $deadline) { throw "Timed out waiting for command $commandId." }
    Start-Sleep -Seconds 20
    $ErrorActionPreference = 'Continue'
    $raw = & $aws ssm get-command-invocation --command-id $commandId --instance-id $instanceId --region $Region --output json 2>$null
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -eq 0) { $invocation = ($raw -join "`n") | ConvertFrom-Json }
} while (-not $invocation -or $invocation.Status -in 'Pending', 'InProgress', 'Delayed')

Write-Output $invocation.StandardOutputContent
if ($invocation.Status -ne 'Success') {
    Write-Output $invocation.StandardErrorContent
    throw "Server install finished with status $($invocation.Status). Full logs: s3://$bucket/ssm-logs/$commandId/"
}

Write-Output ''
Write-Output "Site: $($outputs['SiteUrl'])"
Write-Output 'To create the first administrator, run this and then open http://localhost:8080/Account/Setup :'
Write-Output "  aws ssm start-session --target $instanceId --region $Region --document-name AWS-StartPortForwardingSession --parameters 'portNumber=80,localPortNumber=8080'"
