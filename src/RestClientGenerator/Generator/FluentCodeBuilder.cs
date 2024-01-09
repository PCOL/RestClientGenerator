namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// A fluent code builder.
/// </summary>
internal class FluentCodeBuilder
{
    /// <summary>
    /// A list of code lines.
    /// </summary>
    private readonly List<(int, string)> code = new List<(int, string)>();

    /// <summary>
    /// Adds a line of code.
    /// </summary>
    /// <param name="line">The line of code.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder AddLine(string line, int indent = 0)
    {
        code.Add((indent, line));
        return this;
    }

    /// <summary>
    /// Appends a line of code to the last line.
    /// </summary>
    /// <param name="line">The code to append.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder BlankLine(int indent = 0)
    {
        return this.AddLine(string.Empty, indent);
    }

    /// <summary>
    /// Adds a comment.
    /// </summary>
    /// <param name="comment">The comment.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder Comment(string comment, int indent = 0)
    {
        return this.AddLine($"// {comment}", indent);
    }

    /// <summary>
    /// Adds a variable declaration.
    /// </summary>
    /// <typeparam name="T">The variable type.</typeparam>
    /// <param name="name">The variable name.</param>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
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

    /// <summary>
    /// Adds a variable declaration.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <param name="name">The variable name.</param>
    /// <param name="initialValue">The initial value.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder Variable(string typeName, string name, string initialValue = "null", int indent = 0)
    {
        return this.AddLine($"{typeName} {name} = {initialValue};", indent);
    }

    /// <summary>
    /// Adds a variable assignment.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <param name="value">The value.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder Assign(string name, string value = "null", int indent = 0)
    {
        return this.AddLine($"{name} = {value};", indent);
    }

    /// <summary>
    /// Adds a return statement.
    /// </summary>
    /// <param name="value">The value to return.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder Return(string value, int indent = 0)
    {
        return this.AddLine($"return {value};", indent);
    }

    /// <summary>
    /// Adds an if block.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="action">The code to place inside the block.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder If(
        string expression,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock($"if ({expression})", action, indent);
    }

    /// <summary>
    /// Adds an else if block.
    /// </summary>
    /// <param name="expression">The expression.</param>
    /// <param name="action">The code to place inside the block.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder ElseIf(
        string expression,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock($"else if ({expression})", action, indent);
    }

    /// <summary>
    /// Adds an else block.
    /// </summary>
    /// <param name="action">The code to place inside the block.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder Else(
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock("else", action, indent);
    }

    /// <summary>
    /// Adds a using block.
    /// </summary>
    /// <param name="expression"></param>
    /// <param name="action">The code to place inside the block.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
    public FluentCodeBuilder UsingBlock(
        string expression,
        Action<FluentCodeBuilder> action,
        int indent = 0)
    {
        return this.AddBlock($"using ({expression})", action, indent);
    }

    /// <summary>
    /// Adds a block of code.
    /// </summary>
    /// <param name="command">The command.</param>
    /// <param name="action">The code to place inside the block.</param>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>The <see cref="FluentCodeBuilder"/>.</returns>
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

    /// <summary>
    /// Builds the code.
    /// </summary>
    /// <param name="indent">The number of spaces to indent.</param>
    /// <returns>a string containg the code.</returns>
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
