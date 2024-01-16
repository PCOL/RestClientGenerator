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
internal class ContractSourceGenerator
    : ISourceGenerator
{
    /// <inheritdoc/>
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() =>
            new AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute>());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute> syntaxReceiver)
        {
            return;
        }

        this.GenerateClients(context, syntaxReceiver);
        this.GenerateRestClientContexts(context, syntaxReceiver);
    }

    /// <summary>
    /// Generates the clients.
    /// </summary>
    /// <param name="context">The generator execution context.</param>
    /// <param name="syntaxReceiver">The syntax receiver.</param>
    private void GenerateClients(
        GeneratorExecutionContext context,
        AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute> syntaxReceiver)
    {
        foreach (var interfaceSyntax in syntaxReceiver.Interfaces)
        {
            var builderContext = new ClassBuilderContext();
            
            // Converting the interface to semantic model to access much more meaningful data.
            builderContext.Model = context.Compilation.GetSemanticModel(interfaceSyntax.SyntaxTree);

            // Parse to declared symbol, so you can access each part of code separately,
            // such as interfaces, methods, members, contructor parameters etc.
            builderContext.Symbol = builderContext.Model.GetDeclaredSymbol(interfaceSyntax);

            builderContext.Namespace = GetNamespaceRecursively(builderContext.Symbol.ContainingNamespace);
            builderContext.TypeName = builderContext.Symbol.Name;
            builderContext.ClassName = $"{builderContext.TypeName.RemoveLeadingI()}_Contract";

            builderContext.Symbol.GetAttributeNamedArguments(
                nameof(HttpClientContractAttribute),
                new (string, string, Action<object>)[]
                {
                    (nameof(HttpClientContractAttribute.Route), nameof(String), (v) => builderContext.Route = (string)v),
                    (nameof(HttpClientContractAttribute.ContentType), nameof(String), (v) => builderContext.ContentType = (string)v),
                });

            var classBuilder = new FluentClassBuilder(builderContext.ClassName)
                .Namespace($"{builderContext.Namespace}.Contracts")
                .Using("System")
                .Using("System.Collections.Generic")
                .Using("System.Net")
                .Using("System.Net.Http")
                .Using("System.Threading")
                .Using("System.Threading.Tasks")
                .Public()
                .Partial()
                .Implements(builderContext.TypeName)
                .Field<RestClientContext>("__context")
                .Constructor(c => c
                    .Public())
                .Constructor(c => c
                    .Public()
                    .Param<RestClientContext>("context")
                    .Body(c => c
                        .Assign("this.__context", "context")));

            foreach (var member in builderContext.Symbol.GetAllMembers())
            {
                if (member is IMethodSymbol methodMember)
                {
                    var methodBuilderContext = new MethodBuilderContext()
                    {
                        ClassBuilderContext = builderContext,
                        ClassBuilder = classBuilder,
                        Member = member,
                        MethodMember = methodMember,
                        Route = builderContext.Route,
                        ContentType = builderContext.ContentType ?? "application/json",
                        ReturnType = (INamedTypeSymbol)methodMember.ReturnType,
                        ReturnsTask = methodMember.ReturnType.BaseType.Name == "Task"
                    };

                    methodBuilderContext.ProcessParameters();

                    methodBuilderContext.ProcessMethodAttributes();

                    methodBuilderContext.Generate();
                }
            }

            var sourceCode = classBuilder.Build();

            context.AddSource(
                $"{builderContext.ClassName}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));

            if (builderContext.Symbol.HasAttribute(nameof(OutputCodeAttribute)))
            {
                Console.WriteLine(sourceCode);
            }
        }
    }

    /// <summary>
    /// Generate the REST client contexts.
    /// </summary>
    /// <param name="context">The generator execution context.</param>
    /// <param name="syntaxReceiver">The syntax receiver.</param>
    private void GenerateRestClientContexts(
        GeneratorExecutionContext context,
        AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute> syntaxReceiver)
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

            classBuilder = new FluentClassBuilder(className)
                .Namespace(@namespace)
                .Using("System.Threading")
                .Using("System.Threading.Tasks")
                .Using($"{@namespace}.Contracts")
                .Public()
                .Partial();

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass.Name == nameof(RestClientAttribute))
                {
                    var firstParam = attr.ConstructorArguments.First();
                    var sym = firstParam.Value as ISymbol;
                    var value = firstParam.Value as string;
                    var name = sym.Name;

                    typeName = name.TrimStart('I');

                    classBuilder
                        .Method($"Get{typeName}", m => m
                            .Public()
                            .Returns(firstParam.Value.ToString())
                            .Body(c => c
                                .Return($"new {typeName}_Contract(this)")));

                }
            }

            var sourceCode = classBuilder.Build();

            context.AddSource(
                $"{className}.g.cs",
                SourceText.From(sourceCode, Encoding.UTF8));

            if (symbol.HasAttribute(nameof(OutputCodeAttribute)))
            {
                Console.WriteLine(sourceCode);
            }
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