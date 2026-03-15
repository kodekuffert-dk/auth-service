using Microsoft.Extensions.Primitives;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace auth_service.Middleware;

public class SignatureValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SignatureValidationMiddleware> _logger;

    public SignatureValidationMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<SignatureValidationMiddleware> logger)
    {
        _next = next;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow health check endpoint without signature
        if (context.Request.Path.StartsWithSegments("/health"))
        {
            await _next(context);
            return;
        }

        // Check for required headers
        if (!context.Request.Headers.TryGetValue("X-Client-Id", out StringValues clientId) ||
            !context.Request.Headers.TryGetValue("X-Signature", out StringValues signature) ||
            !context.Request.Headers.TryGetValue("X-Timestamp", out StringValues timestamp))
        {
            _logger.LogWarning("Missing required signature headers");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Missing signature headers (X-Client-Id, X-Signature, X-Timestamp)" });
            return;
        }

        // Validate timestamp (prevent replay attacks)
        if (!long.TryParse(timestamp, out long timestampValue))
        {
            _logger.LogWarning("Invalid timestamp format");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid timestamp format" });
            return;
        }

        var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestampValue);
        var now = DateTimeOffset.UtcNow;
        var timeDifference = Math.Abs((now - requestTime).TotalMinutes);

        // Allow 5 minutes clock skew
        if (timeDifference > 5)
        {
            _logger.LogWarning("Request timestamp outside acceptable range: {TimeDifference} minutes", timeDifference);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Request timestamp outside acceptable range" });
            return;
        }

        // Get client secret from configuration
        var clientSecret = _configuration[$"ClientSecrets:{clientId}"];
        if (string.IsNullOrEmpty(clientSecret))
        {
            _logger.LogWarning("Unknown client ID: {ClientId}", clientId.ToString());
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid client ID" });
            return;
        }

        // Read request body
        context.Request.EnableBuffering();
        var json = await new StreamReader(context.Request.Body).ReadToEndAsync();
        string bodyContent;

        if (string.IsNullOrWhiteSpace(json))
        {
            bodyContent = string.Empty;
        }
        else
        {
            var obj = JsonSerializer.Deserialize<object>(json);
            bodyContent = JsonSerializer.Serialize(obj);
        }

        context.Request.Body.Position = 0; // Reset stream position for next middleware

        // Calculate expected signature
        var method = context.Request.Method;
        var path = context.Request.Path + context.Request.QueryString;
        var dataToSign = $"{method}\n{path}\n{timestamp}\n{bodyContent}";
        var expectedSignature = ComputeHmacSha256(dataToSign, clientSecret);

        // Validate signature
        if (!string.Equals(signature, expectedSignature, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid signature for client: {ClientId}", clientId.ToString());
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid signature" });
            return;
        }

        _logger.LogInformation("Request validated for client: {ClientId}", clientId.ToString());
        await _next(context);
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);

        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(dataBytes);
        return Convert.ToBase64String(hashBytes);
    }
}
