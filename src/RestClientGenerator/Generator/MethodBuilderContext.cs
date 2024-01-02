namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.CodeAnalysis;
using static System.Runtime.CompilerServices.RuntimeHelpers;

internal class MethodBuilderContext
{
    public ClassBuilderContext ClassBuilderContext { get; set; }

    public FluentClassBuilder ClassBuilder { get; set; }

    public ISymbol Member {  get; set; }

    public IMethodSymbol MethodMember { get; set; }

    public string Route { get; set; }

    public string ContentType { get; set; }

    public string RequestUri { get; set; }

    public HttpMethod RequestMethod { get; set; }

    public bool HasRetry { get; set; }

    public bool? DoubleOnRetry { get; set; }

    public int? RetryCount { get; set; }

    public string ResponseProcessorType { get; set; }

    public bool HasAuthorization { get; set; }

    public string AuthorizationHeaderValue { get; set; }

    public string AuthorizationFactoryType { get; set; }

    public List<KeyValuePair<string, string>> formUrlProperties;

    public void ProcessAttributes()
    {
        var attrs = this.Member.GetAttributes();
        foreach (var attr in attrs)
        {
            if (attr.AttributeClass.BaseType.ContainingNamespace.Name == nameof(RestClient))
            {
                if (attr.AttributeClass.BaseType.Name == nameof(MethodAttribute))
                {
                    this.RequestUri = attr.ConstructorArguments.First().Value.ToString();

                    this.RequestMethod = attr.AttributeClass.Name switch
                    {
                        nameof(PostAttribute) => HttpMethod.Post,
                        nameof(GetAttribute) => HttpMethod.Get,
                        nameof(PutAttribute) => HttpMethod.Put,
                        nameof(PatchAttribute) => new HttpMethod("Patch"),
                        nameof(DeleteAttribute) => HttpMethod.Delete,
                        _ => throw new Exception("Unknown method type")
                    };
                }
            }

            if (attr.AttributeClass.Name == nameof(RetryAttribute))
            {
                this.HasRetry = true;
                foreach (var arg in attr.NamedArguments)
                {
                    if (arg.GetValue<bool>(nameof(RetryAttribute.DoubleWaitTimeOnRetry), nameof(Boolean), out var doubleOnRetry))
                    {
                        this.DoubleOnRetry = doubleOnRetry;
                    }
                    else if (arg.GetValue<int>(nameof(RetryAttribute.RetryCount), nameof(Int32), out var retryCount))
                    {
                        this.RetryCount = retryCount;
                    }
                }
            }

            if (attr.AttributeClass.Name == nameof(HttpResponseProcessorAttribute))
            {
                this.ResponseProcessorType = attr.ConstructorArguments.First().Value.ToString();
            }

            if (attr.AttributeClass.Name == nameof(AddAuthorizationHeaderAttribute))
            {
                this.HasAuthorization = true;
                if (attr.ConstructorArguments.Any())
                {
                    var firstArg = attr.ConstructorArguments.First();
                    if (firstArg.Type.Name == nameof(String))
                    {
                        this.AuthorizationHeaderValue = firstArg.Value.ToString();
                    }
                    else if (firstArg.Type.Name == nameof(Type))
                    {
                        this.AuthorizationFactoryType = firstArg.Value.ToString();
                    }
                }
            }

            if (attr.AttributeClass.Name == nameof(AddFormUrlEncodedPropertyAttribute))
            {
                this.formUrlProperties ??= new List<KeyValuePair<string, string>>();
                var firstArg = attr.ConstructorArguments.First();
                var secondArg = attr.ConstructorArguments.Skip(1).First();
                this.formUrlProperties.Add(
                    new KeyValuePair<string, string>(
                        firstArg.IsNull ? null : firstArg.Value.ToString(),
                        secondArg.IsNull ? null : secondArg.Value.ToString()));
            }
        }
    }

