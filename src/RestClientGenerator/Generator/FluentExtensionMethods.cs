namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

/// <summary>
/// Extension methods for the fluent builder classes.
/// </summary>
internal static class FluentExtensionMethods
{
    /// <summary>
    /// Adds a method to the class based on a condition.
    /// </summary>
    /// <param name="classBuilder">A <see cref="FluentClassBuilder"/>.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="name">The name of the method if the condition is true.</param>
    /// <param name="elseName">The name of the method if the condition is false.</param>
    /// <param name="action">The action to build the method if the condition is true.</param>
    /// <param name="elseAction">The action to build the method if the condition is false.</param>
    /// <returns>A <see cref="FluentMethodBuilder"/> for the method.</returns>
    public static FluentMethodBuilder MethodIf(
        this FluentClassBuilder classBuilder,
        bool condition,
        string name,
        string elseName,
        Action<FluentMethodBuilder> action = null,
        Action<FluentMethodBuilder> elseAction = null)
    {
        FluentMethodBuilder method;
        if (condition)
        {
            method = classBuilder.Method(name);
            action?.Invoke(method);
        }
        else
        {
            method = classBuilder.Method(elseName);
            elseAction?.Invoke(method);
        }

        return method;  
    }

    /// <summary>
    /// Adds a method to the struct based on a condition.
    /// </summary>
    /// <param name="structBuilder">A <see cref="FluentStructBuilder"/>.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="name">The name of the method if the condition is true.</param>
    /// <param name="elseName">The name of the method if the condition is false.</param>
    /// <param name="action">The action to build the method if the condition is true.</param>
    /// <param name="elseAction">The action to build the method if the condition is false.</param>
    /// <returns>A <see cref="FluentMethodBuilder"/> for the method.</returns>
    public static FluentMethodBuilder MethodIf(
        this FluentStructBuilder structBuilder,
        bool condition,
        string name,
        string elseName,
        Action<FluentMethodBuilder> action = null,
        Action<FluentMethodBuilder> elseAction = null)
    {
        FluentMethodBuilder method;
        if (condition)
        {
            method = structBuilder.Method(name);
            action?.Invoke(method);
        }
        else
        {
            method = structBuilder.Method(elseName);
            elseAction?.Invoke(method);
        }

        return method;
    }

