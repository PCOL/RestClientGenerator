namespace RestClientGeneratorUnitTests;

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RestClient;

[HttpClientContract]
public interface IPostClient
{
    [OutputCode]
    [Post("api/v1/widgets")]
    Task<HttpResponseMessage> PostWidgetAsync(
        [SendAsContent]Func<MemoryStream> func);
}
