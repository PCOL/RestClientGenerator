namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Text;

public class FluentPropertyBuilder
{
    /// <summary>
    /// The property name.
    /// </summary>
    private readonly string propertyName;

    /// <summary>
    /// The methods accessability.
    /// </summary>
    private string accessibility;

    /// <summary>
    /// The methods return type.
    /// </summary>
    private string returnType = "void";

    /// <summary>
    /// 
    /// </summary>
    private bool isAuto;

    /// <summary>
    /// 
    /// </summary>
    private bool hasGetter;

    /// <summary>
    /// 
    /// </summary>
    private Accessability? getterAccessability;

    /// <summary>
    /// 
    /// </summary>
    private bool hasSetter;

    /// <summary>
    /// 
    /// </summary>
    private Accessability? setterAccessability;

    /// <summary>
    /// The get body of the property.
    /// </summary>
    private StringBuilder getBody;

    /// <summary>
    /// The set body of the property.
    /// </summary>
    private StringBuilder setBody;

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
    public FluentPropertyBuilder Getter(Action<StringBuilder> action, Accessability? accessability = null)
    {
        this.hasGetter = true;
        this.getterAccessability = accessability;
        if (action != null)
        {
            this.getBody = new StringBuilder();
            action(this.getBody);
        }

        return this;
    }

    /// <summary>
    /// Sets the get body.
    /// </summary>
    /// <param name="action">The get body.</param>
    /// <returns>The <see cref="FluentPropertyBuilder"/> instance.</returns>
    public FluentPropertyBuilder Setter(Action<StringBuilder> action, Accessability? accessability = null)
    {
        this.hasSetter = true;
        this.setterAccessability = accessability;
        if (action != null)
        {
            this.setBody = new StringBuilder();
            action(this.setBody);
        }

        return this;
    }

    /// <summary>
    /// Builds the method definition.
    /// </summary>
    /// <returns>The method definition.</returns>
    public string Build()
    {
        return Build(0);
    }

    internal string Build(int indent)
    {
        var indentStr = new string(' ', indent);

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
                    .Append(indentStr)
                    .AppendLine("get")
                    .Append(indentStr)
                    .Append(indentStr)
                    .AppendLine("{")
                    .Append(indentStr)
                    .Append(indentStr)
                    .Append(indentStr)
                    .Append(this.getBody.ToString())
                    .Append(indentStr)
                    .Append(indentStr)
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
                    .Append(indentStr)
                    .AppendLine("set")
                    .Append(indentStr)
                    .Append(indentStr)
                    .AppendLine("{")
                    .Append(indentStr)
                    .Append(indentStr)
                    .Append(indentStr)
                    .Append(this.setBody.ToString())
                    .Append(indentStr)
                    .Append(indentStr)
                    .AppendLine("}");
            }

            propertyDefinition
                .Append(indentStr)
                .AppendLine("}");
        }

        return propertyDefinition.ToString();
    }
}
