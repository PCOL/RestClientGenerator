namespace TestClient;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RestClient;

internal class GetWorkload
{
    private readonly HttpClient httpClient;

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = null;
        var retry = this.CreateRetry();
        if (retry != null)
        {
            response = await retry
                .ExecuteAsync(
                    () =>
                    {
                        return this.SendAsync(cancellationToken);
                    })
                .ConfigureAwait(false);
        }
        else
        {
            response = await this
                .SendAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        response.EnsureSuccessStatusCode();

        return "Test";
    }


    private async Task<HttpResponseMessage> SendAsync(CancellationToken cancellationToken)
    { 
        using (var request = this.CreateRequest())
        {
            return await this.httpClient
                .SendAsync(request)
                .ConfigureAwait(false);
        }
    }

    private HttpRequestMessage CreateRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost.com/");
        request.Headers.Add("Accept", "application/json");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "token");
        return request;
    }

    private IRetry CreateRetry()
    {
        return null;
    }
}
