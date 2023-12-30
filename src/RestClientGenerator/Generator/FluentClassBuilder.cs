﻿namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a fluent class builder.
/// </summary>
public class FluentClassBuilder
{
    /// <summary>
    /// The class name.
    /// </summary>
    private readonly string className;

    /// <summary>
    /// The base class name.
    /// </summary>
    private string baseClassName;

    /// <summary>
    /// The namespace.
    /// </summary>
    private string @namespace;

    /// <summary>
    /// The methods accessability.
    /// </summary>
    private string accessibility;

    /// <summary>
    /// A value indicating whether or not the class is sealed.
    /// </summary>
    private bool isSealed;

    /// <summary>
    /// A value indicating whether or not the class is abstract.
    /// </summary>
    private bool isAbstract;

    /// <summary>
    /// A value indicating whether or not the class is partial.
    /// </summary>
    private bool isPartial;

    /// <summary>
    /// A list of interfaces.
    /// </summary>
    private List<string> interfaces;

    /// <summary>
    /// A list of fields.
    /// </summary>
    private List<FluentFieldBuilder> fields;

    /// <summary>
    /// A list of constructorss.
    /// </summary>
    private List<FluentMethodBuilder> constructors;

    /// <summary>
    /// A list of methods.
    /// </summary>
    private List<FluentMethodBuilder> methods;

    /// <summary>
    /// A list of properties.
    /// </summary>
    private List<FluentPropertyBuilder> properties;

    /// <summary>
    /// A list of class attributes.
    /// </summary>
    private List<FluentAttributeBuilder> attributes;

    /// <summary>
    /// A list of subclasses.
    /// </summary>
    private List<FluentClassBuilder> subClasses;

    /// <summary>
    /// A list of usings.
    /// </summary>
    private List<(string, bool)> usings;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentMethodBuilder"/> class.
    /// </summary>
    /// <param name="className">The name of the class.</param>
    public FluentClassBuilder(string className)
    {
        className = className ?? throw new ArgumentNullException(nameof(className));

        var pos = className.IndexOf('.');
        if (pos == -1)
        {
            this.className = className;
        }
        else
        {
            this.@namespace = className.Substring(0, pos);
            this.className = className.Substring(pos + 1);
        }
    }

    /// <summary>
    /// Sets the methods accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentClassBuilder Public()
    {
        this.accessibility = "public";
        return this;
    }

    /// <summary>
    /// Sets the methods accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentClassBuilder Private()
    {
        this.accessibility = "private";
        return this;
    }

