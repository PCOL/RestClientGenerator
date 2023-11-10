namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Text;

public class FluentClassBuilder
{
    /// <summary>
    /// The class name.
    /// </summary>
    private readonly string className;

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
    /// A list of methods.
    /// </summary>
    private List<FluentMethodBuilder> methods;

    /// <summary>
    /// A list of class attributes.
    /// </summary>
    private List<FluentAttributeBuilder> attributes;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentMethodBuilder"/> class.
    /// </summary>
    /// <param name="className">The name of the class.</param>
    public FluentClassBuilder(string className)
    {
        className = className ?? throw new ArgumentNullException(nameof(className));

        this.className = className;
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
    /// Builds the method definition.
    /// </summary>
    /// <returns>The method definition.</returns>
    public string Build()
    {
        var classDefinition = new StringBuilder();


        foreach (var attribute in this.attributes)
        {
            classDefinition.AppendLine(attribute.Build());
        }

        var value = this.isSealed ? "sealed " : this.isAbstract ? "abstract " : string.Empty;
        classDefinition
            .AppendLine($"{this.accessibility} {value}class {this.className}")
            .AppendLine("{");

        var parms = string.Empty;
        foreach (var method in this.methods)
        {
            classDefinition
                .AppendLine(method.Build(4))
                .AppendLine();
        }

        classDefinition
            .AppendLine("}");
        
        return classDefinition.ToString();
    }
}
