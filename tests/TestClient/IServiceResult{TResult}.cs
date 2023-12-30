namespace TestClient;

/// <summary>
/// Defines the service result interface.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IServiceResult<TResult>
    : IServiceResult
{
    /// <summary>
    /// Gets the result.
    /// </summary>
    TResult Result { get; }
}