namespace RestClient;

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Represents a default retry factory.
/// </summary>
internal class DefaultRetryFactory
    : IRetryFactory
{
    /// <inheritdoc/>
    public IRetry CreateRetry(
        int retryLimit,
        TimeSpan waitTime,
        TimeSpan maxWaitTime,
        bool doubleOnRetry,
        HttpStatusCode[] httpStatusCodes,
        Type[] exceptionTypes)
    {
        var retry = new Retry()
            .SetRetryLimit(retryLimit)
            .SetWaitTime(waitTime)
            .SetMaxWaitTime(maxWaitTime)
            .SetDoubleWaitTimeOnRetry(doubleOnRetry);

        if (exceptionTypes != null)
        {
            retry.AddExceptionHandler<Exception>(
                (ex) =>
                {
                    return Task.FromResult(exceptionTypes.Contains(ex.GetType()));
                });
        }

        if (httpStatusCodes != null)
        {
            retry.AddResultHandler<HttpResponseMessage>(
                (response) =>
                {
                    return Task.FromResult(httpStatusCodes.Contains(response.StatusCode));
                });
        }

        return retry;
    }
}