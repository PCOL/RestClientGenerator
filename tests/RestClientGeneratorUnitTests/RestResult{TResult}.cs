namespace RestClientGeneratorUnitTests;

/// <summary>
/// Implemenetation of the <see cref="IRestResult{TResult}"/> interface.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public class RestResult<TResult>
    : RestResult,
    IRestResult<TResult>
{
    /// <inheritdoc />
    public TResult Result { get; set; }
}