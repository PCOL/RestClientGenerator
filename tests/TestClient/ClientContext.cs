namespace TestClient;

using RestClient;

/// <summary>
/// Represents the rest client context.
/// </summary>
[RestClient(typeof(IClient))]
public partial class ClientContext
    : RestClientContext
{
}
