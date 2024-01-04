namespace RestClient.Generator;

using System;
using System.Collections.Generic;

internal static class FluentExtensionMethods
{
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
}
