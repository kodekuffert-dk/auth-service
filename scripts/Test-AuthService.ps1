# Complete testing script for auth service with signature authentication
param(
    [Parameter(Mandatory=$false)]
    [string]$BaseUrl = "http://localhost:5000",
    
    [Parameter(Mandatory=$false)]
    [string]$ClientId = "example-client",
    
    [Parameter(Mandatory=$false)]
    [string]$Secret = "CHANGE_THIS_SECRET_IN_PRODUCTION"
)

function Invoke-SignedRequest {
    param(
        [string]$Method,
        [string]$Path,
        [object]$BodyObject = $null
    )
    
    $timestamp = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $body = if ($BodyObject) { $BodyObject | ConvertTo-Json -Compress -Depth 10 } else { "" }
    
    # Construct data to sign
    $dataToSign = "$Method`n$Path`n$timestamp`n$body"
    
    # Calculate signature
    $hmac = [Security.Cryptography.HMACSHA256]::new([Text.Encoding]::UTF8.GetBytes($Secret))
    $hashBytes = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($dataToSign))
    $signature = [Convert]::ToBase64String($hashBytes)
    
    # Prepare headers
    $headers = @{
        "X-Client-Id" = $ClientId
        "X-Timestamp" = $timestamp.ToString()
        "X-Signature" = $signature
        "Content-Type" = "application/json"
    }
    
    # Make request
    $uri = "$BaseUrl$Path"
    
    try {
        if ($Method -eq "GET") {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers
        } else {
            $response = Invoke-RestMethod -Uri $uri -Method $Method -Headers $headers -Body $body
        }
        
        Write-Host "✓ $Method $Path - Success" -ForegroundColor Green
        return $response
    } catch {
        Write-Host "✗ $Method $Path - Failed" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "  Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
        return $null
    }
}

Write-Host "`n=== Testing Auth Service with Signature Authentication ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Gray
Write-Host "Client ID: $ClientId" -ForegroundColor Gray
Write-Host ""

# Test 1: Health Check
Write-Host "`n[1] Testing Health Check..." -ForegroundColor Yellow
try {
    $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET
    Write-Host "✓ Health check passed" -ForegroundColor Green
} catch {
    Write-Host "✗ Health check failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Add emails to whitelist
Write-Host "`n[2] Testing Add Emails to Whitelist..." -ForegroundColor Yellow
$whitelistData = @{
    teamName = "Test Team"
    emails = @("testuser@example.com", "admin@example.com")
}
$result = Invoke-SignedRequest -Method "POST" -Path "/whitelist" -BodyObject $whitelistData

# Test 3: Get whitelist
Write-Host "`n[3] Testing Get Whitelist..." -ForegroundColor Yellow
$whitelist = Invoke-SignedRequest -Method "GET" -Path "/whitelist"
if ($whitelist) {
    Write-Host "  Found $($whitelist.Count) entries" -ForegroundColor Gray
}

# Test 4: Create User
Write-Host "`n[4] Testing Create User..." -ForegroundColor Yellow
$userData = @{
    email = "testuser@example.com"
    password = "SecurePassword123!"
}
$user = Invoke-SignedRequest -Method "POST" -Path "/user" -BodyObject $userData

# Test 5: Login
Write-Host "`n[5] Testing Login..." -ForegroundColor Yellow
$loginData = @{
    email = "testuser@example.com"
    password = "SecurePassword123!"
}
$loginResult = Invoke-SignedRequest -Method "POST" -Path "/login" -BodyObject $loginData
if ($loginResult) {
    Write-Host "  Token received (for other services that may need it)" -ForegroundColor Gray
}

# Test 6: Delete from whitelist
Write-Host "`n[6] Testing Delete Emails from Whitelist..." -ForegroundColor Yellow
$deleteData = @{
    emails = @("testuser@example.com")
}
$deleteResult = Invoke-SignedRequest -Method "DELETE" -Path "/whitelist" -BodyObject $deleteData

Write-Host "`n=== Testing Complete ===" -ForegroundColor Cyan
