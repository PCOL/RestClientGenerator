﻿namespace RestClient;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Allows an out parameter or return value to be set from a JSON value.
/// </summary>
[AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter)]
public class FromJsonAttribute
    : FromResponseAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FromJsonAttribute"/> class.
    /// </summary>
    /// <param name="jsonPath">The path to the json value.</param>
    public FromJsonAttribute(string jsonPath)
    {
        this.ContentType = "application/json";
        this.JsonPath = jsonPath;
    }

    /// <summary>
    /// Gets or sets the contentType.
    /// </summary>
    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public new string ContentType
    {
        get => base.ContentType;
        set { }
    }

    /// <summary>
    /// Gets the json path.
    /// </summary>
    public string JsonPath { get; }

    /// <inheritdoc/>
    public override async Task<object> ToObjectAsync(
        HttpResponseMessage response,
        Type dataType,
        IObjectSerializer serializer)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (content.IsNullOrEmpty() == true)
        {
            return null;
        }

        var obj = serializer.DeserializeObject(content, typeof(object));

        Type objectType = this.ReturnType ?? dataType;
        var properties = objectType.GetProperties();

        object returnObj = null;
        if (objectType.IsGenericType == false)
        {
            returnObj = serializer.GetObjectFromPath(obj, objectType, this.JsonPath);
        }
        else
        {
            if (objectType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                objectType.GetGenericTypeDefinition() == typeof(List<>) ||
                objectType.GetGenericTypeDefinition() == typeof(IList<>))
            {
                returnObj = serializer.GetObjectFromPath(obj, objectType, this.JsonPath);
            }
            else
            {
                if (dataType.IsAssignableFrom(objectType) == false)
                {
                    throw new InvalidCastException($"Cannot cast {objectType.Name} to {dataType.Name}");
                }

                var genArgs = objectType.GetGenericArguments();
                if (genArgs?.Length > 1)
                {
                    throw new NotSupportedException("Only one generic argument is supported");
                }

                var resultType = genArgs.First();

                returnObj = Activator.CreateInstance(this.ReturnType);
                properties.SetProperty(
                    returnObj,
                    resultType,
                    () => serializer.GetObjectFromPath(obj, resultType, this.JsonPath));
            }
        }

        properties.SetProperty<HttpResponseMessage>(
            returnObj,
            () => response);

        return returnObj;
    }
}