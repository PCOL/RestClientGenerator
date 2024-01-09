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
    /// Posts a widget.
    /// </summary>
    /// <param name="model">The model to put.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Post("api/v1/widget")]
    HttpResponseMessage PostWidget(
        [SendAsContent]
        PostModel model);

    /// <summary>
    /// Posts a widget (async).
    /// </summary>
    /// <param name="model">The model to put.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent]
        PostModel model);

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

    /// <summary>
    /// Puts a widget (async).
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <param name="model">The model to put.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Put("api/v1/widget/{name}/name")]
    Task<HttpResponseMessage> PutWidgetAsync(
        string name,
        [SendAsContent]
        PutModel model);

    /// <summary>
    /// Patches a widget.
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <param name="model">The model to patch.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Patch("api/v1/widget/{name}/name")]
    HttpResponseMessage PatchWidget(
        string name,
        [SendAsContent]
        PatchModel model);

    /// <summary>
    /// Patches a widget (async).
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <param name="model">The model to patch.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Patch("api/v1/widget/{name}/name")]
    Task<HttpResponseMessage> PatchWidgetAsync(
        string name,
        [SendAsContent]
        PatchModel model);

    /// <summary>
    /// Deletes a widget.
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Delete("api/v1/widget/{name}/name")]
    HttpResponseMessage DeleteWidget(string name);

    /// <summary>
    /// Deletes a widget (async).
    /// </summary>
    /// <param name="name">The name of the widget.</param>
    /// <returns>A <see cref="HttpResponseMessage"/>.</returns>
    [Delete("api/v1/widget/{name}/name")]
    Task<HttpResponseMessage> DeleteWidgetAsync(string name);
}
