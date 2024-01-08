namespace RestClientGeneratorUnitTests;

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Test <see cref="HttpMessageHandler"/>.
/// </summary>
public class TestHttpMessageHandler : HttpMessageHandler
{
    /// <summary>
    /// Gets or sets a function to provider a response.
    /// </summary>
    public Func<HttpRequestMessage, HttpResponseMessage> Response { get; set; }

    /// <summary>
    /// Sends a request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A response.</returns>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Response?.Invoke(request));
    }
}
