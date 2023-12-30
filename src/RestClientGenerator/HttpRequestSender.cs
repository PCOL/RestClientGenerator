namespace RestClient;

using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Implementation of the <see cref="IHttpRequestSender"/> interface.
/// </summary>
public class HttpRequestSender
    : IHttpRequestSender
{
    /// <summary>
    /// The <see cref="HttpClient"/> to use.
    /// </summary>
    private readonly HttpClient httpClient;

    /// <summary>
    /// The request context.
    /// </summary>
    private readonly IHttpRequestContext requestContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpRequestSender"/> class.
    /// </summary>
    /// <param name="httpClient">A http client.</param>
    /// <param name="requestContext">A http request context.</param>
    public HttpRequestSender(
        HttpClient httpClient,
        IHttpRequestContext requestContext)
    {
        this.httpClient = httpClient;
        this.requestContext = requestContext;
    }

    /// <inheritdoc/>
    public async Task<HttpResponseMessage> SendAsync(
        IHttpRequestBuilder requestBuilder,
        HttpCompletionOption completionOption)
    {
        if (this.requestContext.RetryHandler != null)
        {
            return await this.requestContext.RetryHandler.ExecuteAsync<HttpResponseMessage>(
                () =>
                {
                    return this.SendAsync(
                        requestBuilder.Build(),
                        completionOption);
                })
                .ConfigureAwait(false);
        }

        return await this
            .SendAsync(
                requestBuilder.Build(),
                completionOption)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sends the request to the service calling pre and post actions if they are configured.
    /// </summary>
    /// <param name="request">The http request to send.</param>
    /// <param name="completionOption">The completion option.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    private async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        HttpCompletionOption completionOption)
    {
        this.requestContext.InvokeRequestAction(request);

        var response = await this.httpClient
            .SendAsync(
                request,
                completionOption,
                this.requestContext.GetCancellationToken())
            .ConfigureAwait(false);

        this.requestContext.InvokeResponseAction(response);

        return response;
    }
}