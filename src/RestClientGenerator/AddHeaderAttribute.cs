﻿namespace RestClient;

using System;

/// <summary>
/// An attribute used to specify that a header should be added to the request.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
public class AddHeaderAttribute
    : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddHeaderAttribute"/> class.
    /// </summary>
    /// <param name="header">The header name.</param>
    /// <param name="value">The header value.</param>
    public AddHeaderAttribute(string header, string value)
    {
        this.Header = header;
        this.Value = value;
    }

    /// <summary>
    /// Gets the header name.
    /// </summary>
    public string Header { get; }

    /// <summary>
    /// Gets the header value.
    /// </summary>
    public string Value { get; }
}