    /// <summary>
    /// Sets the methods accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentClassBuilder Internal()
    {
        this.accessibility = "internal";
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentClassBuilder Abstract()
    {
        this.isAbstract = true;
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentClassBuilder Partial()
    {
        this.isPartial = true;
        return this;
    }

    public FluentClassBuilder Namespace(string @namespace)
    {
        this.@namespace = @namespace;
        return this;
    }

    public FluentClassBuilder Using(string @using, bool @static = false)
    {
        this.usings ??= new List<(string, bool)>();
        this.usings.Add((@using, @static));
        return this;
    }

    /// <summary>
    /// Specifies that an interface is implemented.
    /// </summary>
    /// <param name="interface">The interface name.</param>
    /// <returns>The <see cref="FluentClassBuilder"/>.</returns>
    public FluentClassBuilder Implements(string @interface)
    {
        this.interfaces ??= new List<string>();
        this.interfaces.Add(@interface);
        return this;
    }

    /// <summary>
    /// Specifies that the class ihnerits from another class.
    /// </summary>
    /// <param name="baseClassName">The base classes name.</param>
    /// <returns>The <see cref="FluentClassBuilder"/>.</returns>
    public FluentClassBuilder Inherits(string baseClassName)
    {
        this.baseClassName = baseClassName;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the class.
    /// </summary>
    /// <param name="builder">An attribute builder action.</param>
    /// <returns>The <see cref="FluentClassBuilder"/> instance.</returns>
    public FluentClassBuilder Attribute(Action<FluentAttributeBuilder> builder)
    {
        var attributeBuilder = new FluentAttributeBuilder();
        builder(attributeBuilder);
        this.attributes ??= new List<FluentAttributeBuilder>();
        this.attributes.Add(attributeBuilder);
        return this;
    }

    public FluentClassBuilder Field<T>(
        string fieldName,
        Action<FluentFieldBuilder> action = null)
    {
        return this.Field(typeof(T).FullName, fieldName, action);
    }

    public FluentClassBuilder Field(
        string typeName,
        string fieldName,
        Action<FluentFieldBuilder> action = null)
    {
        var builder = new FluentFieldBuilder(typeName, fieldName);
        action?.Invoke(builder);

        this.fields ??= new List<FluentFieldBuilder>();
        this.fields.Add(builder);
        return this;
    }

    public FluentClassBuilder Constructor(
        Action<FluentMethodBuilder> action)
    {
        var builder = new FluentMethodBuilder(this.className);
        action(builder);

        this.constructors ??= new List<FluentMethodBuilder>();
        this.constructors.Add(builder);
        return this;
    }

    public FluentClassBuilder Method(
        string methodName,
        Action<FluentMethodBuilder> action)
    {
        var methodBuilder = new FluentMethodBuilder(methodName);
        action(methodBuilder);
        this.methods ??= new List<FluentMethodBuilder>();
        this.methods.Add(methodBuilder);
        return this;
    }

    /// <summary>
    /// Adds a method.
    /// </summary>
    /// <param name="methodName">The name of the method.</param>
    /// <returns>A <see cref="FluentMethodBuilder"/>.</returns>
    public FluentMethodBuilder Method(string methodName)
    {
        var methodBuilder = new FluentMethodBuilder(methodName);
        this.methods ??= new List<FluentMethodBuilder>();
        this.methods.Add(methodBuilder);
        return methodBuilder;
    }


    public FluentClassBuilder Property(
        string propertyName,
        Action<FluentPropertyBuilder> action)
    {
        var propertyBuilder = new FluentPropertyBuilder(propertyName);
        action(propertyBuilder);
        this.properties ??= new List<FluentPropertyBuilder>();
        this.properties.Add(propertyBuilder);
        return this;
    }

    public FluentClassBuilder SubClass(
        string className,
        Action<FluentClassBuilder> action)
    {
        var classBuilder = new FluentClassBuilder(className);
        action(classBuilder);
        this.subClasses ??= new List<FluentClassBuilder>();
        this.subClasses.Add(classBuilder);
        return this;
    }

    /// <summary>
    /// Builds the method definition.
    /// </summary>
    /// <returns>The method definition.</returns>
    public string Build()
    {
        return this.Build(0);
    }

    private string Build(int indent)
    {
        var indentStr = new string(' ', indent);

        var classDefinition = new StringBuilder();
        if (this.@namespace != null)
        {
            classDefinition
                .Append(indentStr)
                .AppendLine($"namespace {this.@namespace};")
                .AppendLine();
        }

        if (this.usings != null)
        {
            foreach (var (@using, @static) in this.usings.OrderBy(u => u.Item2))
            {
                var staticStr = @static ? "static " : string.Empty;
                classDefinition
                    .Append(indentStr)
                    .AppendLine($"using {staticStr}global::{@using.Trim()};");
            }

            classDefinition.AppendLine();
        }

        if (this.attributes != null)
        {
            foreach (var attribute in this.attributes)
            {
                classDefinition
                    .AppendLine(attribute.Build());
            }
        }

        var value = this.isSealed ? "sealed " : this.isAbstract ? "abstract " : this.isPartial ? "partial " : string.Empty;
        classDefinition
            .Append(indentStr)
            .AppendLine($"{this.accessibility} {value}class {this.className}");

        if (this.interfaces != null ||
            string.IsNullOrWhiteSpace(this.baseClassName) == false)
        {
            classDefinition
                .Append(indentStr)
                .Append("   : ");

            if (string.IsNullOrWhiteSpace(this.baseClassName) == false)
            {
                classDefinition
                    .Append(this.baseClassName)
                    .AppendLine();
            }

            classDefinition
                .Append(string.Join($",\r\n{indentStr}", this.interfaces))
                .AppendLine();
        }

        classDefinition
            .Append(indentStr)
            .AppendLine("{");

        if (this.fields != null)
        {
            classDefinition.AppendLine();

            foreach (var field in this.fields)
            {
                classDefinition
                    .AppendLine(field.Build(indent + 4));
            }
        }

        if (this.constructors != null)
        {
            classDefinition.AppendLine();

            foreach (var ctor in this.constructors)
            {
                classDefinition
                    .AppendLine(ctor.Build(indent + 4, true));
            }
        }

        if (this.properties != null)
        {
            foreach (var property in this.properties)
            {
                classDefinition
                    .AppendLine(property.Build(indent + 4));
            }
        }

        if (this.methods != null)
        {
            classDefinition.AppendLine();

            foreach (var method in this.methods)
            {
                classDefinition
                    .AppendLine(method.Build(indent + 4));
            }
        }

        if (this.subClasses != null)
        {
            classDefinition.AppendLine();

            foreach (var subClass in this.subClasses)
            {
                classDefinition
                    .AppendLine(subClass.Build(indent + 4));
            }
        }

        classDefinition
            .Append(indentStr)
            .AppendLine("}");
        
        return classDefinition.ToString();
    }
}
