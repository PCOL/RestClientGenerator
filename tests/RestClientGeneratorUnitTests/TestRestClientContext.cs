namespace RestClientGeneratorUnitTests;

using RestClient;

[RestClient(typeof(ISimpleTestClient))]
[RestClient(typeof(IGetClient))]
[RestClient(typeof(IPostClient))]
public partial class TestRestClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return default(T);
        }

        return new JsonContext().Deserialize<T>(content);
    }

    public override string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return null;
        }

        return new JsonContext().Serialize<T>(obj);
    }
}
