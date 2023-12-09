namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// A fluent method builder.
/// </summary>
public class FluentMethodBuilder
{
    /// <summary>
    /// The method name.
    /// </summary>
    private readonly string methodName;

    /// <summary>
    /// The methods accessability.
    /// </summary>
    private string accessibility;

    /// <summary>
    /// The methods return type.
    /// </summary>
    private string returnType = "void";

    /// <summary>
    /// A value indicating whether or not the method is async.
    /// </summary>
    private bool async;

    /// <summary>
    /// The body of the method.
    /// </summary>
    private StringBuilder body;

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
    /// <param name="methodName">The name of the method.</param>
    public FluentMethodBuilder(
        string methodName)
    {
        this.methodName = methodName;
    }

    /// <summary>
    /// Sets the methods accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Public()
    {
        this.accessibility = "public";
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Async()
    {
        this.async = true;
        return this;
    }

    /// <summary>
    /// Sets the return type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Returns(string typeName)
    {
        this.returnType = typeName;
        return this;
    }

    /// <summary>
    /// Adds a parameter to the method.
    /// </summary>
    /// <param name="typeName">The parameter type name.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="builder">The parameter builder action.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Param(
        string typeName,
        string parameterName,
        Action<FluentParameterBuilder> builder = null)
    {
        var parameterBuilder = new FluentParameterBuilder(typeName, parameterName);
        builder?.Invoke(parameterBuilder);
        this.parameters ??= new List<FluentParameterBuilder>();
        this.parameters.Add(parameterBuilder);
        return this;
    }

    /// <summary>
    /// Adds a parameter to the method.
    /// </summary>
    /// <param name="builder">The parameter builder action.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Param(Action<FluentParameterBuilder> builder)
    {
        var parameterBuilder = new FluentParameterBuilder();
        builder(parameterBuilder);
        this.parameters ??= new List<FluentParameterBuilder>();
        this.parameters.Add(parameterBuilder);
        return this;
    }

    /// <summary>
    /// Adds a parameter to the method.
    /// </summary>
    /// <param name="builder">The parameter builder action.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentParameterBuilder Param(string parameterName)
    {
        var parameterBuilder = new FluentParameterBuilder()
            .Name(parameterName);

        this.parameters ??= new List<FluentParameterBuilder>();
        this.parameters.Add(parameterBuilder);
        return parameterBuilder;
    }

    /// <summary>
    /// Adds a parameter to the method.
    /// </summary>
    /// <param name="builder">The parameter builder action.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Params(
        Action<FluentParametersBuilder> builder)
    {
        var parametersBuilder = new FluentParametersBuilder(this);
        builder(parametersBuilder);
        return this;
    }


    /// <summary>
    /// Adds an attribute to the method.
    /// </summary>
    /// <param name="builder">An attribute builder action.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Attribute(Action<FluentAttributeBuilder> builder)
    {
        var attributeBuilder = new FluentAttributeBuilder();
        builder(attributeBuilder);
        this.attributes ??= new List<FluentAttributeBuilder>();
        this.attributes.Add(attributeBuilder);
        return this;
    }

    /// <summary>
    /// Sets the method body.
    /// </summary>
    /// <param name="action">The method body.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentMethodBuilder Body(Action<StringBuilder> action)
    {
        this.body = new StringBuilder();
        action(this.body);
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

        var methodDefinition = new StringBuilder();
        if (this.attributes != null)
        {
            foreach (var attribute in this.attributes)
            {
                methodDefinition
                    .Append(indentStr)
                    .AppendLine(attribute.Build());
            }
        }

        var asyncValue = this.async ? "async " : string.Empty;
        methodDefinition
            .Append(indentStr)
            .AppendLine($"{this.accessibility} {asyncValue}{this.returnType} {this.methodName}({parms})")
            .Append(indentStr)
            .AppendLine("{")
            .Append(indentStr)
            .AppendLine(this.body.ToString())
            .Append(indentStr)
            .AppendLine("}");

        return methodDefinition.ToString();
    }
}
