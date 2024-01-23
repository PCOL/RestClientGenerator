namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Represents a fluent struct builder.
/// </summary>
internal class FluentStructBuilder
{
    /// <summary>
    /// The struct name.
    /// </summary>
    private readonly string structName;

    /// <summary>
    /// The base name.
    /// </summary>
    private string baseName;

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
    private List<FluentStructBuilder> subClasses;

    /// <summary>
    /// A list of usings.
    /// </summary>
    private List<(string, bool)> usings;

    /// <summary>
    /// Initialises a new instance of the <see cref="FluentStructBuilder"/> class.
    /// </summary>
    /// <param name="structName">The name of the struct.</param>
    public FluentStructBuilder(string structName)
    {
        structName = structName ?? throw new ArgumentNullException(nameof(structName));

        var pos = structName.IndexOf('.');
        if (pos == -1)
        {
            this.structName = structName;
        }
        else
        {
            this.@namespace = structName.Substring(0, pos);
            this.structName = structName.Substring(pos + 1);
        }
    }

    /// <summary>
    /// Sets the classes accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Public()
    {
        this.accessibility = "public";
        return this;
    }

    /// <summary>
    /// Sets the classes accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Private()
    {
        this.accessibility = "private";
        return this;
    }

    /// <summary>
    /// Sets the classes accessability to public.
    /// </summary>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Internal()
    {
        this.accessibility = "internal";
        return this;
    }

    /// <summary>
    /// Sets the class as abstract.
    /// </summary>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Abstract()
    {
        this.isAbstract = true;
        return this;
    }

    /// <summary>
    /// Sets the class as partial.
    /// </summary>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Partial()
    {
        this.isPartial = true;
        return this;
    }

    /// <summary>
    /// Sets the class as sealed.
    /// </summary>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Sealed()
    {
        this.isSealed = true;
        return this;
    }

    /// <summary>
    /// Sets the classes namespace.
    /// </summary>
    /// <param name="namespace">The namespace.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Namespace(string @namespace)
    {
        this.@namespace = @namespace;
        return this;
    }

    /// <summary>
    /// Adds a using statement.
    /// </summary>
    /// <param name="using">The using.</param>
    /// <param name="static">A value indicating whether or not this is a static using.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Using(string @using, bool @static = false)
    {
        this.usings ??= new List<(string, bool)>();
        this.usings.Add((@using, @static));
        return this;
    }

    /// <summary>
    /// Specifies that an interface is implemented.
    /// </summary>
    /// <param name="interface">The interface name.</param>
    /// <returns>The <see cref="FluentStructBuilder"/>.</returns>
    public FluentStructBuilder Implements(string @interface)
    {
        this.interfaces ??= new List<string>();
        this.interfaces.Add(@interface);
        return this;
    }

    /// <summary>
    /// Specifies that the class ihnerits from another class.
    /// </summary>
    /// <param name="baseClassName">The base classes name.</param>
    /// <returns>The <see cref="FluentStructBuilder"/>.</returns>
    public FluentStructBuilder Inherits(string baseClassName)
    {
        this.baseName = baseClassName;
        return this;
    }

    /// <summary>
    /// Adds an attribute to the class.
    /// </summary>
    /// <param name="builder">An attribute builder action.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Attribute(Action<FluentAttributeBuilder> builder)
    {
        var attributeBuilder = new FluentAttributeBuilder();
        builder(attributeBuilder);
        this.attributes ??= new List<FluentAttributeBuilder>();
        this.attributes.Add(attributeBuilder);
        return this;
    }

