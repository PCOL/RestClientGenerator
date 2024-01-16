﻿namespace RestClient;

using System;
using System.Net;

/// <summary>
/// An attribute used to indicate that a method or all methods on an interface should
/// attempt to retry upon failure.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
public class RetryAttribute
    : Attribute
{
    /// <summary>
    /// Gets or sets the http status code that will cause an operation to retry.
    /// </summary>
    public HttpStatusCode[] HttpStatusCodesToRetry { get; set; }

    /// <summary>
    /// Gets or sets the exceptions that will cause an operation to retry.
    /// </summary>
    public Type[] ExceptionTypesToRetry { get; set; }

    /// <summary>
    /// Gets or sets the number of retries.
    /// </summary>
    public int RetryLimit { get; set; }

    /// <summary>
    /// Gets or sets the number of milliseconds to wait between retries.
    /// </summary>
    public int WaitTime { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of milliseconds to wait between retries.
    /// </summary>
    public int MaxWaitTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not to double the wait time upon each retry.
    /// </summary>
    public bool DoubleWaitTimeOnRetry { get; set; }
}