namespace RestClient;

using System;
using System.Net.Http;

public abstract class RestClientContext
{
    private RestClientOptions options;

    private HttpClient httpClient;

    public RestClientOptions Options
    {
        get => this.options ??= new RestClientOptions();

        set
        {
            this.options = value;
        }
    }

    ////public TClient GetClient<TClient>()
    ////{
    ////    return (TClient)GetClient(typeof(TClient));
    ////}

    ////public object GetClient(Type clientType)
    ////{
    ////    clientType.ThrowIfArgumentNull(nameof(clientType));
    ////    clientType.ThrowIfNotInterface(nameof(clientType));

    ////    return null;
    ////}

    /// <summary>
    /// Gets the <see cref="HttpClient"/>.
    /// </summary>
    /// <returns>A <see cref="HttpClient"/>.</returns>
    public HttpClient GetHttpClient()
    {
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

    public abstract string Serialize<T>(T obj);

    public abstract T Deserialize<T>(string json);
}