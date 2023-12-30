namespace TestClient;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for Http related classes.
/// </summary>
public static class HttpExtensionMethods
{
    /// <summary>
    /// Sets the content on a http request.
    /// </summary>
    /// <param name="request">A <see cref="HttpRequestMessage"/> instance.</param>
    /// <param name="model">A object model instance.</param>
    public static void SetContent<T>(this HttpRequestMessage request, T model)
    {
        if (model != null)
        {
            var json = JsonSerializer.Serialize<T>(
                model,  
                new JsonContext().GetTypeInfo(typeof(T)) as JsonTypeInfo<T>);

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }
    }

    /// <summary>
    /// Gets the error message from a response.
    /// </summary>
    /// <param name="response">The response.</param>
    /// <param name="defaultValue">The value to return if an error message was not found.</param>
    /// <returns>The error message.</returns>
    public static async Task<string> GetResponseErrorMessageAsync(
        this HttpResponseMessage response,
        string defaultValue = "Unknown error")
    {
        var errorModel = await response.GetResponseModelAsync<ErrorModel>();
        if (errorModel != null &&
            string.IsNullOrEmpty(errorModel.Message) == false)
        {
            return errorModel.Message;
        }

        return defaultValue;
    }

    /// <summary>
    /// Gets the content model from a response.
    /// </summary>
    /// <typeparam name="TResult">The content model type.</typeparam>
    /// <param name="response">The response.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The content if any; otherwise the default value is returned.</returns>
    public static async Task<TResult> GetResponseModelAsync<TResult>(
        this HttpResponseMessage response,
        TResult defaultValue = default)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(content) == false)
        {
            try
            {
                return JsonSerializer.Deserialize<TResult>(
                    content,
                    new JsonContext().GetTypeInfo(typeof(TResult)) as JsonTypeInfo<TResult>);
            }
            catch
            {
            }
        }

        return defaultValue;
    }

    /// <summary>
    /// Checks if the operation should be retried.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
    /// <returns>True if it should retry; otherwise false.</returns>
    public static async Task<bool> ShouldRetryAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode == true ||
            response.StatusCode == HttpStatusCode.InternalServerError ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            return false;
        }
        else if (response.StatusCode == (HttpStatusCode)429)
        {
            return true;
        }
        else if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return response.GetServiceResponseHeader() == null;
        }
        else
        {
            var errorModel = await response.GetResponseModelAsync<ErrorModel>();
            if (errorModel == null || errorModel.Retry == false)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the service response header.
    /// </summary>
    /// <param name="response">The response message.</param>
    /// <returns>The response header if found; otherwise null.</returns>
    public static string GetServiceResponseHeader(this HttpResponseMessage response)
    {
        if (response.Headers.TryGetValues("x-service-response", out IEnumerable<string> values) == true)
        {
            return values.FirstOrDefault();
        }

        return null;
    }
}
