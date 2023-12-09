namespace RestClient.Generator;

using System;
using System.Linq;
using System.Text;
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

            var @namespace = GetNamespaceRecursively(symbol.ContainingNamespace);
            var @interface = symbol.Name;
            var className = $"{@interface.TrimStart('I')}Contract";
            var classBuilder = new FluentClassBuilder(className)
                .Namespace($"{@namespace}.Contracts")
                .Using("System")
                .Using("System.Threading")
                .Using("System.Threading.Tasks")
                .Public()
                .Partial()
                .Implements(@interface);

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

                    classBuilder.Method(
                        methodMember.Name,
                        m => m
                            .Public()
                            .Returns(methodMember.ReturnType.ToString())
                            .Params(
                                pb =>
                                {
                                    foreach (var param in methodMember.Parameters)
                                    {
                                        pb.Param(
                                            param.Name,
                                            p => p
                                                .TypeName(param.Type.OriginalDefinition.ToString()));
                                    }
                                })
                            .Body(
                                sb => sb
                                    .Append("    var requestUri = $\"").Append(requestUri).AppendLine("\";")
                                    .Append("    Console.WriteLine(requestUri);").AppendLine()
                                    .Append("    return Task.FromResult(\"Test\");").AppendLine()));

                }
            }

            var sourceCode = classBuilder.Build();

            context.AddSource(
                $"{className}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));

            Console.WriteLine(sourceCode);
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

            var className = symbol.Name;
            FluentClassBuilder classBuilder = null;

            var @namespace = GetNamespaceRecursively(symbol.ContainingNamespace);
            var typeName = "Unknown";
            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass.Name == "RestClientAttribute")
                {
                    var firstParam = attr.ConstructorArguments.First();
                    var sym = firstParam.Value as ISymbol;
                    var value = firstParam.Value as string;
                    var name = sym.Name;

                    typeName = name.TrimStart('I');

                    classBuilder = new FluentClassBuilder(className)
                        .Namespace(@namespace)
                        .Using("System.Threading")
                        .Using("System.Threading.Tasks")
                        .Using($"{@namespace}.Contracts")
                        .Public()
                        .Partial()
                        .Property(
                            name,
                            p => p
                                .Public()
                                .Returns(firstParam.Value.ToString())
                                .Getter(
                                    c => c
                                        .Append("return new ")
                                        .Append(typeName)
                                        .Append("Contract();")
                                        .AppendLine()));
                }
            }

            var sourceCode = classBuilder.Build();

            context.AddSource(
                $"{className}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));

            Console.WriteLine(sourceCode);
        }
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