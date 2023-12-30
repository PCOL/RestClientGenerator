namespace RestClient;

using System;
using System.Threading.Tasks;

/// <summary>
/// Defines the retry interface.
/// </summary>
public interface IRetry
{
    /// <summary>
    /// Executes an function with retry.
    /// </summary>
    /// <typeparam name="TResult">The result type.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <returns>The result of the function.</returns>
    Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> func);
}
