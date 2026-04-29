using System.Net;
using System.Text;
using System.Text.Json;

namespace CagHome.EhrIntegrationService.Tests.Helpers;

public class FakeEhrHttpHandler : HttpMessageHandler
{
    private string? _jsonResponse;
    private Exception? _exception;

    public void RespondWithJson<T>(T body)
    {
        _jsonResponse = JsonSerializer.Serialize(body);
        _exception = null;
    }

    public void ThrowOnNextRequest(Exception ex)
    {
        _exception = ex;
        _jsonResponse = null;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_exception is not null)
            throw _exception;

        return Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    _jsonResponse ?? "[]",
                    Encoding.UTF8,
                    "application/json"
                ),
            }
        );
    }
}
