﻿namespace RestClient;

using System;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// An abstract attribute class used for creating attributes that convert a <see cref="HttpResponseMessage"/>
/// into a type.
/// </summary>
public class FromResponseAttribute
    : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FromResponseAttribute"/> class.
    /// </summary>
    public FromResponseAttribute()
    {
    }

    /// <summary>
    /// Gets or sets the content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the return type.
    /// </summary>
    public Type ReturnType { get; set; }

    /// <summary>
    /// Converts the given <see cref="HttpResponseMessage"/> into the required type.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
    /// <param name="dataType">The required type.</param>
    /// <param name="serializer">A <see cref="IObjectSerializer"/> for the responses contnet type.</param>
    /// <returns>An instance of the type; otherwise null.</returns>
    public virtual object ToObject(
        HttpResponseMessage response,
        Type dataType,
        IObjectSerializer serializer)
    {
        return this.ToObjectAsync(response, dataType, serializer).Result;
    }

    /// <summary>
    /// Converts the given <see cref="HttpResponseMessage"/> into the required type.
    /// </summary>
    /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
    /// <param name="dataType">The required type.</param>
    /// <param name="serializer">A <see cref="IObjectSerializer"/> for the responses contnet type.</param>
    /// <returns>An instance of the type; otherwise null.</returns>
    public virtual async Task<object> ToObjectAsync(
        HttpResponseMessage response,
        Type dataType,
        IObjectSerializer serializer)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (content.IsNullOrEmpty() == true)
        {
            return null;
        }

        Type objectType = this.ReturnType ?? dataType;
        object model = serializer.DeserializeObject(content, objectType);
        if (model == null)
        {
            return null;
        }

        return model;
    }

}