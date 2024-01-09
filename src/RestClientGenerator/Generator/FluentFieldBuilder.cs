namespace RestClient.Generator;

using System;
using System.Collections.Generic;

/// <summary>
/// A fluent builder for a field.
/// </summary>
internal class FluentFieldBuilder
{
    /// <summary>
    /// The fields accessability.
    /// </summary>
    private string accessibility = "private";

    /// <summary>
    /// The type name.
    /// </summary>
    private string typeName;

    /// <summary>
    /// The parameter name.
    /// </summary>
    private string fieldName;

    /// <summary>
    /// The initial value.
    /// </summary>
    private string initialValue;

    /// <summary>
    /// A value indicating whether or not this is static.
    /// </summary>
    private bool isStatic;

    /// <summary>
    /// A value indicating whether or not this is read only.
    /// </summary>
    private bool isReadonly;

    /// <summary>
    /// A list of parameter attributes.
    /// </summary>
    private List<FluentAttributeBuilder> attributes;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentFieldBuilder"/> class.
    /// </summary>
    public FluentFieldBuilder()
    {
    }

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentFieldBuilder"/> class.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="fieldName">The field name.</param>
    public FluentFieldBuilder(string typeName, string fieldName)
    {
        this.typeName = typeName;
        this.fieldName = fieldName;
    }

    /// <summary>
    /// Sets the type name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder TypeName(string typeName)
    {
        this.typeName = typeName;
        return this;
    }

    public FluentFieldBuilder Type<T>()
    {
        this.typeName = typeof(T).FullName;
        return this;
    }

    /// <summary>
    /// Sets the parameter name.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentFieldBuilder Name(string parameterName)
    {
        this.fieldName = parameterName;
        return this;
    }

    /// <summary>
    /// Sets the property accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder Public()
    {
        this.accessibility = "public";
        return this;
    }

    /// <summary>
    /// Sets the fields accessability to protected.
    /// </summary>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder Protected()
    {
        this.accessibility = "protected";
        return this;
    }

    /// <summary>
    /// Sets the fields accessability to internal.
    /// </summary>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder Internal()
    {
        this.accessibility = "internal";
        return this;
    }

    /// <summary>
    /// Sets the fields accessability to private.
    /// </summary>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder Private()
    {
        this.accessibility = "private";
        return this;
    }

    /// <summary>
    /// Sets the fields initial value.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder InitialValue(string initialValue)
    {
        this.initialValue = initialValue;
        return this;
    }

    /// <summary>
    /// Sets the fields as static.
    /// </summary>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder Static()
    {
        this.isStatic = true;
        return this;
    }

    /// <summary>
    /// Sets the fields as read only.
    /// </summary>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder ReadOnly()
    {
        this.isReadonly = true;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the parameter.
    /// </summary>
    /// <param name="builder">An attribute builder action.</param>
    /// <returns>The <see cref="FluentFieldBuilder"/> instance.</returns>
    public FluentFieldBuilder Attribute(Action<FluentAttributeBuilder> builder)
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
        return Build(0);
    }

    internal string Build(int indent)
    {
        var indentStr = new string(' ', indent);
        var definition = $"{this.typeName} {this.fieldName}";

        if (this.isStatic)
        {
            definition = $"static {definition}";
        }

        if (this.isReadonly)
        {
            definition = $"readonly {definition}";
        }

        if (this.initialValue != null)
        {
            definition += $" = {this.initialValue}";
        }

        definition = $"{indentStr}{this.accessibility} {definition};";

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