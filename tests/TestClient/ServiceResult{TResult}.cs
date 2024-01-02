namespace TestClient;

using System.Text.Json.Serialization;

/// <summary>
/// Implemenetation of the <see cref="IServiceResult{TResult}"/> interface.
/// </summary>
/// <typeparam name="TResult">The result type.</typeparam>
public class ServiceResult<TResult>
    : ServiceResult,
    IServiceResult<TResult>
{
    /// <inheritdoc />
    public TResult Result { get; set; }
}