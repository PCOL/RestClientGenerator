namespace TestClient;

using System.Threading.Tasks;
using RestClient;

public class AuthorizationFactory
    : IAuthorizationHeaderFactory
{
    public string GetAuthorizationHeaderScheme()
    {
        return "bearer";
    }

    public string GetAuthorizationHeaderValue()
    {
        return "bearer token";
    }

    public Task<string> GetAuthorizationHeaderValueAsync()
    {
        return Task.FromResult("bearer token");
    }
}