    /// <summary>
    /// Adds a field to the class.
    /// </summary>
    /// <typeparam name="T">The fields type.</typeparam>
    /// <param name="fieldName">The fields name.</param>
    /// <param name="action">The action to define the field.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Field<T>(
        string fieldName,
        Action<FluentFieldBuilder> action = null)
    {
        return this.Field(typeof(T).FullName, fieldName, action);
    }

    /// <summary>
    /// Adds a field to the class.
    /// </summary>
    /// <param name="typeName">The fields type name.</param>
    /// <param name="fieldName">The fields name.</param>
    /// <param name="action">The action to define the field.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Field(
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

    /// <summary>
    /// Adds a constructor to the class.
    /// </summary>
    /// <param name="action">The action to define the constructor.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Constructor(
        Action<FluentMethodBuilder> action)
    {
        var builder = new FluentMethodBuilder(this.structName);
        action(builder);

        this.constructors ??= new List<FluentMethodBuilder>();
        this.constructors.Add(builder);
        return this;
    }

    /// <summary>
    /// Adds a method to the class.
    /// </summary>
    /// <param name="methodName">The methods name.</param>
    /// <param name="action">The action to define the method.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Method(
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
    /// Adds a method to the class.
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

    /// <summary>
    /// Adds a property to the class.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="action">The action to define the property.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder Property(
        string propertyName,
        Action<FluentPropertyBuilder> action)
    {
        var propertyBuilder = new FluentPropertyBuilder(propertyName);
        action(propertyBuilder);
        this.properties ??= new List<FluentPropertyBuilder>();
        this.properties.Add(propertyBuilder);
        return this;
    }

    /// <summary>
    /// Adds a sub class to the class.
    /// </summary>
    /// <param name="className">The name of the sub class.</param>
    /// <param name="action">The action to define the class.</param>
    /// <returns>The <see cref="FluentStructBuilder"/> instance.</returns>
    public FluentStructBuilder SubClass(
        string className,
        Action<FluentStructBuilder> action)
    {
        var structBuilder = new FluentStructBuilder(className);
        action(structBuilder);
        this.subClasses ??= new List<FluentStructBuilder>();
        this.subClasses.Add(structBuilder);
        return this;
    }

    /// <summary>
    /// Builds the class definition.
    /// </summary>
    /// <returns>The class definition.</returns>
    public string Build()
    {
        return this.Build(0);
    }

    /// <summary>
    /// Builds the class definition.
    /// </summary>
    /// <param name="indent">The number of characters to indent.</param>
    /// <returns>The class definition.</returns>
    private string Build(int indent)
    {
        var indentStr = new string(' ', indent);

        var structDefinition = new StringBuilder();
        if (this.@namespace != null)
        {
            structDefinition
                .Append(indentStr)
                .AppendLine($"namespace {this.@namespace};")
                .AppendLine();
        }

        if (this.usings != null)
        {
            foreach (var (@using, @static) in this.usings.OrderBy(u => u.Item2))
            {
                var staticStr = @static ? "static " : string.Empty;
                structDefinition
                    .Append(indentStr)
                    .AppendLine($"using {staticStr}global::{@using.Trim()};");
            }

            structDefinition.AppendLine();
        }

        if (this.attributes != null)
        {
            foreach (var attribute in this.attributes)
            {
                structDefinition
                    .AppendLine(attribute.Build());
            }
        }

        structDefinition
            .Append(indentStr)
            .AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"RestClientGenerator\", \"{System.Reflection.Assembly.GetCallingAssembly().GetName().Version}\")]");

        var value = this.isSealed ? "sealed " : this.isAbstract ? "abstract " : this.isPartial ? "partial " : string.Empty;
        structDefinition
            .Append(indentStr)
            .AppendLine($"{this.accessibility} {value}struct {this.structName}");

        if (this.interfaces != null ||
            string.IsNullOrWhiteSpace(this.baseName) == false)
        {
            structDefinition
                .Append(indentStr)
                .Append("   : ");

            if (string.IsNullOrWhiteSpace(this.baseName) == false)
            {
                structDefinition
                    .Append(this.baseName)
                    .AppendLine();
            }

            structDefinition
                .Append(string.Join($",\r\n{indentStr}", this.interfaces))
                .AppendLine();
        }

        structDefinition
            .Append(indentStr)
            .AppendLine("{");

        if (this.fields != null)
        {
            structDefinition.AppendLine();

            foreach (var field in this.fields)
            {
                structDefinition
                    .AppendLine(field.Build(indent + 4));
            }
        }

        if (this.constructors != null)
        {
            structDefinition.AppendLine();

            foreach (var ctor in this.constructors)
            {
                structDefinition
                    .AppendLine(ctor.Build(indent + 4, true));
            }
        }

        if (this.properties != null)
        {
            foreach (var property in this.properties)
            {
                structDefinition
                    .AppendLine(property.Build(indent + 4));
            }
        }

        if (this.methods != null)
        {
            structDefinition.AppendLine();

            foreach (var method in this.methods)
            {
                structDefinition
                    .AppendLine(method.Build(indent + 4));
            }
        }

        if (this.subClasses != null)
        {
            structDefinition.AppendLine();

            foreach (var subClass in this.subClasses)
            {
                structDefinition
                    .AppendLine(subClass.Build(indent + 4));
            }
        }

        structDefinition
            .Append(indentStr)
            .AppendLine("}");
        
        return structDefinition.ToString();
    }
}
