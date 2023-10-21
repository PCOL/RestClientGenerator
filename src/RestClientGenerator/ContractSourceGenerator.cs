namespace RestClientGenerator;

using System.IO;
using System.Linq;
using System.Text;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Contract source generator.
/// </summary>
[Generator]
public class ContractSourceGenerator
    : ISourceGenerator
{
    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() =>
            new AttributeSyntaxReceiver<GenerateContractAttribute>());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver<GenerateContractAttribute> syntaxReceiver)
        {
            return;
        }

        foreach (var interfaceSyntax in syntaxReceiver.Interfaces)
        {
            // Converting the interface to semantic model to access much more meaningful data.
            var model = context.Compilation.GetSemanticModel(interfaceSyntax.SyntaxTree);

            // Parse to declared symbol, so you can access each part of code separately,
            // such as interfaces, methods, members, contructor parameters etc.
            var symbol = model.GetDeclaredSymbol(interfaceSyntax);

            var generatedCode = new StringBuilder();

            foreach (var member in symbol.GetAllMembers())
            {
                if (member is IMethodSymbol methodMember)
                {
                    generatedCode
                        .Append("  public ")
                        .Append(methodMember.ReturnType)
                        .Append(' ')
                        .Append(methodMember.Name)
                        .Append('(')
                        .Append(string.Join(", ", methodMember.Parameters.Select(p => $"{p.Type.Name} {p.Name}")))
                        .AppendLine(")")
                        .AppendLine("  {")
                        .AppendLine("    return Task.FromResult(\"Test\");")
                        .AppendLine("  }")
                        .AppendLine();

                    Console.WriteLine(generatedCode.ToString());
                }
            }

            // Finding my GenerateContracyAttribute over it. I'm sure this attribute is placed, because my syntax receiver already checked before.
            // So, I can surely execute following query.
            var attribute = interfaceSyntax
                .AttributeLists
                .SelectMany(sm => sm.Attributes)
                .First(x => x.Name
                    .ToString()
                    .EnsureEndsWith("Attribute")
                    .Equals(typeof(GenerateContractAttribute).Name));

            // Getting constructor parameter of the attribute. It might be not presented.
            var templateParameter = attribute
                .ArgumentList?
                .Arguments
                .FirstOrDefault()?
                .GetLastToken()
                .ValueText; // Temprorary... Attribute has only one argument for now.

            // Can't access embeded resource of main project.
            // So overridden template must be marked as Analyzer Additional File to be able to be accessed by an analyzer.
            var overridenTemplate = templateParameter != null ?
                context.AdditionalFiles
                    .FirstOrDefault(x => x.Path.EndsWith(templateParameter))?
                    .GetText()
                    .ToString() :
                null;

            // Generate the real source code. Pass the template parameter if there is a overriden template.
            var sourceCode = GetSourceCodeFor(symbol, overridenTemplate);

            sourceCode = sourceCode.Replace("{{" + nameof(DefaultTemplateParameters.InterfaceImpl) + "}}", generatedCode.ToString());

            context.AddSource(
                $"{symbol.Name.TrimStart('I')}{templateParameter ?? "Contract"}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private string GetSourceCodeFor(ISymbol symbol, string template = null)
    {
        // If template isn't provieded, use default one from embeded resources.
        template ??= GetEmbededResource("RestClientGenerator.Templates.Default.txt");

        // Can't use scriban at the moment, make it manually for now.
        return template
            .Replace("{{" + nameof(DefaultTemplateParameters.ClassName) + "}}", symbol.Name.TrimStart('I'))
            .Replace("{{" + nameof(DefaultTemplateParameters.InterfaceName) + "}}", symbol.Name)
            .Replace("{{" + nameof(DefaultTemplateParameters.Namespace) + "}}", GetNamespaceRecursively(symbol.ContainingNamespace))
            .Replace("{{" + nameof(DefaultTemplateParameters.PrefferredNamespace) + "}}", symbol.ContainingAssembly.Name);
    }

    private string GetEmbededResource(string path)
    {
        using var stream = GetType().Assembly.GetManifestResourceStream(path);
        using var streamReader = new StreamReader(stream);
        return streamReader.ReadToEnd();
    }

    private string GetNamespaceRecursively(INamespaceSymbol symbol)
    {
        if (symbol.ContainingNamespace == null)
        {
            return symbol.Name;
        }

        return (GetNamespaceRecursively(symbol.ContainingNamespace) + "." + symbol.Name).Trim('.');
    }
}