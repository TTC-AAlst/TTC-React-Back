using System.Diagnostics;
using Newtonsoft.Json;
using Ttc.Model.Core;

namespace Ttc.WebApi.Utilities;

public class RequestLoggingFilter
{
    private readonly RequestDelegate _next;
    private readonly TtcLogger _logger;

    public RequestLoggingFilter(RequestDelegate next, TtcLogger logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (!context.Request.Path.ToString().StartsWith("/api") || context.Request.Method == HttpMethods.Options)
        {
            await _next(context);
            return;
        }

        var timer = Stopwatch.StartNew();
        context.Request.EnableBuffering();
        var request = context.Request;

        string body = "";
        if (request.Method != HttpMethods.Get && request.Method != HttpMethods.Delete && request.ContentLength > 0 
            && request.Path != "/api/User/sign-in" && request.Path != "/api/users/ValidateToken")
        {
            request.Body.Position = 0;
            using var reader = new StreamReader(request.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        var qs = request.Query.ToDictionary(q => q.Key, q => q.Value.ToString());
        var queryParams = JsonConvert.SerializeObject(qs);

        if (qs.Count > 0 && body.Length > 0)
        {
            _logger.Information($"{request.Method} {request.Path} - Query: {queryParams}, Body: {body}");
        }
        else if (qs.Count > 0)
        {
            _logger.Information($"{request.Method} {request.Path} - Query: {queryParams}");
        }
        else if (body.Length > 0)
        {
            _logger.Information($"{request.Method} {request.Path} - Body: {body}");
        }
        else
        {
            _logger.Information($"{request.Method} {request.Path}");
        }


        await _next(context);


        _logger.Information($"{request.Method} {request.Path} - in {timer.Elapsed:g}");
    }
}