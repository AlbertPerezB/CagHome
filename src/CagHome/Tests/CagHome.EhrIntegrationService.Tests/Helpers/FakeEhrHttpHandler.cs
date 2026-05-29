using System.Net;
using System.Text;
using System.Text.Json;

namespace CagHome.EhrIntegrationService.Tests.Helpers;

/// <summary>
/// A fake HTTP handler for simulating responses from an EHR system.
/// </summary>
public class FakeEhrHttpHandler : HttpMessageHandler
{
    private string? _jsonResponse;
    private Exception? _exception;
    public Uri? LastRequestUri { get; private set; }

    /// <summary>
    /// Method to set the JSON response that will be returned on the next request.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize to JSON.</typeparam>
    /// <param name="body">The object to serialize and return as the response.</param>
    public void RespondWithJson<T>(T body)
    {
        _jsonResponse = JsonSerializer.Serialize(body);
        _exception = null;
    }

    /// <summary>
    /// Configures the next request to throw the specified exception instead of returning a normal response.
    /// </summary>
    /// <param name="ex">The exception to be thrown on the next request. Cannot be null.</param>
    public void ThrowOnNextRequest(Exception ex)
    {
        _exception = ex;
        _jsonResponse = null;
    }

    /// <summary>
    /// Sends an HTTP request asynchronously and returns a response message.
    /// </summary>
    /// <param name="request">The HTTP request message to send. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task containing the HTTP response message.</returns>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        LastRequestUri = request.RequestUri;

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
