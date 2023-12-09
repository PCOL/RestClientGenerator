namespace RestClient.Generator;

using System;
using System.Collections.Generic;

/// <summary>
/// A fluent parameter builder.
/// </summary>
public class FluentParametersBuilder
{
    private readonly FluentMethodBuilder methodBuilder;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentParameterBuilder"/> class.
    /// </summary>
    public FluentParametersBuilder(FluentMethodBuilder methodBuilder)
    {
        this.methodBuilder = methodBuilder;
    }

    /// <summary>
    /// Adds a parameter.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParametersBuilder Param(
        string parameterName,
        Action<FluentParameterBuilder> builder)
    {
        var paramBuilder = methodBuilder.Param(parameterName);
        builder(paramBuilder);
        return this;
    }
}