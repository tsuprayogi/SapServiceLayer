using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public class SapSessionHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SapSessionHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

   protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
{
    var user = _httpContextAccessor.HttpContext?.User;

    var sessionId = user?.FindFirst("SapSessionId")?.Value;
    var routeId = user?.FindFirst("RouteId")?.Value;

    if (!string.IsNullOrWhiteSpace(sessionId))
    {
        request.Headers.Remove("Cookie");

        var cookieHeader = $"B1SESSION={sessionId}";
        if (!string.IsNullOrWhiteSpace(routeId))
        {
            cookieHeader += $"; RouteId={routeId}";
        }

        request.Headers.Add("Cookie", cookieHeader);
    }

    return await base.SendAsync(request, cancellationToken);
}
}