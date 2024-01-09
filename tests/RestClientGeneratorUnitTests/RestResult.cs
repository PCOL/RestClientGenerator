namespace RestClientGeneratorUnitTests;

using System.Text.Json.Serialization;

/// <summary>
/// Implemenetation of the <see cref="IServiceResult"/> interface.
/// </summary>
[JsonDerivedType(typeof(RestResult<string>))]
[JsonDerivedType(typeof(IRestResult<string>))]
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToBaseType)]
public class RestResult
    : IRestResult
{
    /// <inheritdoc />
    public RestStatus Status { get; set; }

    /// <inheritdoc />
    public bool IsError { get; set; }

    /// <inheritdoc />
    public IErrorModel Error { get; set; }
}