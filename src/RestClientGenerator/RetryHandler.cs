namespace RestClient;

using System;
using System.Threading.Tasks;

/// <summary>
/// The exception handler base class.
/// </summary>
public abstract class RetryHandler
{
    /// <summary>
    /// Determines whether or not the handler matches.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>True if it matches; otherwise false.</returns>
    public abstract bool IsMatch(Type type);

    /// <summary>
    /// Determines whether or not an operation should be retried.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns>True if it should be retried; otherwise false.</returns>
    public abstract Task<bool> ShouldRetryAsync(object obj);
}
