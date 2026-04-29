using System.Net;

namespace CagHome.NotificationService.Tests.Integration;

public class FakeEhrHttpHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    public HttpRequestMessage? LastRequest { get; private set; }
    public int CallCount { get; private set; }

    public void RespondWith(HttpStatusCode statusCode) => _statusCode = statusCode;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        LastRequest = request;
        CallCount++;
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }

    public void Reset()
    {
        LastRequest = null;
        CallCount = 0;
        _statusCode = HttpStatusCode.OK;
    }
}
