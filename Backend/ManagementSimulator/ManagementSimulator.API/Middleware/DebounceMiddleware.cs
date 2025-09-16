using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ManagementSimulator.API.Middleware
{
    public class DebounceMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new();
        private readonly TimeSpan _debounceInterval = TimeSpan.FromMilliseconds(500); // 500ms debounce

        public DebounceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.Value != null && context.Request.Path.Value.Contains("search", StringComparison.OrdinalIgnoreCase))
            {
                var key = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var now = DateTime.UtcNow;
                if (_lastRequestTimes.TryGetValue(key, out var lastTime))
                {
                    if (now - lastTime < _debounceInterval)
                    {
                        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        await context.Response.WriteAsync("Too many requests. Please wait before searching again.");
                        return;
                    }
                }
                _lastRequestTimes[key] = now;
            }
            await _next(context);
        }
    }
}
