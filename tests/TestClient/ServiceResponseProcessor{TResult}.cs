namespace TestClient;


using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using RestClient;

/// <summary>
/// A service result response provider.
/// </summary>
/// <typeparam name="TResult">The content type.</typeparam>
public class ServiceResponseProcessor<TResult>
    : HttpResponseProcessor<IServiceResult<TResult>>
{
    /// <inheritdoc />
    public override async Task<IServiceResult<TResult>> ProcessResponseAsync(HttpResponseMessage response)
    {
        var result = new ServiceResult<TResult>();
        if (response.IsSuccessStatusCode == true)
        {
            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content) == false)
                {
                    result.Result = new JsonContext().Deserialize<TResult>(content);
                }
            }

            result.IsError = false;
            result.Status = response.StatusCode.ToStatus();
        }
        else
        {
            result.IsError = true;
            var errorModel = await response.GetResponseModelAsync<ErrorModel>();

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
                result.Error = errorModel ?? new ErrorModel() { Message = "Bad Request" };
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                result.Status = ServiceStatus.Unauthorized;
                result.Error = errorModel ?? new ErrorModel() { Message = "Unauthorized" };
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                result.Status = ServiceStatus.Forbidden;
                result.Error = errorModel ?? new ErrorModel() { Message = "Forbidden" };
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                result.Status = ServiceStatus.ResourceExists;
                result.Error = errorModel ?? new ErrorModel() { Message = "Resource exists" };
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                result.Status = ServiceStatus.TooManyRequests;
                result.Error = errorModel ?? new ErrorModel() { Message = "Too Many Requests" };
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                result.Status = ServiceStatus.ServiceUnavailable;
                result.Error = errorModel ?? new ErrorModel() { Message = "Service unavailable" };
            }
            else
            {
                result.Status = ServiceStatus.Error;
                result.Error = errorModel ?? new ErrorModel() { Message = response.ReasonPhrase };
            }
        }

        return result;
    }

    /*
            public override async Task<bool> ShouldRetryAsync(HttpResponseMessage response)
            {
                if (response.StatusCode == (HttpStatusCode)429)
                {
                    return true;
                }
                else if (response.IsSuccessStatusCode == false &&
                    response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var error = JsonConvert.DeserializeObject<ErrorModel>(content);
                    return error.Retry.GetValueOrDefault(false);
                }

                return false;
            }
    */
}