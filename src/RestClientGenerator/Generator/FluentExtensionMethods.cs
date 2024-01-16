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
        IDictionary<string, object> queryStrings)
    {
        code.Variable("var", variable, "string.Empty");

        if (queryStrings != null)
        {
            code
                .Variable("string", "value", "null")
                .BlankLine();

            var first = true;
            foreach (var queryString in queryStrings)
            {
                if (first == true)
                {
                    code.AddLine($"{variable} = \"?\";");
                }
                else
                {
                    code.AddLine($"{variable} += \"&\";");
                }

                if (queryString.Value is string queryStringValue)
                {
                    var key = Uri.EscapeDataString(queryString.Key);

                    code.Assign("value", $"Uri.EscapeDataString($\"{queryString.Value}\")")
                        .AddLine($"{variable} += \"{key}=\" + value;");
                }
                else if (queryString.Value is List<string> queryStringList)
                {
                    foreach (var value in queryStringList)
                    {
                        var key = Uri.EscapeDataString(queryString.Key);

                        code.Assign("value", $"Uri.EscapeDataString($\"{value}\")")
                            .AddLine($"{variable} += \"{key}=\" + value;");
                    }
                }

                code.BlankLine();
                first = false;
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
