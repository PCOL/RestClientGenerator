namespace RestClientGeneratorUnitTests;

using RestClient;

[HttpClientContract]
public interface ISimpleTestClient
{
    [Get("api/v1/widget/{name}/name")]
    HttpResponseMessage GetWidget(string name);
}
