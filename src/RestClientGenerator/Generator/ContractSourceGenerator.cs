namespace RestClient.Generator;

using System;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading;

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
            var className = $"{@interface.RemoveLeadingI()}_Contract";
            var classBuilder = new FluentClassBuilder(className)
                .Namespace($"{@namespace}.Contracts")
                .Using("System")
                .Using("System.Net")
                .Using("System.Net.Http")
                .Using("System.Threading")
                .Using("System.Threading.Tasks")
                .Public()
                .Partial()
                .Implements(@interface)
                .Field<RestClientContext>("context")
                .Constructor(c => c
                    .Public())
                .Constructor(c => c
                    .Public()
                    .Param<RestClientContext>("context")
                    .Body(c => c
                        .Assign("this.context", "context")));

            foreach (var member in symbol.GetAllMembers())
            {
                if (member is IMethodSymbol methodMember)
                {
                    BuildMethodSubClass(model, member, methodMember, classBuilder);
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
                if (attr.AttributeClass.Name == nameof(RestClientAttribute))
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

    private void BuildMethodSubClass(
        SemanticModel semanticModel,
        ISymbol member,
        IMethodSymbol methodMember,
        FluentClassBuilder classBuilder)
    {
        var memberClassName = $"{methodMember.Name}_class";
        classBuilder.SubClass(
            memberClassName,
            methodSubClassBuilder =>
            {
                var contentParameter = this.GetParameterWithAttribute(
                    methodMember,
                    nameof(SendAsContentAttribute),
                    out var contentAttribute);

                methodSubClassBuilder
                    .Private()
                    .Field<RestClientContext>("context")
                    .Constructor(m => m
                        .Public()
                        .Param<RestClientContext>("context")
                        .Body(c => c.Assign("this.context", "context")));

                var sendMethod = BuildSendMethod(
                    semanticModel,
                    member,
                    methodMember,
                    contentParameter,
                    methodSubClassBuilder);

                ////methodSubClassBuilder.Method("SendAsync")
                ////    .Private()
                ////    .Async()
                ////    .Returns("System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>")
                ////    .Param<CancellationToken>("cancellationToken")
                ////    .Body(c => c
                ////        .UsingBlock("var request = this.CreateRequest()", b => b
                ////            .Variable("var", "httpClient", "this.context.GetHttpClient()")
                ////            .Return("await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false)")));

                var createRequestMethod = methodSubClassBuilder.Method("CreateRequest")
                    .Private()
                    .Returns<HttpRequestMessage>()
                    .Body(c => c.AddLine("throw new NotSupportedException();"));

                var retryMethod = methodSubClassBuilder.Method("CreateRetry")
                    .Private()
                    .Returns<IRetry>()
                    .Body(c => c.Return("null"));

                var requestUriMethod = methodSubClassBuilder.Method("GetRequestUri")
                    .Public()
                    .Returns<string>()
                    .Body(c => c.Return("null"));

                var innerReturnType = this.GetInnerType(methodMember.ReturnType.ToDisplayParts());

                var executeMethod = BuildExecuteMethod(
                    semanticModel,
                    member,
                    methodMember,
                    contentParameter,
                    methodSubClassBuilder);

                ////var executeCode = new FluentCodeBuilder()
                ////    .Variable<HttpResponseMessage>("response")
                ////    .Variable("var", "retry", "this.CreateRetry()")
                ////    .If("retry != null", m => m
                ////        .Assign("response", "await retry.ExecuteAsync(() => { return this.SendAsync(cancellationToken); }).ConfigureAwait(false);"))
                ////    .Else(m => m
                ////        .Assign("response", "await this.SendAsync(cancellationToken).ConfigureAwait(false)"))
                ////    .BlankLine()
                ////    .Return("await this.ProcessResponseAsync(response)");

                ////var executeMethod = methodSubClassBuilder.Method("ExecuteAsync")
                ////    .Public()
                ////    .Async()
                ////    .Returns(methodMember.ReturnType.ToString())
                ////    .Params(
                ////        pb =>
                ////        {
                ////            var hasCancellationToken = false;
                ////            foreach (var param in methodMember.Parameters)
                ////            {
                ////                var typeName = param.Type.OriginalDefinition.ToString();
                ////                pb.Param(param.Name, typeName);
                ////                if (typeName == "System.Threading.CancellationToken")
                ////                {
                ////                    hasCancellationToken = true;
                ////                }
                ////            }

                ////            if (hasCancellationToken == false)
                ////            {
                ////                pb.Param<CancellationToken>("cancellationToken", p => p.Default("default"));
                ////            }
                ////        })
                ////    .Body(executeCode);

                var processResponseMethod = methodSubClassBuilder.Method("ProcessResponseAsync")
                    .Private()
                    .Async()
                    .Returns(methodMember.ReturnType.ToString())
                    .Param<HttpResponseMessage>("response")
                    .Body(c => c
                        .AddLine("response.EnsureSuccessStatusCode();")
                        .AddLine("await Task.Yield();")
                        .Return("null"));

                var attrs = member.GetAttributes();
                foreach (var attr in attrs)
                {
                    if (attr.AttributeClass.BaseType.ContainingNamespace.Name == nameof(RestClient))
                    {
                        if (attr.AttributeClass.BaseType.Name == nameof(MethodAttribute))
                        {
                            var requestUri = attr.ConstructorArguments.First().Value.ToString();

                            requestUriMethod
                                .Body(c => c
                                    .Variable("var", "baseUrl", "this.context.Options.BaseUrl")
                                    .If("baseUrl != null", c => c
                                        .Return($"baseUrl.TrimEnd('/') + \"/{requestUri}\""))
                                    .BlankLine()
                                    .AddLine($"return \"{requestUri}\";"));

                            if (attr.AttributeClass.Name == nameof(GetAttribute))
                            {
                                createRequestMethod
                                    .Body(c => c
                                        .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Get, this.GetRequestUri())")
                                        .AddLine("request.Headers.Add(\"Accept\", \"application/json\");")
                                        .Return("request"));
                            }
                            else
                            {
                                if (attr.AttributeClass.Name == nameof(PostAttribute))
                                {
                                    var code = new FluentCodeBuilder()
                                        .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Post, this.GetRequestUri())");
                                        
                                    if (contentParameter != null)
                                    {
                                        createRequestMethod
                                            .Param(contentParameter.Name, contentParameter.Type.OriginalDefinition.ToString());
                                    }

                                    code.Return("request");

                                    createRequestMethod.Body(code);
                                }
                            }
                        }
                    }

                    if (attr.AttributeClass.Name == nameof(RetryAttribute))
                    {
                        var retryCode = new FluentCodeBuilder()
                            .Variable("RestClient.Retry", "retry", "new RestClient.Retry()")
                            .AddLine("retry = retry");

                        foreach (var arg in attr.NamedArguments)
                        {
                            if (arg.Key == nameof(RetryAttribute.DoubleWaitTimeOnRetry) &&
                                arg.Value.Type.Name == nameof(Boolean))
                            {
                                var b = (bool)arg.Value.Value;
                                retryCode.AddLine($".SetDoubleWaitTimeOnRetry({b.ToString().ToLower()})", 1);
                            }
                            else if (arg.Key == nameof(RetryAttribute.RetryCount))
                            {
                                var count = (int)arg.Value.Value;
                                retryCode.AddLine($".SetRetryLimit({count})", 1);
                            }
                        }

                        retryCode
                            .AppendLine(";")
                            .Return("retry");

                        retryMethod.Body(retryCode);
                    }

                    if (attr.AttributeClass.Name == nameof(HttpResponseProcessorAttribute))
                    {
                        var responseProcessorType = attr.ConstructorArguments.First().Value.ToString();

                        var codeBuilder = new FluentCodeBuilder()
                            .Variable("var", "responseProcessor", $"new {responseProcessorType}()")
                            .BlankLine()
                            .Variable("var", "result", "await responseProcessor.ProcessResponseAsync(response)")
                            .Return("result");

                        processResponseMethod
                            .Async()
                            .Body(codeBuilder);
                    }
                }
            });

        var parametersStr = string.Join(", ", methodMember.Parameters.Select(p => p.Name));

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
                            pb.Param(param.Name, param.Type.OriginalDefinition.ToString());
                        }
                    })
                .Body(
                    c => c
                        .Variable("var", "request", $"new {memberClassName}(this.context)")
                        .AddLine("Console.WriteLine(request.GetRequestUri());")
                        .Return($"request.ExecuteAsync({parametersStr})")));
    }

    private FluentMethodBuilder BuildExecuteMethod(
        SemanticModel semanticModel,
        ISymbol member,
        IMethodSymbol methodMember,
        IParameterSymbol contentParameterSymbol,
        FluentClassBuilder classBuilder)
    {
        var contentParameter = contentParameterSymbol != null ? $"{contentParameterSymbol.Name}, " : string.Empty;

        var executeCode = new FluentCodeBuilder()
            .Variable<HttpResponseMessage>("response")
            .Variable("var", "retry", "this.CreateRetry()")
            .If("retry != null", m => m
                .Assign("response", $"await retry.ExecuteAsync(() => {{ return this.SendAsync({contentParameter}cancellationToken); }}).ConfigureAwait(false);"))
            .Else(m => m
                .Assign("response", $"await this.SendAsync({contentParameter}cancellationToken).ConfigureAwait(false)"))
            .BlankLine()
            .Return("await this.ProcessResponseAsync(response)");

        var executeMethod = classBuilder.Method("ExecuteAsync")
            .Public()
            .Async()
            .Returns(methodMember.ReturnType.ToString())
            .Params(
                pb =>
                {
                    var hasCancellationToken = false;
                    foreach (var param in methodMember.Parameters)
                    {
                        var typeName = param.Type.OriginalDefinition.ToString();
                        pb.Param(param.Name, typeName);
                        if (typeName == "System.Threading.CancellationToken")
                        {
                            hasCancellationToken = true;
                        }
                    }

                    if (hasCancellationToken == false)
                    {
                        pb.Param<CancellationToken>("cancellationToken", p => p.Default("default"));
                    }
                })
            .Body(executeCode);

        return executeMethod;
    }

    private FluentMethodBuilder BuildSendMethod(
        SemanticModel semanticModel,
        ISymbol member,
        IMethodSymbol methodMember,
        IParameterSymbol contentParameterSymbol,
        FluentClassBuilder classBuilder)
    {
           
        var sendMethod = classBuilder.Method("SendAsync")
            .Private()
            .Async()
            .Returns("System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>");

        var requestParam = string.Empty;
        if (contentParameterSymbol != null)
        {
            sendMethod.Param(
                contentParameterSymbol.Name,
                contentParameterSymbol.Type.OriginalDefinition.ToString());

            requestParam = contentParameterSymbol.Name;
        }

        sendMethod
            .Param<CancellationToken>("cancellationToken")
            .Body(c => c
                .UsingBlock($"var request = this.CreateRequest({requestParam})", b => b
                    .Variable("var", "httpClient", "this.context.GetHttpClient()")
                    .Return("await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false)")));

        return sendMethod;
    }

    private IParameterSymbol GetParameterWithAttribute(
        IMethodSymbol methodMember,
        string attributeName,
        out AttributeData attribute)
    {
        attribute = null;
        foreach (var param in methodMember.Parameters)
        {
            var attrs = param.GetAttributes();
            if (attrs.Any() == true)
            {
                foreach (var attr in attrs)
                {
                    if (attr.AttributeClass.Name == attributeName)
                    {
                        attribute = attr;
                        return param;
                    }
                }
            }
        }

        return null;
    }

    private string GetInnerType(ImmutableArray<SymbolDisplayPart> parts)
    {
        var capture = false;
        var typeName = string.Empty;
        for(int i = 0; i < parts.Length; i++)
        {
            if (capture == false &&
                parts[i].Kind == SymbolDisplayPartKind.Punctuation &&
                parts[i].ToString() == "<")
            {
                capture = true;
                continue;
            }
            else if (capture == true &&
                parts[i].Kind == SymbolDisplayPartKind.Punctuation &&
                parts[i].ToString() == ">" &&
                i == parts.Length - 1)
            {
                capture = false;
                continue;
            }

            if (capture == true)
            {
                typeName += parts[i].ToString();
            }
        }

        return typeName;
    }
}