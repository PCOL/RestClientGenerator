namespace TestClient;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

public class JsonObject
    : IJsonOnDeserialized
{
    /// <summary>
    /// Gets or sets the configurartion.
    /// </summary>
    [JsonExtensionData]
    public IDictionary<string, JsonElement> Properties { get; set; }

    /// <inheritdoc />
    public void OnDeserialized()
    {
        if (this.Properties != null)
        {
            this.Properties = new Dictionary<string, JsonElement>(this.Properties, StringComparer.OrdinalIgnoreCase);
        }
    }
}