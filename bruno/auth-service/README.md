# Bruno Collection for Auth Service

This Bruno collection provides automatic signature generation for all API requests.

## Features

✅ **Automatic Signature Calculation**: Pre-request scripts automatically generate HMAC-SHA256 signatures  
✅ **Timestamp Management**: Fresh timestamps generated for every request  
✅ **No Manual Work**: Just hit send - signatures are calculated automatically  
✅ **Environment Support**: Easy switching between environments (Local, Production, etc.)

## Setup

### 1. Install Bruno
Download from: https://www.usebruno.com/

### 2. Open the Collection
In Bruno, click **Open Collection** and select the `bruno/auth-service` folder.

### 3. Configure Environment
1. Go to **Environments** tab
2. Select **Local** environment
3. Update the variables if needed:
   - `baseUrl`: Your service URL (default: `http://localhost:5000`)
   - `clientId`: Your client identifier (default: `example-client`)
   - `secret`: Your secret key (default: `CHANGE_THIS_SECRET_IN_PRODUCTION`)

### 4. Start Testing
Just select a request and click **Send**! The signature will be calculated automatically.

## How It Works

Each request has a **pre-request script** that:
1. Gets the current timestamp
2. Reads the request body
3. Calculates the HMAC-SHA256 signature
4. Sets the `X-Timestamp` and `X-Signature` headers automatically

## Requests Available

1. **Health Check** - No signature required
2. **Create User** - Register a new user
3. **Login** - Authenticate a user
4. **Get Whitelist** - Retrieve all whitelisted emails
5. **Add Emails to Whitelist** - Add emails to whitelist
6. **Delete Emails from Whitelist** - Remove emails from whitelist

## Console Output

Check Bruno's console to see the signing process:
- Data being signed
- Generated timestamp
- Generated signature

## Adding New Requests

When adding new requests, include this pre-request script:

```javascript
const crypto = require('crypto');

const method = req.getMethod();
const url = new URL(req.getUrl());
const path = url.pathname;
const timestamp = Math.floor(Date.now() / 1000).toString();

// Get body - compact JSON (no formatting)
const body = req.getBody() ? JSON.stringify(req.getBody()) : '';

// Calculate signature
const dataToSign = `${method}\n${path}\n${timestamp}\n${body}`;
const hmac = crypto.createHmac('sha256', bru.getEnvVar('secret'));
hmac.update(dataToSign);
const signature = hmac.digest('base64');

// Set variables
bru.setVar('timestamp', timestamp);
bru.setVar('signature', signature);

console.log('Signing data:', dataToSign);
console.log('Timestamp:', timestamp);
console.log('Signature:', signature);
```

And add these headers:
```
X-Client-Id: {{clientId}}
X-Timestamp: {{timestamp}}
X-Signature: {{signature}}
```

## Benefits vs .http Files

- ✅ No manual PowerShell commands
- ✅ Automatic signature generation
- ✅ Better testing workflow
- ✅ Collections can be version controlled
- ✅ Easy environment switching

## Troubleshooting

**Signature fails?**
- Check the console output to see what data is being signed
- Verify the `secret` in your environment matches `appsettings.json`
- Ensure your service is running on the correct `baseUrl`

**Timestamp expired?**
- Just resend the request - a fresh timestamp is generated automatically
