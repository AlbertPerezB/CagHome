using System.Net;

namespace CagHome.NotificationService.Tests.Helpers;

/// <summary>
/// A fake HTTP handler for simulating responses from an EHR system. It allows tests to specify the HTTP
/// status code that should be returned and keeps track of the last request made and how many times it was called.
/// </summary>
public class FakeEhrHttpHandler : HttpMessageHandler
{
    private HttpStatusCode _statusCode = HttpStatusCode.OK;
    public HttpRequestMessage? LastRequest { get; private set; }
    public int CallCount { get; private set; }

    /// <summary>
    /// Sets the HTTP status code for the response.
    /// </summary>
    /// <param name="statusCode">The HTTP status code to set for the response. Specify a value from the <see cref="System.Net.HttpStatusCode"/>
    /// enumeration.</param>
    public void RespondWith(HttpStatusCode statusCode) => _statusCode = statusCode;

    /// <summary>
    /// Sends an HTTP request asynchronously and returns the response message.
    /// </summary>
    /// <param name="request">The HTTP request message to send. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task result containing the HTTP response message.</returns>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        LastRequest = request;
        CallCount++;
        return Task.FromResult(new HttpResponseMessage(_statusCode));
    }

    /// <summary>
    /// Resets the state of the object to its initial values.
    /// </summary>
    public void Reset()
    {
        LastRequest = null;
        CallCount = 0;
        _statusCode = HttpStatusCode.OK;
    }
}
