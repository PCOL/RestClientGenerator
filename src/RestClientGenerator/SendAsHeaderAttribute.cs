﻿namespace RestClient;

using System;

/// <summary>
/// An attribute used to send a parameter as a request header.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class SendAsHeaderAttribute
    : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SendAsHeaderAttribute"/> class.
    /// </summary>
    /// <param name="headerName">The header name.</param>
    public SendAsHeaderAttribute(string headerName)
    {
        this.Name = headerName;
    }

    /// <summary>
    /// Gets the header name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the header format.
    /// </summary>
    public string Format { get; set; }
}