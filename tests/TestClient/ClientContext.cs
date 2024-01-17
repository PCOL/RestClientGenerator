namespace TestClient;

using RestClient;
using TestClient.Services;

/// <summary>
/// Represents the rest client context.
/// </summary>
[OutputCode]
[RestClient(typeof(IClient))]
[RestClient(typeof(ITokenClient))]
public partial class ClientContext
    : RestClientContext
{
    /// <inheritdoc/>
    public override T Deserialize<T>(string content)
    {
        return new JsonContext().Deserialize<T>(content);
    }

    /// <inheritdoc/>
    public override string Serialize<T>(T obj)
    {
        return new JsonContext().Serialize(obj);
    }
}
