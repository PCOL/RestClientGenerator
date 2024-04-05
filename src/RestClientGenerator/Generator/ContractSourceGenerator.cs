namespace RestClient.Generator;

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;

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
            new AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute>());
    }

    /// <inheritdoc/>
    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute> syntaxReceiver)
        {
            return;
        }

        this.GenerateClients(context, syntaxReceiver, out var clientNamespaces);
        this.GenerateRestClientContexts(context, syntaxReceiver, clientNamespaces);
    }

    /// <summary>
    /// Generates the clients.
    /// </summary>
    /// <param name="context">The generator execution context.</param>
    /// <param name="syntaxReceiver">The syntax receiver.</param>
    /// <param name="clientNamespaces">A variable to recieve the client namespaces.</param>
    private void GenerateClients(
        GeneratorExecutionContext context,
        AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute> syntaxReceiver,
        out List<string> clientNamespaces)
    {
        clientNamespaces = new List<string>();

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

            var hasAuthorization = false;
            string authorizationHeaderValue = null;
            string authorizationFactoryType = null;
            var attrs = builderContext.Symbol.GetAttributes();
            foreach (var attr in attrs)
            {
                if (attr.AttributeClass.Name == nameof(AddAuthorizationHeaderAttribute))
                {
                    hasAuthorization = true;
                    if (attr.ConstructorArguments.Any())
                    {
                        var firstArg = attr.ConstructorArguments.First();
                        if (firstArg.Type.Name == nameof(String))
                        {
                            authorizationHeaderValue = firstArg.Value.ToString();
                        }
                        else if (firstArg.Type.Name == nameof(Type))
                        {
                            authorizationFactoryType = firstArg.Value.ToString();
                        }
                    }
                }
            }

            clientNamespaces.Add($"{builderContext.Namespace}.Contracts");

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
                        ReturnsTask = methodMember.ReturnType.BaseType.Name == "Task",
                        HasAuthorization = hasAuthorization,
                        AuthorizationHeaderValue = authorizationHeaderValue,
                        AuthorizationFactoryType = authorizationFactoryType,
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
    /// <param name="clientNamespaces">A list of client namespaces.</param>
    private void GenerateRestClientContexts(
        GeneratorExecutionContext context,
        AttributeSyntaxReceiver<GenerateContractAttribute, HttpClientContractAttribute, RestClientAttribute> syntaxReceiver,
        IEnumerable<string> clientNamespaces)
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
                .Public()
                .Partial();

            foreach(var ns in clientNamespaces.Distinct())
            {
                classBuilder.Using(ns);
            }

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass.Name == nameof(RestClientAttribute))
                {
                    var firstParam = attr.ConstructorArguments.First();
                    var sym = firstParam.Value as ISymbol;
                    var value = firstParam.Value as string;
                    var name = sym.Name;

                    typeName = name;
                    if (typeName.StartsWith("I"))
                    {
                        typeName = name.Substring(1);
                    }

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