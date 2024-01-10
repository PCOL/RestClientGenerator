namespace RestClientGeneratorUnitTests;

using System.Net.Http;
using System.Threading.Tasks;
using RestClient;

[HttpClientContract(ContentType = "application/json")]
public interface IGetClient
{
    /// <summary>
    /// Get widgets.
    /// </summary>
    /// <returns>A response.</returns>
    [Get("api/v1/widgets")]
    Task<HttpResponseMessage> GetWidgetsAsync();

    /// <summary>
    /// Get widgets.
    /// </summary>
    /// <param name="name">A name query parameter.</param>
    /// <returns>A response.</returns>
    [Get("api/v1/widgets")]
    Task<HttpResponseMessage> GetWidgetsAsync(
        [SendAsQuery("name")] string name);

    /// <summary>
    /// Get widgets.
    /// </summary>
    /// <param name="name">A name query parameter.</param>
    /// <param name="option">Options query parameter.</param>
    /// <returns>A response.</returns>
    [Get("api/v1/widgets")]
    Task<HttpResponseMessage> GetWidgetsAsync(
        [SendAsQuery("name")] string name,
        [SendAsQuery("option")] TestOptions option);
}
