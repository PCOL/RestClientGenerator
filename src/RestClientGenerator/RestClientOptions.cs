namespace RestClient;

using System.Net.Http;

/// <summary>
/// Represents the rest client options.
/// </summary>
public class RestClientOptions
{
    /// <summary>
    /// Gets or sets the base url.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="HttpClient"/> to use.
    /// </summary>
    public HttpClient HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IHttpClientFactory"/> to use.
    /// </summary>
    public IHttpClientFactory HttpClientFactory { get; set; }


}
