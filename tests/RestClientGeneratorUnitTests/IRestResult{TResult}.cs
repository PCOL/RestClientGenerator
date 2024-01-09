namespace RestClientGeneratorUnitTests;

/// <summary>
/// Defines the service result interface.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IRestResult<TResult>
    : IRestResult
{
    /// <summary>
    /// Gets the result.
    /// </summary>
    TResult Result { get; }
}