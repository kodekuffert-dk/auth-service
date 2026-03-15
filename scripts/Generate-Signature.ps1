# Helper script to generate HMAC-SHA256 signatures for API requests
param(
    [Parameter(Mandatory=$true)]
    [string]$Method,
    
    [Parameter(Mandatory=$true)]
    [string]$Path,
    
    [Parameter(Mandatory=$false)]
    [string]$Body = "",
    
    [Parameter(Mandatory=$true)]
    [string]$Secret,
    
    [Parameter(Mandatory=$false)]
    [string]$ClientId = "example-client"
)

# Get current Unix timestamp
$timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()

# Construct data to sign
$dataToSign = "$Method`n$Path`n$timestamp`n$Body"

# Calculate HMAC-SHA256 signature
$hmac = [Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Secret))
$hashBytes = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($dataToSign))
$signature = [Convert]::ToBase64String($hashBytes)

# Output results
Write-Host "=== Signature Generated ===" -ForegroundColor Green
Write-Host "X-Client-Id: $ClientId" -ForegroundColor Cyan
Write-Host "X-Timestamp: $timestamp" -ForegroundColor Cyan
Write-Host "X-Signature: $signature" -ForegroundColor Cyan
Write-Host ""
Write-Host "Data signed:" -ForegroundColor Yellow
Write-Host $dataToSign -ForegroundColor Gray
Write-Host ""
Write-Host "Copy these headers to your .http file:" -ForegroundColor Green
Write-Host "X-Client-Id: $ClientId"
Write-Host "X-Timestamp: $timestamp"
Write-Host "X-Signature: $signature"

# Return as object for programmatic use
return @{
    ClientId = $ClientId
    Timestamp = $timestamp
    Signature = $signature
}
