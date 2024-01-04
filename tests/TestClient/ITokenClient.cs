namespace TestClient;

using System.Threading.Tasks;
using RestClient;

/// <summary>
/// Defines the token client interface.
/// </summary>
[HttpClientContract(Route = "identity", ContentType = "application/json")]
public interface ITokenClient
{
    /// <summary>
    /// Gets a token.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="scopes">The scopes.</param>
    /// <returns>A token if successful; otherwise null.</returns>
    [Post("identity/connect/token")]
    [AddFormUrlEncodedProperty("grant_type", "client_credentials")]
    [AddAuthorizationHeader("Basic {apiKey}")]
    [HttpResponseProcessor(typeof(TokenServiceResponseProcessor))]
    ////[return: FromJson("access_token")]
    Task<IServiceResult<string>> GetTokenAsync(
        [AuthorizationHeader("Basic")]
        string apiKey,
        [SendAsFormUrl(Name = "scope")]
        string scopes = "platform cluster");
}