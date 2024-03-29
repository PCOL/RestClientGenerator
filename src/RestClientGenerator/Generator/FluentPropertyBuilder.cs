﻿namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A fluent property builder.
/// </summary>
internal class FluentPropertyBuilder
{
    /// <summary>
    /// The property name.
    /// </summary>
    private readonly string propertyName;

    /// <summary>
    /// The properties accessability.
    /// </summary>
    private string accessibility;

    /// <summary>
    /// The properties return type.
    /// </summary>
    private string returnType = "void";

    /// <summary>
    /// A value indicating whether the property is auto-implemented.
    /// </summary>
    private bool isAuto;

    /// <summary>
    /// A value indicating whether the property has a getter.
    /// </summary>
    private bool hasGetter;

    /// <summary>
    /// The getters accessability.
    /// </summary>
    private Accessability? getterAccessability;

    /// <summary>
    /// A value indicating whether the property has a setter.
    /// </summary>
    private bool hasSetter;

    /// <summary>
    /// The setters accessability.
    /// </summary>
    private Accessability? setterAccessability;

    /// <summary>
    /// The get body of the property.
    /// </summary>
    private FluentCodeBuilder getBody;

    /// <summary>
    /// The set body of the property.
    /// </summary>
    private FluentCodeBuilder setBody;

    /// <summary>
    /// A list of parameters.
    /// </summary>
    private List<FluentParameterBuilder> parameters;

    /// <summary>
    /// A list of method attributes.
    /// </summary>
    private List<FluentAttributeBuilder> attributes;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentMethodBuilder"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    public FluentPropertyBuilder(
        string propertyName)
    {
        this.propertyName = propertyName;
    }

    /// <summary>
    /// Sets the property accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Public()
    {
        this.accessibility = "public";
        return this;
    }

    /// <summary>
    /// Sets the property accessability to protected.
    /// </summary>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Protected()
    {
        this.accessibility = "protected";
        return this;
    }

    /// <summary>
    /// Sets the property accessability to internal.
    /// </summary>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Internal()
    {
        this.accessibility = "internal";
        return this;
    }

    /// <summary>
    /// Sets the property accessability to private.
    /// </summary>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Private()
    {
        this.accessibility = "private";
        return this;
    }

    /// <summary>
    /// Sets the property to auto-implemented.
    /// </summary>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Auto()
    {
        this.isAuto = true;
        return this;
    }

    /// <summary>
    /// Sets the return type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Returns(string typeName)
    {
        this.returnType = typeName;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the property.
    /// </summary>
    /// <param name="builder">An attribute builder action.</param>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Attribute(Action<FluentAttributeBuilder> builder)
    {
        var attributeBuilder = new FluentAttributeBuilder();
        builder(attributeBuilder);
        this.attributes ??= new List<FluentAttributeBuilder>();
        this.attributes.Add(attributeBuilder);
        return this;
    }

    /// <summary>
    /// Sets the get body.
    /// </summary>
    /// <param name="action">The get body.</param>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Getter(Action<FluentCodeBuilder> action, Accessability? accessability = null)
    {
        this.hasGetter = true;
        this.getterAccessability = accessability;
        if (action != null)
        {
            this.getBody = new FluentCodeBuilder();
            action(this.getBody);
        }

        return this;
    }

    /// <summary>
    /// Sets the get body.
    /// </summary>
    /// <param name="action">The get body.</param>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Setter(Action<FluentCodeBuilder> action, Accessability? accessability = null)
    {
        this.hasSetter = true;
        this.setterAccessability = accessability;
        if (action != null)
        {
            this.setBody = new FluentCodeBuilder();
            action(this.setBody);
        }

        return this;
    }

    /// <summary>
    /// Builds the property definition.
    /// </summary>
    /// <returns>The property definition.</returns>
    public string Build()
    {
        return Build(0);
    }

    /// <summary>
    /// Builds the property definition.
    /// </summary>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The property definition.</returns>
    internal string Build(int indent)
    {
        var indentStr = new string(' ', indent);
        var indentTabStr = new string(' ', 4);

        var parms = string.Empty;
        if (this.parameters != null)
        {
            foreach (var parm in this.parameters)
            {
                if (parms.Length > 0)
                {
                    parms += ", ";
                }

                parms += parm.Build();
            }
        }

        var propertyDefinition = new StringBuilder();
        if (this.attributes != null)
        {
            foreach (var attribute in this.attributes)
            {
                propertyDefinition
                    .Append(indentStr)
                    .AppendLine(attribute.Build());
            }
        }

        if (this.isAuto)
        {
            var getterAccess = this.getterAccessability.HasValue ? $"{this.getterAccessability.Value.ToString().ToLower()} " : string.Empty;
            var getter = this.hasGetter ? $"{getterAccess}get; " : string.Empty;

            var setterAccess = this.setterAccessability.HasValue ? $"{this.setterAccessability.Value.ToString().ToLower()} " : string.Empty;
            var setter = this.hasSetter ? $"{setterAccess}set; " : string.Empty;

            propertyDefinition
                .Append(indentStr)
                .AppendLine($"{this.accessibility} {this.returnType} {this.propertyName} {{ {getter}{setter}}}");
        }
        else
        {
            propertyDefinition
                .Append(indentStr)
                .AppendLine($"{this.accessibility} {this.returnType} {this.propertyName}")
                .Append(indentStr)
                .AppendLine("{");

            if (this.hasGetter)
            {
                propertyDefinition
                    .Append(indentStr)
                    .Append(indentTabStr)
                    .AppendLine("get")
                    .Append(indentStr)
                    .Append(indentTabStr)
                    .AppendLine("{")
                    .Append(this.getBody.Build(indent + 8))
                    .Append(indentStr)
                    .Append(indentTabStr)
                    .AppendLine("}");
            }

            if (this.hasSetter)
            {
                if (this.hasGetter)
                {
                    propertyDefinition
                        .AppendLine();
                }

                propertyDefinition
                    .Append(indentStr)
                    .Append(indentTabStr)
                    .AppendLine("set")
                    .Append(indentStr)
                    .Append(indentTabStr)
                    .AppendLine("{")
                    .Append(this.setBody.Build(indent + 8))
                    .Append(indentStr)
                    .Append(indentTabStr)
                    .AppendLine("}");
            }

            propertyDefinition
                .Append(indentStr)
                .AppendLine("}");
        }

        return propertyDefinition.ToString();
    }
}
