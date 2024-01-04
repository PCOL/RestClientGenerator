namespace TestClient;

using System.Text.Json;
using System.Text.Json.Serialization;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(ErrorModel))]
[JsonSerializable(typeof(IErrorModel))]
[JsonSerializable(typeof(ServiceResult))]
[JsonSerializable(typeof(IServiceResult))]
[JsonSerializable(typeof(ServiceResult<string>))]
[JsonSerializable(typeof(IServiceResult<string>))]
[JsonSerializable(typeof(JsonObject))]
[JsonSerializable(typeof(CreateWorkloadModel))]
[JsonSerializable(typeof(WorkloadModel))]
public partial class JsonContext
    : JsonSerializerContext
{
    /// <summary>
    /// Deserialize a JSON object.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="json">The JSON to deserialize.</param>
    /// <returns>An object representation of the JSON.</returns>
    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(
            json,
            new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            });
    }

    /// <summary>
    /// Serializes a object to JSON.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>JSON representation of the object.</returns>
    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize<T>(obj);
    }
}