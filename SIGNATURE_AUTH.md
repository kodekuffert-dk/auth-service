# Signature-Based Authentication

This service uses HMAC-SHA256 signature-based authentication for all API requests (except `/health`).

## How It Works

Each client must:
1. Have a registered **Client ID** and **Secret Key**
2. Include three headers with every request:
   - `X-Client-Id`: Your client identifier
   - `X-Timestamp`: Current Unix timestamp (seconds since epoch)
   - `X-Signature`: HMAC-SHA256 signature of the request

## Signature Calculation

The signature is calculated as follows:

```
dataToSign = METHOD + "\n" + PATH + "\n" + TIMESTAMP + "\n" + BODY
signature = Base64(HMACSHA256(dataToSign, secret))
```

Where:
- **METHOD**: HTTP method (GET, POST, etc.)
- **PATH**: Full path including query string (e.g., `/User?id=123`)
- **TIMESTAMP**: Unix timestamp from X-Timestamp header
- **BODY**: Raw request body (empty string if no body)
- **secret**: Your client secret key

## Security Features

- **Replay Attack Prevention**: Requests older than 5 minutes are rejected
- **HMAC-SHA256**: Industry-standard cryptographic signing
- **Secret Key Management**: Keys stored securely in configuration

## Configuration

Add client credentials to `appsettings.json`:

```json
{
  "ClientSecrets": {
    "your-client-id": "your-secret-key"
  }
}
```

## Example: C# Client

```csharp
using System.Security.Cryptography;
using System.Text;

public class AuthServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _secret;

    public AuthServiceClient(string baseUrl, string clientId, string secret)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _clientId = clientId;
        _secret = secret;
    }

    public async Task<HttpResponseMessage> CreateUserAsync(object userData)
    {
        var path = "/User";
        var method = "POST";
        var body = JsonSerializer.Serialize(userData);
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var signature = GenerateSignature(method, path, timestamp, body);

        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Client-Id", _clientId);
        request.Headers.Add("X-Timestamp", timestamp);
        request.Headers.Add("X-Signature", signature);

        return await _httpClient.SendAsync(request);
    }

    private string GenerateSignature(string method, string path, string timestamp, string body)
    {
        var dataToSign = $"{method}\n{path}\n{timestamp}\n{body}";
        var keyBytes = Encoding.UTF8.GetBytes(_secret);
        var dataBytes = Encoding.UTF8.GetBytes(dataToSign);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
```

## Example: JavaScript/Node.js Client

```javascript
const crypto = require('crypto');
const axios = require('axios');

class AuthServiceClient {
    constructor(baseUrl, clientId, secret) {
        this.baseUrl = baseUrl;
        this.clientId = clientId;
        this.secret = secret;
    }

    async createUser(userData) {
        const path = '/User';
        const method = 'POST';
        const body = JSON.stringify(userData);
        const timestamp = Math.floor(Date.now() / 1000).toString();

        const signature = this._generateSignature(method, path, timestamp, body);

        const response = await axios.post(`${this.baseUrl}${path}`, userData, {
            headers: {
                'X-Client-Id': this.clientId,
                'X-Timestamp': timestamp,
                'X-Signature': signature,
                'Content-Type': 'application/json'
            }
        });

        return response.data;
    }

    _generateSignature(method, path, timestamp, body) {
        const dataToSign = `${method}\n${path}\n${timestamp}\n${body}`;
        const hmac = crypto.createHmac('sha256', this.secret);
        hmac.update(dataToSign);
        return hmac.digest('base64');
    }
}

// Usage example
const client = new AuthServiceClient(
    'http://localhost:5000',
    'example-client',
    'your-secret-key'
);

client.createUser({
    Email: 'test@example.com',
    Password: 'password123'
}).then(result => {
    console.log('User created:', result);
}).catch(error => {
    console.error('Error:', error.response?.data || error.message);
});
```

### Express.js Middleware Example

For integrating into your Node.js gateway:

```javascript
const crypto = require('crypto');

function createSignatureMiddleware(clientId, secret) {
    return async (req, res, next) => {
        const path = req.originalUrl;
        const method = req.method;
        const body = req.body ? JSON.stringify(req.body) : '';
        const timestamp = Math.floor(Date.now() / 1000).toString();

        const dataToSign = `${method}\n${path}\n${timestamp}\n${body}`;
        const hmac = crypto.createHmac('sha256', secret);
        hmac.update(dataToSign);
        const signature = hmac.digest('base64');

        req.headers['x-client-id'] = clientId;
        req.headers['x-timestamp'] = timestamp;
        req.headers['x-signature'] = signature;

        next();
    };
}

// Usage in your gateway
app.use('/api/auth-service', createSignatureMiddleware('gateway-client', 'your-secret'));
```

## Example: Python Client

```python
import hmac
import hashlib
import base64
import time
import requests

class AuthServiceClient:
    def __init__(self, base_url, client_id, secret):
        self.base_url = base_url
        self.client_id = client_id
        self.secret = secret

    def create_user(self, user_data):
        path = "/User"
        method = "POST"
        body = json.dumps(user_data)
        timestamp = str(int(time.time()))

        signature = self._generate_signature(method, path, timestamp, body)

        headers = {
            "X-Client-Id": self.client_id,
            "X-Timestamp": timestamp,
            "X-Signature": signature,
            "Content-Type": "application/json"
        }

        return requests.post(f"{self.base_url}{path}", data=body, headers=headers)

    def _generate_signature(self, method, path, timestamp, body):
        data_to_sign = f"{method}\n{path}\n{timestamp}\n{body}"
        signature = hmac.new(
            self.secret.encode('utf-8'),
            data_to_sign.encode('utf-8'),
            hashlib.sha256
        ).digest()
        return base64.b64encode(signature).decode('utf-8')
```

## Testing with cURL

```bash
CLIENT_ID="example-client"
SECRET="your-secret-key"
TIMESTAMP=$(date +%s)
METHOD="POST"
PATH="/User"
BODY='{"Email":"test@example.com","Password":"password123"}'

# Calculate signature (requires openssl)
DATA_TO_SIGN="${METHOD}\n${PATH}\n${TIMESTAMP}\n${BODY}"
SIGNATURE=$(echo -ne "$DATA_TO_SIGN" | openssl dgst -sha256 -hmac "$SECRET" -binary | base64)

# Make request
curl -X POST http://localhost:5000/User \
  -H "Content-Type: application/json" \
  -H "X-Client-Id: $CLIENT_ID" \
  -H "X-Timestamp: $TIMESTAMP" \
  -H "X-Signature: $SIGNATURE" \
  -d "$BODY"
```

## Error Responses

- **401 Unauthorized**: Missing headers, invalid signature, or expired timestamp
- **400 Bad Request**: Malformed request data
