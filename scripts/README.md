# Testing Scripts for Auth Service

This folder contains PowerShell scripts to help test the auth service with signature-based authentication.

## Scripts

### `Generate-Signature.ps1`

Generate HMAC-SHA256 signatures for individual API requests.

**Usage:**
```powershell
.\scripts\Generate-Signature.ps1 `
    -Method "POST" `
    -Path "/user" `
    -Body '{"email":"test@example.com","password":"pass123"}' `
    -Secret "CHANGE_THIS_SECRET_IN_PRODUCTION" `
    -ClientId "example-client"
```

**Parameters:**
- `Method` (required): HTTP method (GET, POST, DELETE, etc.)
- `Path` (required): API path (e.g., `/user`, `/whitelist`)
- `Body` (optional): JSON body as string (empty for GET requests)
- `Secret` (required): Your client secret key
- `ClientId` (optional): Your client ID (defaults to "example-client")

**Output:**
```
=== Signature Generated ===
X-Client-Id: example-client
X-Timestamp: 1704067200
X-Signature: abc123...

Copy these headers to your .http file:
X-Client-Id: example-client
X-Timestamp: 1704067200
X-Signature: abc123...
```

### `Test-AuthService.ps1`

Complete automated test suite for the auth service.

**Usage:**
```powershell
# Test with default settings (localhost:5000)
.\scripts\Test-AuthService.ps1

# Test with custom settings
.\scripts\Test-AuthService.ps1 `
    -BaseUrl "http://localhost:8080" `
    -ClientId "my-client" `
    -Secret "my-secret-key"
```

**Parameters:**
- `BaseUrl` (optional): Service base URL (default: "http://localhost:5000")
- `ClientId` (optional): Client identifier (default: "example-client")
- `Secret` (optional): Client secret (default: "CHANGE_THIS_SECRET_IN_PRODUCTION")

**What it tests:**
1. ✓ Health check endpoint
2. ✓ Add emails to whitelist
3. ✓ Get whitelist
4. ✓ Create user
5. ✓ Login
6. ✓ Delete emails from whitelist

## Quick Start

### 1. Start the auth service
```powershell
cd auth-service
dotnet run
```

### 2. Run the automated tests
```powershell
.\scripts\Test-AuthService.ps1
```

### 3. Or generate signatures for manual testing
```powershell
# For a POST request with body
.\scripts\Generate-Signature.ps1 `
    -Method "POST" `
    -Path "/user" `
    -Body '{"email":"test@example.com","password":"SecurePassword123!"}' `
    -Secret "CHANGE_THIS_SECRET_IN_PRODUCTION"

# For a GET request (no body)
.\scripts\Generate-Signature.ps1 `
    -Method "GET" `
    -Path "/whitelist" `
    -Secret "CHANGE_THIS_SECRET_IN_PRODUCTION"
```

## Configuration

Make sure your client credentials match what's configured in `appsettings.json`:

```json
{
  "ClientSecrets": {
    "example-client": "CHANGE_THIS_SECRET_IN_PRODUCTION"
  }
}
```

## Using with .http files

1. Generate signature using `Generate-Signature.ps1`
2. Copy the output headers to your `.http` file
3. Send the request in Visual Studio

See `auth-service.http` for examples.
