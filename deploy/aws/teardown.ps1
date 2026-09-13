[CmdletBinding(SupportsShouldProcess = $true, ConfirmImpact = 'High')]
param(
    [string] $Region = 'af-south-1',
    [string] $StackName = 'atlas-premier-properties'
)

$ErrorActionPreference = 'Stop'
$aws = (Get-Command aws -ErrorAction SilentlyContinue).Source
if (-not $aws) { $aws = 'C:\Program Files\Amazon\AWSCLIV2\aws.exe' }

$bucket = & $aws cloudformation describe-stacks --stack-name $StackName --region $Region --query "Stacks[0].Outputs[?OutputKey=='ArtifactBucketName'].OutputValue" --output text
if ($LASTEXITCODE -ne 0) { throw "Stack $StackName was not found in $Region." }

if ($PSCmdlet.ShouldProcess("$StackName in $Region", 'Permanently delete the server (including its database), the S3 bucket and the CloudFront distribution')) {
    & $aws s3 rm "s3://$bucket" --recursive --region $Region --only-show-errors
    if ($LASTEXITCODE -ne 0) { throw 'Emptying the S3 bucket failed.' }

    & $aws cloudformation delete-stack --stack-name $StackName --region $Region
    if ($LASTEXITCODE -ne 0) { throw 'delete-stack failed.' }

    Write-Output 'Deleting. CloudFront is the slowest part and often takes 10-20 minutes...'
    & $aws cloudformation wait stack-delete-complete --stack-name $StackName --region $Region
    if ($LASTEXITCODE -ne 0) { throw 'Stack deletion did not finish. Check the CloudFormation console.' }

    Write-Output 'All Atlas Premier Properties AWS resources are deleted.'
}
