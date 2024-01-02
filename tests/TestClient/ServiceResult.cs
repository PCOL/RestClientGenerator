namespace TestClient;

using System.Text.Json.Serialization;

/// <summary>
/// Implemenetation of the <see cref="IServiceResult"/> interface.
/// </summary>
[JsonDerivedType(typeof(ServiceResult<string>))]
[JsonDerivedType(typeof(IServiceResult<string>))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
public class ServiceResult
    : IServiceResult
{
    /// <inheritdoc />
    public ServiceStatus Status { get; set; }

    /// <inheritdoc />
    public bool IsError { get; set; }

    /// <inheritdoc />
    public IErrorModel Error { get; set; }
}