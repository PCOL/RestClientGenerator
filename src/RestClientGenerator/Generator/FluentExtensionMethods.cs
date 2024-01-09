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
                    ////query += string.Join("&", queryStringList.Select(q => $"{Uri.EscapeDataString(queryString.Key)}={Uri.EscapeDataString(q)}"));
                }

                code.BlankLine();
                first = false;
            }
        }

        return code;
    }

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
