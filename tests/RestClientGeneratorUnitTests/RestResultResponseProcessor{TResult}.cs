namespace RestClientGeneratorUnitTests;

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RestClient;

/// <summary>
/// A REST result response provider.
/// </summary>
/// <typeparam name="TResult">The content type.</typeparam>
public class RestResultResponseProcessor<TResult>
    : HttpResponseProcessor<IRestResult<TResult>>
{
    /// <inheritdoc />
    public override async Task<IRestResult<TResult>> ProcessResponseAsync(HttpResponseMessage response)
    {
        var result = new RestResult<TResult>();
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
                    result.Status = RestStatus.ServiceNotFound;
                    result.Error = new ErrorModel() { Message = "Service not found" };
                }
                else
                {
                    result.Status = RestStatus.ResourceNotFound;
                    result.Error = new ErrorModel() { Message = "Resource not found" };
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                result.Status = RestStatus.BadRequest;
                result.Error = errorModel ?? new ErrorModel() { Message = "Bad Request" };
            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                result.Status = RestStatus.Unauthorized;
                result.Error = errorModel ?? new ErrorModel() { Message = "Unauthorized" };
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                result.Status = RestStatus.Forbidden;
                result.Error = errorModel ?? new ErrorModel() { Message = "Forbidden" };
            }
            else if (response.StatusCode == HttpStatusCode.Conflict)
            {
                result.Status = RestStatus.ResourceExists;
                result.Error = errorModel ?? new ErrorModel() { Message = "Resource exists" };
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                result.Status = RestStatus.TooManyRequests;
                result.Error = errorModel ?? new ErrorModel() { Message = "Too Many Requests" };
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                result.Status = RestStatus.ServiceUnavailable;
                result.Error = errorModel ?? new ErrorModel() { Message = "Service unavailable" };
            }
            else
            {
                result.Status = RestStatus.Error;
                result.Error = errorModel ?? new ErrorModel() { Message = response.ReasonPhrase };
            }
        }

        return result;
    }
}