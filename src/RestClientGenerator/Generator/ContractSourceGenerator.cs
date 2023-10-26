namespace RestClient.Generator;

using System.IO;
using System.Linq;
using System.Text;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Xml.Schema;

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
            new AttributeSyntaxReceiver<GenerateContractAttribute, RestClientAttribute>());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver<GenerateContractAttribute, RestClientAttribute> syntaxReceiver)
        {
            return;
        }

        this.GenerateClients(context, syntaxReceiver);
        this.GenerateRestClientContexts(context, syntaxReceiver);
    }

    private void GenerateClients(
        GeneratorExecutionContext context,
        AttributeSyntaxReceiver<GenerateContractAttribute, RestClientAttribute> syntaxReceiver)
    {
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
                    var requestUri = string.Empty;
                    var attrs = member.GetAttributes();
                    foreach (var attr in attrs)
                    {
                        if (attr.AttributeClass.BaseType.ContainingNamespace.Name == nameof(RestClient) &&
                            attr.AttributeClass.BaseType.Name == nameof(MethodAttribute))
                        {
                            requestUri = attr.ConstructorArguments.First().Value.ToString();
                        }
                    }

                    generatedCode
                        .Append("  public ")
                        .Append(methodMember.ReturnType)
                        .Append(' ')
                        .Append(methodMember.Name)
                        .Append('(')
                        .Append(string.Join(", ", methodMember.Parameters.Select(p => $"{p.Type.Name} {p.Name}")))
                        .Append(')').AppendLine()
                        .Append("  {").AppendLine()
                        .Append("    var requestUri = $\"").Append(requestUri).AppendLine("\";")
                        .Append("    Console.WriteLine(requestUri);").AppendLine()
                        .Append("    return Task.FromResult(\"Test\");").AppendLine()
                        .Append("  }").AppendLine()
                        .AppendLine();

                }
            }

            //Console.WriteLine(generatedCode.ToString());

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

            Console.WriteLine(sourceCode);

            context.AddSource(
                $"{symbol.Name.TrimStart('I')}{templateParameter ?? "Contract"}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private void GenerateRestClientContexts(
        GeneratorExecutionContext context,
        AttributeSyntaxReceiver<GenerateContractAttribute, RestClientAttribute> syntaxReceiver)
    {
        foreach (var classSyntax in syntaxReceiver.Classes)
        {
            // Converting the class to semantic model to access much more meaningful data.
            var model = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);

            // Parse to declared symbol, so you can access each part of code separately,
            // such as interfaces, methods, members, contructor parameters etc.
            var symbol = model.GetDeclaredSymbol(classSyntax);

            var generatedCode = new StringBuilder();

            var typeName = "Unknown";
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass.Name == "RestClientAttribute")
                {
                    var firstParam = attr.ConstructorArguments.First();
                    var sym = firstParam.Value as ISymbol;
                    var value = firstParam.Value as string;
                    var name = sym.Name;

                    typeName = name;

                    generatedCode
                        .Append("  public ")
                        .Append(firstParam.Value)
                        .Append(' ')
                        .Append(name).AppendLine()
                        .Append("  {").AppendLine()
                        .Append("    get").AppendLine()
                        .Append("    {").AppendLine()
                        .Append("      return new ").Append(name.TrimStart('I')).Append("Contract();").AppendLine()
                        .Append("    }").AppendLine()
                        .Append("  }").AppendLine()
                        .AppendLine();

                }
            }

            //Console.WriteLine(generatedCode.ToString());

            // Generate the real source code. Pass the template parameter if there is a overriden template.
            var sourceCode = GetSourceCodeForNamedTemplate(symbol, "RestClient.Templates.RestClientContext.txt");

            sourceCode = sourceCode.Replace("{{" + nameof(DefaultTemplateParameters.InterfaceImpl) + "}}", generatedCode.ToString());

            Console.WriteLine(sourceCode);

            context.AddSource(
                $"{symbol.Name}_{typeName}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));
        }
    }

    private string GetSourceCodeFor(ISymbol symbol, string template = null)
    {
        // If template isn't provieded, use default one from embeded resources.
        template ??= GetEmbededResource("RestClient.Templates.Default.txt");

        // Can't use scriban at the moment, make it manually for now.
        return template
            .Replace("{{" + nameof(DefaultTemplateParameters.ClassName) + "}}", symbol.Name.TrimStart('I'))
            .Replace("{{" + nameof(DefaultTemplateParameters.InterfaceName) + "}}", symbol.Name)
            .Replace("{{" + nameof(DefaultTemplateParameters.Namespace) + "}}", GetNamespaceRecursively(symbol.ContainingNamespace))
            .Replace("{{" + nameof(DefaultTemplateParameters.PrefferredNamespace) + "}}", symbol.ContainingAssembly.Name);
    }

    private string GetSourceCodeForNamedTemplate(ISymbol symbol, string templateName)
    {
        var template = GetEmbededResource(templateName);

        // Can't use scriban at the moment, make it manually for now.
        return template
            .Replace("{{" + nameof(DefaultTemplateParameters.ClassName) + "}}", symbol.Name)
            .Replace("{{" + nameof(DefaultTemplateParameters.InterfaceName) + "}}", symbol.Name)
            .Replace("{{" + nameof(DefaultTemplateParameters.Namespace) + "}}", GetNamespaceRecursively(symbol.ContainingNamespace))
            .Replace("{{" + nameof(DefaultTemplateParameters.PrefferredNamespace) + "}}", symbol.ContainingAssembly.Name);
    }



    private string GetEmbededResource(string path)
    {
        try
        {
            using var stream = GetType().Assembly.GetManifestResourceStream(path);
            using var streamReader = new StreamReader(stream);
            return streamReader.ReadToEnd();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

        return null;
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