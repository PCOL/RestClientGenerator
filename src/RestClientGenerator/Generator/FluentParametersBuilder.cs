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
        Action<FluentParameterBuilder> action = null)
    {
        var paramBuilder = methodBuilder.Param(parameterName);
        action(paramBuilder);
        return this;
    }

    /// <summary>
    /// Adds a parameter.
    /// </summary>
    /// <typeparam name="T">The parameters type.</typeparam>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParametersBuilder Param<T>(
        string parameterName,
        Action<FluentParameterBuilder> action = null)
    {
        return this.Param(parameterName, typeof(T).FullName, action);
    }

    /// <summary>
    /// Adds a parameter.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParametersBuilder Param(
        string parameterName,
        string typeName,
        Action<FluentParameterBuilder> action = null)
    {
        var paramBuilder = methodBuilder.Param(parameterName).TypeName(typeName);
        action?.Invoke(paramBuilder);
        return this;
    }

}