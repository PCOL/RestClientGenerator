namespace RestClient;

using System;
using System.Threading.Tasks;

/// <summary>
/// Represents an exception handler.
/// </summary>
/// <typeparam name="T">The exception type.</typeparam>
public class RetryHandler<T>
    : RetryHandler
{
    /// <summary>
    /// The handler function.
    /// </summary>
    private readonly Func<T, Task<bool>> handler;

    /// <summary>
    /// Initialises a new instance of the <see cref="RetryHandler{T}"/> class.
    /// </summary>
    /// <param name="handler">The handler function.</param>
    public RetryHandler(Func<T, Task<bool>> handler)
    {
        ////handler.ThrowIfNull(nameof(handler));

        this.handler = handler;
    }

    /// <inheritdoc />
    public override bool IsMatch(Type type)
    {
        return typeof(T).IsAssignableFrom(type);
    }

    /// <inheritdoc />
    public override Task<bool> ShouldRetryAsync(object obj)
    {
        return this.handler((T)obj);
    }
}
