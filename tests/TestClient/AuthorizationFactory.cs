namespace TestClient;

using System.Threading.Tasks;
using RestClient;

public class AuthorizationFactory
    : IAuthorizationHeaderFactory
{
    private readonly string scheme;
    private readonly string token;

    public AuthorizationFactory()
    {
    }

    public AuthorizationFactory(string scheme, string token)
    {
        this.scheme = scheme;
        this.token = token;
    }

    public string GetAuthorizationHeaderScheme()
    {
        return this.scheme ?? "bearer";
    }

    public string GetAuthorizationHeaderValue()
    {
        return $"{this.scheme} {this.token}";
    }

    public Task<string> GetAuthorizationHeaderValueAsync()
    {
        return Task.FromResult($"{scheme} {token}");
    }
}