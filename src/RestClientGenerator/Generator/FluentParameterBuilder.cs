namespace RestClient.Generator;

using System;
using System.Collections.Generic;

/// <summary>
/// A fluent parameter builder.
/// </summary>
public class FluentParameterBuilder
{
    /// <summary>
    /// The type name.
    /// </summary>
    private string typeName;

    /// <summary>
    /// The parameter name.
    /// </summary>
    private string parameterName;

    /// <summary>
    /// The default value.
    /// </summary>
    private string @default;

    /// <summary>
    /// A value indicating whether or not the parameter is a params type.
    /// </summary>
    private bool @params;

    /// <summary>
    /// A list of parameter attributes.
    /// </summary>
    private List<FluentAttributeBuilder> attributes;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentParameterBuilder"/> class.
    /// </summary>
    public FluentParameterBuilder()
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentParameterBuilder"/> class.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="typeName">The type name.</param>
    public FluentParameterBuilder(string parameterName, string typeName)
    {
        this.typeName = typeName;
        this.parameterName = parameterName;
    }

    /// <summary>
    /// Sets the type name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParameterBuilder TypeName(string typeName)
    {
        this.typeName = typeName;
        return this;
    }

    public FluentParameterBuilder Type<T>()
    {
        this.typeName = typeof(T).FullName;
        return this;
    }

    /// <summary>
    /// Sets the parameter name.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParameterBuilder Name(string parameterName)
    {
        this.parameterName = parameterName;
        return this;
    }

    /// <summary>
    /// Sets the paremeter as optional.
    /// </summary>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParameterBuilder Optional()
    {
        this.@default = "default";
        return this;
    }

    /// <summary>
    /// Sets the parameters defaulf value.
    /// </summary>
    /// <param name="default">The default value.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParameterBuilder Default(string @default)
    {
        this.@default = @default;
        return this;
    }

    /// <summary>
    /// Sets the parameter as a "params" parameter. 
    /// </summary>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParameterBuilder Params()
    {
        @params = true;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the parameter.
    /// </summary>
    /// <param name="builder">An attribute builder action.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentParameterBuilder Attribute(Action<FluentAttributeBuilder> builder)
    {
        var attributeBuilder = new FluentAttributeBuilder();
        builder(attributeBuilder);
        this.attributes ??= new List<FluentAttributeBuilder>();
        this.attributes.Add(attributeBuilder);
        return this;
    }

    /// <summary>
    /// Builds the parameter definition.
    /// </summary>
    /// <returns>The built parameter definition.</returns>
    public string Build()
    {
        var definition = $"{this.typeName} {this.parameterName}";

        if (this.@params)
        {
            definition = $"params {definition}";
        }
        else
        {
            if (this.@default != null)
            {
                definition += $" = {this.@default}";
            }
        }

        if (this.attributes != null)
        {
            var attrs = string.Empty;
            foreach (var attribute in this.attributes)
            {
                attrs += attribute.Build();
            }

            definition = attrs + definition;
        }

        return definition;
    }
}