namespace TestClient;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the service result interface.
/// </summary>
[JsonDerivedType(typeof(ServiceResult<string>))]
[JsonDerivedType(typeof(IServiceResult<string>))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
public interface IServiceResult
{
    /// <summary>
    /// Gets the service status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    ServiceStatus Status { get; }

    /// <summary>
    /// Gets a value indicating whether or not the service request resulted in an error.
    /// </summary>
    bool IsError { get; }

    /// <summary>
    /// Gets the error.
    /// </summary>
    IErrorModel Error { get; }
}