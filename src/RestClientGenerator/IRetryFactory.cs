namespace RestClient;

using System;
using System.Net;

/// <summary>
/// Defines the retry factory interface.
/// </summary>
public interface IRetryFactory
{
    /// <summary>
    /// Creates a retry.
    /// </summary>
    /// <param name="retryLimit">The retry limit.</param>
    /// <param name="waitTime">The amount of time to wait between retries.</param>
    /// <param name="doubleOnRetry">A value indicating whether ot not the wait time doubles on each retry.</param>
    /// <returns>A <see cref="IRetry"/>.</returns>
    IRetry CreateRetry(
        int retryLimit,
        TimeSpan waitTime,
        TimeSpan maxWaitTime,
        bool doubleOnRetry,
        HttpStatusCode[] httpStatusCodes,
        Type[] exceptionTypes);
}
