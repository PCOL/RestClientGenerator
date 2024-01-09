namespace RestClientGeneratorUnitTests;

using System.Text.Json.Serialization;

/// <summary>
/// Defines the service result interface.
/// </summary>
[JsonDerivedType(typeof(RestResult<string>))]
[JsonDerivedType(typeof(IRestResult<string>))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
public interface IRestResult
{
    /// <summary>
    /// Gets the service status.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    RestStatus Status { get; }

    /// <summary>
    /// Gets a value indicating whether or not the service request resulted in an error.
    /// </summary>
    bool IsError { get; }

    /// <summary>
    /// Gets the error.
    /// </summary>
    IErrorModel Error { get; }
}