﻿namespace RestClient;

using System;

/// <summary>
/// An attribute used to specifiy that an out parameter should use a Model Binder to
/// return a specific type.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
public class BindToModelAttribute
    : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BindToModelAttribute"/> class.
    /// </summary>
    /// <param name="modelType">The model type.</param>
    public BindToModelAttribute(Type modelType)
    {
        this.ModelType = modelType;
    }

    /// <summary>
    /// Gets the model type.
    /// </summary>
    public Type ModelType { get; }
}