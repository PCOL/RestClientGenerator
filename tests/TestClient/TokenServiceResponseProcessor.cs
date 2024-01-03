namespace TestClient;

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestClient;

/// <summary>
/// A token service result response provider.
/// </summary>
public class TokenServiceResponseProcessor
    : HttpResponseProcessor<IServiceResult<string>>
{
    /// <inheritdoc />
    public override async Task<IServiceResult<string>> ProcessResponseAsync(HttpResponseMessage response)
    {
        var result = new ServiceResult<string>();
        if (response.IsSuccessStatusCode == true)
        {
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync();
                var model = new JsonContext().Deserialize<JsonObject>(content);
                if (model.Properties.TryGetValue("access_token", out var value) == true)
                {
                    result.Result = value.ToString();
                }
            }

            result.IsError = false;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                result.Status = ServiceStatus.Success;
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                result.Status = ServiceStatus.SuccessNoData;
            }
            else if (response.StatusCode == HttpStatusCode.Accepted)
            {
                result.Status = ServiceStatus.Accepted;
            }
        }
        else
        {
            result.IsError = true;
            if (response.Content.Headers.ContentLength > 0)
            {
                var content = await response.Content.ReadAsStringAsync();

                try
                {
                    var model = new JsonContext().Deserialize<JsonObject>(content);
                    if (model.Properties.TryGetValue("error", out var message) == true)
                    {
                        result.Error = new ErrorModel() { Message = message.ToString() };
                    }
                }
                catch
                {
                    result.Error = new ErrorModel() { Message = content };
                }
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                if (response.GetServiceResponseHeader() == null)
                {
                    result.Status = ServiceStatus.ServiceNotFound;
                    result.Error = new ErrorModel() { Message = "Service not found" };
                }
                else
                {
                    result.Status = ServiceStatus.ResourceNotFound;
                    result.Error = new ErrorModel() { Message = "Resource not found" };
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                result.Status = ServiceStatus.BadRequest;
                result.Error = result.Error ?? new ErrorModel() { Message = "Bad Request" };
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                result.Status = ServiceStatus.Unauthorized;
                result.Error = result.Error ?? new ErrorModel() { Message = "Unauthorized" };
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                result.Status = ServiceStatus.Forbidden;
                result.Error = result.Error ?? new ErrorModel() { Message = "Forbidden" };
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                result.Status = ServiceStatus.ResourceExists;
                result.Error = result.Error ?? new ErrorModel() { Message = "Resource exists" };
            }
            else
            {
                result.Status = ServiceStatus.Error;
                result.Error = result.Error ?? new ErrorModel() { Message = response.ReasonPhrase };
            }
        }

        return result;
    }
}