namespace TestClient;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ErrorModel))]
[JsonSerializable(typeof(IErrorModel))]
[JsonSerializable(typeof(ServiceResult))]
[JsonSerializable(typeof(IServiceResult))]
[JsonSerializable(typeof(ServiceResult<string>))]
[JsonSerializable(typeof(IServiceResult<string>))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(CreateWorkloadModel))]
public partial class JsonContext
    : JsonSerializerContext
{

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(
            json,
            this.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>);
    }

    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize<T>(
            obj,
            this.GetTypeInfo(typeof(T)) as JsonTypeInfo<T>);
    }
}