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
    private string accessibility = "private";

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
    private FluentCodeBuilder body;

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
    /// Sets the methods accessability to private.
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Private()
    {
        this.accessibility = "private";
        return this;
    }

    /// <summary>
    /// Sets the methods accessability to internal.
    /// </summary>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Internal()
    {
        this.accessibility = "internal";
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
    /// <typeparam name="T">The return type.</typeparam>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Returns<T>()
    {
        return Returns(typeof(T).FullName);
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

    public FluentMethodBuilder Param<T>(
        string parameterName,
        Action<FluentParameterBuilder> builder = null)
    {
        return this.Param(parameterName, typeof(T).FullName);
    }

    /// <summary>
    /// Adds a parameter to the method.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <param name="typeName">The parameter type name.</param>
    /// <param name="builder">The parameter builder action.</param>
    /// <returns>The <see cref="FluentMethodBuilder"/> instance.</returns>
    public FluentMethodBuilder Param(
        string parameterName,
        string typeName,
        Action<FluentParameterBuilder> builder = null)
    {
        var parameterBuilder = new FluentParameterBuilder(parameterName, typeName);
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
    public FluentMethodBuilder Body(Action<FluentCodeBuilder> action)
    {
        var code = new FluentCodeBuilder();
        action(code);
        return this.Body(code);
    }

    /// <summary>
    /// Sets the method body.
    /// </summary>
    /// <param name="codeBuilder">The method body.</param>
    /// <returns>The <see cref="FluentParameterBuilder"/> instance.</returns>
    public FluentMethodBuilder Body(FluentCodeBuilder codeBuilder)
    {
        this.body = codeBuilder;
        return this;
    }

    /// <summary>
    /// Builds the method definition.
    /// </summary>
    /// <returns>The method definition.</returns>
    public string Build(bool isConstructor = false)
    {
        return Build(0, isConstructor);
    }

    internal string Build(int indent, bool isConstructor = false)
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

        if (isConstructor)
        {
            methodDefinition
                .Append(indentStr)
                .AppendLine($"{this.accessibility} {this.methodName}({parms})")
                .Append(indentStr)
                .AppendLine("{");
        }
        else
        {
            var asyncValue = this.async ? "async " : string.Empty;
            methodDefinition
                .Append(indentStr)
                .AppendLine($"{this.accessibility} {asyncValue}{this.returnType} {this.methodName}({parms})")
                .Append(indentStr)
                .AppendLine("{");
        }

        if (this.body != null)
        {
            methodDefinition
                .Append(this.body.Build(indent + 4));
        }

        methodDefinition
            .Append(indentStr)
            .AppendLine("}");

        return methodDefinition.ToString();
    }
}