    /// <summary>
    /// Adds headers to a request variable.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="requestVariable">The request variable.</param>
    /// <param name="headers">A dictionary of headers.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder AddHeaders(
        this FluentCodeBuilder code,
        string requestVariable,
        IDictionary<string, string> headers)
    {
        if (headers != null)
        {
            foreach (var kvp in headers)
            {
                code
                    .AddLine($"{requestVariable}.Headers.Add(\"{kvp.Key}\", {kvp.Value});");
            }
        }

        return code;
    }

    /// <summary>
    /// Adds query strings to a variable.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="variable">The variable to add them to.</param>
    /// <param name="queryStrings">A dictionary of query strings.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder AddQueryStrings(
        this FluentCodeBuilder code,
        string variable,
        IDictionary<string, (object, IParameterSymbol)> queryStrings)
    {
        code.Variable("var", variable, "string.Empty");

        if (queryStrings != null)
        {
            code
                .Variable("string", "__value", "null")
                .BlankLine();

            foreach (var queryString in queryStrings)
            {
                var namedType = queryString.Value.Item2.Type as INamedTypeSymbol;
                if (namedType != null &&
                    namedType.IsGenericType &&
                    namedType.OriginalDefinition.ToDisplayString() == "System.Collections.Generic.IEnumerable<T>")
                {
                    code
                        .ForEach($"var __item in {queryString.Value.Item2.Name}", c => c
                            .Variable("var", "__escapedValue", "Uri.EscapeDataString(__item?.ToString())")
                            .If($"string.{nameof(string.IsNullOrWhiteSpace)}(__escapedValue) == false", ic => ic
                                .AddLine($"{variable} += {variable}.Length == 0 ? \"?\" : \"&\";")
                                .AddLine($"{variable} += \"{queryString.Key}=\" + __escapedValue;")));
                }
                else
                {
                    if (queryString.Value.Item1 is string queryStringValue)
                    {
                        var key = Uri.EscapeDataString(queryString.Key);

                        code
                            .Assign("__value", $"$\"{queryStringValue}\"")
                            .If($"string.{nameof(string.IsNullOrWhiteSpace)}(__value) == false", c => c
                                .AddLine($"{variable} += {variable}.Length == 0 ? \"?\" : \"&\";")
                                .AddLine($"{variable} += \"{key}=\" + Uri.EscapeDataString(__value);"));
                    }
                    else if (queryString.Value.Item1 is List<string> queryStringList)
                    {
                        foreach (var value in queryStringList)
                        {
                            var key = Uri.EscapeDataString(queryString.Key);

                            code.Assign("__value", $"$\"{value}\"")
                                .If($"string.{nameof(string.IsNullOrWhiteSpace)}(__value) == false", c => c
                                    .AddLine($"{variable} += {variable}.Length == 0 ? \"?\" : \"&\";")
                                    .AddLine($"{variable} += \"{key}=\" + Uri.EscapeDataString(__value);"));
                        }
                    }
                }

                code.BlankLine();
            }
        }

        return code;
    }

    /// <summary>
    /// Adds a line to the code based on a condition.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="line">The line to add if the condition is true.</param>
    /// <param name="elseLine">The line to add if the condition is false.</param>
    /// <param name="indent">The number of characters to indent the code.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder AddLineIf(
        this FluentCodeBuilder code,
        bool condition,
        string line,
        string elseLine = null,
        int indent = 0)
    {
        if (condition)
        {
            code.AddLine(line, indent);
        }
        else if (elseLine != null)
        {
            code.AddLine(elseLine, indent);
        }

        return code;
    }

    /// <summary>
    /// Adds a return statement to the code based on a condition.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="value">The value to return if the condition is true.</param>
    /// <param name="elseValue">The value to return if the condition is false.</param>
    /// <param name="indent">The number of characters to indent the code.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder ReturnIf(
        this FluentCodeBuilder code,
        bool condition,
        string value,
        string elseValue = null,
        int indent = 0)
    {
        return code
            .AddLineIf(
                condition,
                $"return {value};",
                $"return {elseValue};",
                indent);
    }

    /// <summary>
    /// Sets a variable based on a condition.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="name">The variable to set.</param>
    /// <param name="value">The value to set if the condition is true.</param>
    /// <param name="elseValue">The value to set if the condition is false.</param>
    /// <param name="indent">The number of characters to indent the code.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder AssignIf(
        this FluentCodeBuilder code,
        bool condition,
        string name,
        string value,
        string elseValue = null,
        int indent = 0)
    {
        return code
            .AddLineIf(
                condition,
                $"{name} = {value};",
                $"{name} = {elseValue};",
                indent);
    }

    /// <summary>
    /// Adds a variable declaration.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="condition">The condition.</param>
    /// <param name="typeName">The type name.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="elseInitialValue">The initial value.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder VariableIf(
        this FluentCodeBuilder code,
        bool condition,
        string typeName,
        string name,
        string initialValue = "null",
        string elseInitialValue = "null",
        int indent = 0)
    {
        return code
            .AddLineIf(
                condition,
                $"{typeName} {name} = {initialValue};",
                $"{typeName} {name} = {elseInitialValue};",
                indent);
    }

    /// <summary>
    /// Adds a variable declaration.
    /// </summary>
    /// <param name="code">A <see cref="FluentCodeBuilder"/>.</param>
    /// <param name="typeName">The type name.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="list">The list.</param>
    /// <param name="func">A function to encode the value.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public static FluentCodeBuilder Variable<T>(
        this FluentCodeBuilder code,
        string typeName,
        string name,
        IEnumerable<T> list,
        Func<T, string> func,
        int indent = 0)
    {
        var initialValue = list == null ? "null;" : list.Any() == false ? $"new {typeName}[0];" : $"new {typeName}[]";
        code.AddLine($"{typeName}[] {name} = {initialValue}", indent);
        if (list.IsNullOrEmpty() == false)
        {
            code.AddLine("{", indent);
            foreach(var item in list)
            {
                code.AddLine($"    {func(item)},", indent);
            }

            code.AddLine("};", indent);
        }

        return code;
    }


    /// <summary>
    /// Gets a type or its inner type depending on the types arity.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <returns>The type or inner type.</returns>
    public static INamedTypeSymbol GetTypeOrInnerTypeSymbol(
        this INamedTypeSymbol type)
    {
        if (type.Arity == 0)
        {
            return  type;
        }

        return type.TypeArguments.First() as INamedTypeSymbol;
    }
}
