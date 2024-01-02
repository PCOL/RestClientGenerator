namespace RestClient;

/// <summary>
/// Specifies the the parameter should be sent as an authorization header.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class AuthorizationHeaderAttribute
    : SendAsHeaderAttribute
{
    /// <summary>
    /// Initialises a new instance of the <see cref="AddAuthorizationHeaderAttribute"/> class.
    /// </summary>
    /// <param name="scheme">The authorization scheme.</param>
    public AuthorizationHeaderAttribute(string scheme = "basic")
        : base("Authorization")
    {
        this.Scheme = scheme;
    }
    
    /// <summary>
    /// Gets or sets the authorization scheme.
    /// </summary>
    public string Scheme { get; set; }
}
