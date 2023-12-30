namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Xsl;

public class FluentCodeBuilder
{
    private readonly List<(int, string)> code = new List<(int, string)>();

    public FluentCodeBuilder AddLine(string line, int indent = 0)
    {
        code.Add((indent, line));
        return this;
    }

    public FluentCodeBuilder AppendLine(string line, int indent = 0)
    {
        if (code.Any() == false)
        {
            code.Add((indent, line));
        }
        else
        {
            var index = code.Count - 1;
            var (i, l) = code[index];
            code[index] = (i, l + line);
        }

        return this;
    }


    public FluentCodeBuilder BlankLine(int indent = 0)
    {
        return this.AddLine(string.Empty, indent);
    }

    public FluentCodeBuilder Comment(string comment, int indent = 0)
    {
        return this.AddLine($"// {comment}", indent);
    }

    public FluentCodeBuilder Variable<T>(string name, T initialValue = default, int indent = 0)
    {
        string value;
        if (initialValue != null)
        {
            value = initialValue.ToString();
            if (initialValue is bool)
            {
                value = value.ToLower();
            }
        }
        else
        {
            value = "null";
        }

        return this.AddLine($"{typeof(T).FullName} {name} = {value};", indent);
    }

    public FluentCodeBuilder Variable(string typeName, string name, string initialValue = "null", int indent = 0)
    {
        return this.AddLine($"{typeName} {name} = {initialValue};", indent);
    }

    public FluentCodeBuilder Assign(string name, string value = "null", int indent = 0)
    {
        return this.AddLine($"{name} = {value};", indent);
    }

    public FluentCodeBuilder Return(string value, int indent = 0)
    {
        return this.AddLine($"return {value};", indent);
    }

    public FluentCodeBuilder If(
        string expression,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock($"if ({expression})", action, indent);
    }

    public FluentCodeBuilder ElseIf(
        string expression,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock($"else if ({expression})", action, indent);
    }

    public FluentCodeBuilder Else(
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock("else", action, indent);
    }

    public FluentCodeBuilder UsingBlock(
        string expression,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock($"using ({expression})", action, indent);
    }

    private FluentCodeBuilder AddBlock(
        string command,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        var builder = new FluentCodeBuilder();
        action(builder);

        this.AddLine(command, indent);
        this.AddLine("{", indent);
        foreach (var (ind, line) in builder.code)
        {
            this.AddLine(line, indent + 1 + ind);
        }

        this.AddLine("}", indent);

        return this;
    }

    public string Build(int indent)
    {
        var indentStr = new string(' ', indent);
        var indentTabStr = new string(' ', 4);

        var codeStr = new StringBuilder(1024);
        foreach (var (ind, line) in code)
        {
            codeStr.Append(indentStr);
            for (var i = 0; i < ind; i++)
            {
                codeStr.Append(indentTabStr);
            }

            codeStr.AppendLine(line);
        }

        return codeStr.ToString();
    }
}
