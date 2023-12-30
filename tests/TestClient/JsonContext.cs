namespace TestClient;

using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ErrorModel))]
[JsonSerializable(typeof(ServiceResult))]
[JsonSerializable(typeof(IServiceResult))]
[JsonSerializable(typeof(ServiceResult<string>))]
[JsonSerializable(typeof(IServiceResult<string>))]
public partial class JsonContext
    : JsonSerializerContext
{
}