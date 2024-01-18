namespace RestClient;

using System;
using System.Net.Http;

/// <summary>
/// Represents the rest client options.
/// </summary>
public class RestClientOptions
{
    /// <summary>
    /// Gets or sets the base url.
    /// </summary>
    public string BaseUrl { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="HttpClient"/> to use.
    /// </summary>
    public HttpClient HttpClient { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IAuthorizationHeaderFactory"/> to use.
    /// </summary>
    public IAuthorizationHeaderFactory AuthorizationHeaderFactory { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IRetryFactory"/> to use.
    /// </summary>
    public IRetryFactory RetryFactory { get; set; }

    /// <summary>
    /// Gets or sets a <see cref="IServiceProvider"/> for use with Dependency injection.
    /// </summary>
    public IServiceProvider Services { get; set; }

    /// <summary>
    /// Gets a service.
    /// </summary>
    /// <typeparam name="T">The service type.</typeparam>
    /// <returns>An instance of the type if found; otherwise null.</returns>
    internal T GetService<T>()
    {
        return (T)this.Services?.GetService(typeof(T));
    }

    /// <summary>
    /// Gets a <see cref="IAuthorizationHeaderFactory"/> if set.
    /// </summary>
    /// <returns>A <see cref="IAuthorizationHeaderFactory"/> if set; otherwise null.</returns>
    public IAuthorizationHeaderFactory GetAuthorizationHeaderFactory()
    {
        if (this.AuthorizationHeaderFactory != null)
        {
            return this.AuthorizationHeaderFactory;
        }

        return this.GetService<IAuthorizationHeaderFactory>();
    }
}
