namespace RestClientGeneratorUnitTests;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RestClient;

[HttpClientContract]
public interface IPostClient
{
    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent]MemoryStream func);

    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent] Func<MemoryStream> func);

    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent] PostModel model,
        Action<HttpRequestMessage> requestAction);

    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent] PostModel model,
        Func<HttpRequestMessage, Task> requestFunc);

    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent] PostModel model,
        Action<HttpResponseMessage> responseAction);

    [Post("api/v1/widget")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent] PostModel model,
        Func<HttpResponseMessage, Task> responseFunc);
}
