namespace RestClientGeneratorUnitTests;

using System.Net.Http;
using System.Threading.Tasks;
using RestClient;

[HttpClientContract]
public interface ISimpleTestClient
{
    /// <summary>
    /// Gets a widget.
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Get("api/v1/widget/{name}/name")]
    HttpResponseMessage GetWidget(string name);

    /// <summary>
    /// Gets a widget (async).
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Get("api/v1/widget/{name}/name")]
    Task<HttpResponseMessage> GetWidgetAsync(string name);

    /// <summary>
    /// Puts a widget.
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <param name="model">The model to put.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Put("api/v1/widget/{name}/name")]
    HttpResponseMessage PutWidget(
        string name,
        [SendAsContent]
        PutModel model);
}