    public void Build()
    {
        var memberClassName = $"{this.MethodMember.Name}_class";
        this.ClassBuilder.SubClass(
            memberClassName,
            methodSubClassBuilder =>
            {
                var contentParameterSymbol = this.MethodMember.GetParameterWithAttribute(
                    nameof(SendAsContentAttribute),
                    out var contentAttribute);

                // Add 'context' field to class
                methodSubClassBuilder
                    .Private()
                    .Field<RestClientContext>("context")
                    .Constructor(m => m
                        .Public()
                        .Param<RestClientContext>("context")
                        .Body(c => c.Assign("this.context", "context")));

                // Build the 'SendAsync' method
                var sendMethod = methodSubClassBuilder.Method("SendAsync")
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

                // Generate 'CreateRequest' method.
                var createRequestMethod = methodSubClassBuilder.Method("CreateRequest")
                    .Private()
                    .Returns<HttpRequestMessage>()
                    .Body(c => c.AddLine("throw new NotSupportedException();"));

                if (this.RequestMethod == HttpMethod.Get)
                {
                    createRequestMethod.Body(c => c
                        .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Get, this.GetRequestUri())")
                        .AddLine($"request.Headers.Add(\"Accept\", \"{this.ContentType}\");")
                        .Return("request"));
                }
                else if (this.RequestMethod == HttpMethod.Post)
                {
                    var code = new FluentCodeBuilder()
                        .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Post, this.GetRequestUri())");

                    if (contentParameterSymbol != null)
                    {
                        createRequestMethod
                            .Param(contentParameterSymbol.Name, contentParameterSymbol.Type.OriginalDefinition.ToString());

                        code
                            .Variable("var", "json", $"this.context.Serialize({contentParameterSymbol.Name})")
                            .AddLine($"request.Content = new StringContent(json, System.Text.UTF8Encoding.UTF8, \"{this.ContentType}\");");
                    }

                    code.Return("request");

                    createRequestMethod.Body(code);
                }

                // Generate 'CreateRetry' method.
                var retryMethod = methodSubClassBuilder.Method("CreateRetry")
                    .Private()
                    .Returns<IRetry>();

                if (this.HasRetry == true)
                {
                    var retryCode = new FluentCodeBuilder()
                        .Variable("RestClient.Retry", "retry", "new RestClient.Retry()")
                        .AddLine("retry = retry");

                    if (this.DoubleOnRetry.HasValue)
                    {
                        retryCode.AddLine($".SetDoubleWaitTimeOnRetry({this.DoubleOnRetry.Value.ToString().ToLower()})", 1);
                    }
                    
                    if (this.RetryCount.HasValue)
                    {
                        retryCode.AddLine($".SetRetryLimit({this.RetryCount.Value})", 1);
                    }

                    retryCode
                        .AppendLine(";")
                        .Return("retry");

                    retryMethod.Body(retryCode);
                }
                else
                {
                    retryMethod.Body(c => c.Return("null"));
                }

                // Generate 'GetRequestUri' method.
                var requestUriMethod = methodSubClassBuilder.Method("GetRequestUri")
                    .Public()
                    .Returns<string>();

                if (string.IsNullOrWhiteSpace(this.RequestUri) == false)
                {
                    requestUriMethod.Body(c => c
                        .Variable("var", "baseUrl", "this.context.Options.BaseUrl")
                        .If("baseUrl != null", c => c
                            .Return($"baseUrl.TrimEnd('/') + \"/{this.RequestUri}\""))
                        .BlankLine()
                        .AddLine($"return \"{this.RequestUri}\";"));
                }
                else
                {
                    requestUriMethod.Body(c => c.Return("null"));
                }

                // Generate 'ExecuteAsync()' method.
                var contentParameter = contentParameterSymbol != null ? $"{contentParameterSymbol.Name}, " : string.Empty;

                var executeMethod = methodSubClassBuilder.Method("ExecuteAsync")
                    .Public()
                    .Async()
                    .Returns(this.MethodMember.ReturnType.ToString())
                    .Params(
                        pb =>
                        {
                            var hasCancellationToken = false;
                            foreach (var param in this.MethodMember.Parameters)
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
                    .Body(c => c
                        .Variable<HttpResponseMessage>("response")
                        .Variable("var", "retry", "this.CreateRetry()")
                        .If("retry != null", m => m
                            .Assign("response", $"await retry.ExecuteAsync(() => {{ return this.SendAsync({contentParameter}cancellationToken); }}).ConfigureAwait(false);"))
                        .Else(m => m
                            .Assign("response", $"await this.SendAsync({contentParameter}cancellationToken).ConfigureAwait(false)"))
                        .BlankLine()
                        .Return("await this.ProcessResponseAsync(response)"));

                // Generate the 'ProcessResponseAsync()' method.
                var processResponseMethod = methodSubClassBuilder.Method("ProcessResponseAsync")
                    .Private()
                    .Async()
                    .Returns(this.MethodMember.ReturnType.ToString())
                    .Param<HttpResponseMessage>("response");

                if (this.ResponseProcessorType == null)
                {
                    processResponseMethod.Body(c => c
                        .AddLine("response.EnsureSuccessStatusCode();")
                        .AddLine("await Task.Yield();")
                        .Return("null"));
                }
                else
                {
                    processResponseMethod
                        .Async()
                        .Body(c => c
                            .Variable("var", "responseProcessor", $"new {this.ResponseProcessorType}()")
                            .BlankLine()
                            .Variable("var", "result", "await responseProcessor.ProcessResponseAsync(response)")
                            .Return("result"));
                }

            });

        var parametersStr = string.Join(", ", this.MethodMember.Parameters.Select(p => p.Name));

        this.ClassBuilder.Method(
            this.MethodMember.Name,
            m => m
                .Public()
                .Returns(this.MethodMember.ReturnType.ToString())
                .Params(
                    pb =>
                    {
                        foreach (var param in this.MethodMember.Parameters)
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
}
