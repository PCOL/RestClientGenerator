namespace RestClientGeneratorUnitTests;

using RestClient;

[RestClient(typeof(ISimpleTestClient))]
public partial class TestRestClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string content)
    {
        throw new NotImplementedException();
    }

    public override string Serialize<T>(T obj)
    {
        throw new NotImplementedException();
    }
}
