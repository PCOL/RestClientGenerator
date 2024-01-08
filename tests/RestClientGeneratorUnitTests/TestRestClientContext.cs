namespace RestClientGeneratorUnitTests;

using System;
using RestClient;

[RestClient(typeof(ISimpleTestClient))]
public partial class TestRestClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return default(T);
        }

        throw new NotImplementedException();
    }

    public override string Serialize<T>(T obj)
    {
        if (obj == null)
        {
            return null;
        }

        throw new NotImplementedException();
    }
}
