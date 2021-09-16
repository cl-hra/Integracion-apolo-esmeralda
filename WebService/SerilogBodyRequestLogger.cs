using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace WebService
{
    public class SerilogBodyRequestLogger
    {
        private readonly RequestDelegate _next;

        public SerilogBodyRequestLogger(RequestDelegate next)
        {
            if (next == null) throw new ArgumentNullException(nameof(next));

            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) throw new ArgumentNullException(nameof(httpContext));

            LogContext.PushProperty("Body", await ReadBodyFromRequest(httpContext.Request));
            await _next(httpContext);
        }

        private async Task<string> ReadBodyFromRequest(HttpRequest httpRequest)
        {
            httpRequest.EnableBuffering();
            using var streamReader = new StreamReader(httpRequest.Body, leaveOpen: true);
            var requestBody = await streamReader.ReadToEndAsync();

            httpRequest.Body.Position = 0;
            return requestBody;
        }
    }
}
