namespace RestClient;

using System.Net.Http;

/// <summary>
/// 
/// </summary>
public abstract class RestClientContext
{
    /// <summary>
    /// The REST client optins.
    /// </summary>
    private RestClientOptions options;

    /// <summary>
    /// The REST client <see cref="HttpClient"/>.
    /// </summary>
    private HttpClient httpClient;

    /// <summary>
    /// Gets or set the client options.
    /// </summary>
    public RestClientOptions Options
    {
        get => this.options ??= new RestClientOptions();

        set
        {
            this.options = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="name">A client name.</param>
    /// <returns>A <see cref="HttpClient"/>.</returns>
    public HttpClient GetHttpClient(string name = "")
    {
        var httpClientFactory = this.options.GetHttpClientFactory();    
        if (httpClientFactory != null)
        {
            var client = httpClientFactory.CreateClient(name);
            if (client != null)
            {
                return client;
            }
        }
            
        if (this.httpClient == null)
        {
            lock (this)
            {
                if (this.httpClient == null)
                {

                    this.httpClient = this.options.HttpClient ?? new HttpClient();
                }
            }
        }

        return this.httpClient;
    }

    /// <summary>
    /// Serializes an object.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A string representation of the object.</returns>
    public abstract string Serialize<T>(T obj);

    /// <summary>
    /// Deserializes an object.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="content">The content to deserialize.</param>
    /// <returns>An object representation of the content.</returns>
    public abstract T Deserialize<T>(string content);

    /// <summary>
    /// Gets the retry factory.
    /// </summary>
    /// <returns>The retry factory.</returns>
    public virtual IRetryFactory GetRetryFactory()
    {
        if (this.options.RetryFactory != null)
        {
            return this.options.RetryFactory;
        }

        return new DefaultRetryFactory();
    }
}