namespace TestClient;

using RestClient;

/// <summary>
/// Represents the rest client context.
/// </summary>
[RestClient(typeof(IClient))]
[RestClient(typeof(ITokenClient))]
public partial class ClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string json)
    {
        return new JsonContext().Deserialize<T>(json);
    }

    public override string Serialize<T>(T obj)
    {
        return new JsonContext().Serialize<T>(obj);
    }
}
