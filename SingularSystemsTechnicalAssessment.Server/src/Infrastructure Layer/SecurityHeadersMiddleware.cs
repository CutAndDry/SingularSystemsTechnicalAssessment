using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace SingularSystemsTechnicalAssessment.Server.src.Infrastructure_Layer
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Basic security headers; keep conservative to avoid breaking the SPA.
            context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
            context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
            context.Response.Headers.TryAdd("Referrer-Policy", "no-referrer-when-downgrade");
            context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
            // A simple CSP that allows the app's own origin and common CDN/inline styles if needed.
            context.Response.Headers.TryAdd("Content-Security-Policy", "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data:;");

            await _next(context);
        }
    }
}
