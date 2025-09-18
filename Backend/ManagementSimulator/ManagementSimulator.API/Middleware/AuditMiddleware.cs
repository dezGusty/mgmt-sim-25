using ManagementSimulator.Core.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace ManagementSimulator.API.Middleware
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        private static readonly HashSet<string> _excludedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/api/auth/me",
            "/health",
            "/swagger",
            "/favicon.ico"
        };

        private static readonly HashSet<string> _readOnlyMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "GET",
            "HEAD",
            "OPTIONS"
        };

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
        {
            // Skip audit logging for certain paths and methods
            if (ShouldSkipAudit(context))
            {
                await _next(context);
                return;
            }

            var requestBody = string.Empty;
            var originalBodyStream = context.Response.Body;

            try
            {
                // Capture request body for non-GET requests
                if (!_readOnlyMethods.Contains(context.Request.Method))
                {
                    requestBody = await CaptureRequestBodyAsync(context);
                }

                // Create a memory stream to capture response
                using var responseBodyStream = new MemoryStream();
                context.Response.Body = responseBodyStream;

                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                Exception? caughtException = null;

                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    caughtException = ex;
                    throw;
                }
                finally
                {
                    stopwatch.Stop();

                    // Reset response body stream position
                    responseBodyStream.Seek(0, SeekOrigin.Begin);

                    // Copy response back to original stream
                    await responseBodyStream.CopyToAsync(originalBodyStream);

                    // Log the audit entry
                    await LogRequestAsync(
                        context,
                        auditLogService,
                        requestBody,
                        stopwatch.ElapsedMilliseconds,
                        caughtException);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in audit middleware");
                context.Response.Body = originalBodyStream;
                throw;
            }
        }

        private bool ShouldSkipAudit(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // Skip read-only methods (GET, HEAD, OPTIONS)
            if (_readOnlyMethods.Contains(method))
            {
                return true;
            }

            // Skip certain paths
            if (_excludedPaths.Any(excluded => path.StartsWith(excluded, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            // Skip static files
            if (path.Contains('.') && (path.EndsWith(".css") || path.EndsWith(".js") || path.EndsWith(".png") || path.EndsWith(".jpg")))
            {
                return true;
            }

            // Skip audit logs endpoint to prevent recursion
            if (path.StartsWith("/api/auditlogs", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private async Task<string> CaptureRequestBodyAsync(HttpContext context)
        {
            try
            {
                context.Request.EnableBuffering();

                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to capture request body");
                return string.Empty;
            }
        }

        private async Task LogRequestAsync(
            HttpContext context,
            IAuditLogService auditLogService,
            string requestBody,
            long elapsedMilliseconds,
            Exception? exception)
        {
            try
            {
                var action = DetermineAction(context);
                var success = exception == null && context.Response.StatusCode < 400;
                var errorMessage = exception?.Message;

                var additionalData = new
                {
                    RequestBody = !string.IsNullOrEmpty(requestBody) ? TrySanitizeRequestBody(requestBody) : null,
                    StatusCode = context.Response.StatusCode,
                    ElapsedMilliseconds = elapsedMilliseconds,
                    ContentType = context.Request.ContentType,
                    UserAgent = context.Request.Headers["User-Agent"].FirstOrDefault(),
                    Referer = context.Request.Headers["Referer"].FirstOrDefault()
                };

                await auditLogService.LogHttpRequestAsync(
                    context,
                    action,
                    success,
                    errorMessage,
                    additionalData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log audit entry for request {Method} {Path}",
                    context.Request.Method, context.Request.Path);
            }
        }

        private string DetermineAction(HttpContext context)
        {
            var method = context.Request.Method.ToUpperInvariant();
            var path = context.Request.Path.Value ?? string.Empty;

            // More specific action determination based on endpoint patterns
            if (path.Contains("/login", StringComparison.OrdinalIgnoreCase))
                return "LOGIN_ATTEMPT";
            if (path.Contains("/logout", StringComparison.OrdinalIgnoreCase))
                return "LOGOUT";
            if (path.Contains("/impersonate", StringComparison.OrdinalIgnoreCase))
                return "IMPERSONATE";
            if (path.Contains("/stop-impersonation", StringComparison.OrdinalIgnoreCase))
                return "STOP_IMPERSONATION";

            // Generic HTTP method actions
            return method switch
            {
                "GET" => "READ",
                "POST" => "CREATE",
                "PUT" => "UPDATE",
                "PATCH" => "UPDATE",
                "DELETE" => "DELETE",
                _ => "HTTP_REQUEST"
            };
        }

        private object? TrySanitizeRequestBody(string requestBody)
        {
            try
            {
                var jsonDocument = JsonDocument.Parse(requestBody);
                var sanitized = SanitizeJsonElement(jsonDocument.RootElement);
                return sanitized;
            }
            catch
            {
                // If it's not valid JSON or too large, just return a placeholder
                return requestBody.Length > 1000 ?
                    $"[Request body too large: {requestBody.Length} characters]" :
                    "[Non-JSON request body]";
            }
        }

        private object SanitizeJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object>();
                    foreach (var property in element.EnumerateObject())
                    {
                        var key = property.Name.ToLowerInvariant();

                        // Sanitize sensitive fields
                        if (key.Contains("password") || key.Contains("token") || key.Contains("secret"))
                        {
                            obj[property.Name] = "[REDACTED]";
                        }
                        else
                        {
                            obj[property.Name] = SanitizeJsonElement(property.Value);
                        }
                    }
                    return obj;

                case JsonValueKind.Array:
                    return element.EnumerateArray().Select(SanitizeJsonElement).ToArray();

                case JsonValueKind.String:
                    return element.GetString() ?? string.Empty;

                case JsonValueKind.Number:
                    return element.TryGetInt64(out var longValue) ? longValue : element.GetDouble();

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                case JsonValueKind.Null:
                    return null;

                default:
                    return element.ToString();
            }
        }
    }
}