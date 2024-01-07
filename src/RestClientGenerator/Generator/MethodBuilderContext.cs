namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

internal class MethodBuilderContext
{
    private readonly static HttpMethod Patch = new HttpMethod("Patch");

    /// <summary>
    /// A dictionary of query strings.
    /// </summary>
    private Dictionary<string, object> queryStrings;

    /// <summary>
    /// A dictionary of headers for this request.
    /// </summary>
    private Dictionary<string, string> headers;

    /// <summary>
    /// A list of form url properties for this request.
    /// </summary>
    private List<KeyValuePair<string, string>> formUrlProperties;

    /// <summary>
    /// Gets or sets the class builder context.
    /// </summary>
    public ClassBuilderContext ClassBuilderContext { get; set; }

    /// <summary>
    /// Gets or sets the class builder.
    /// </summary>
    public FluentClassBuilder ClassBuilder { get; set; }

    /// <summary>
    /// Gets or sets the member symbol.
    /// </summary>
    public ISymbol Member {  get; set; }

    /// <summary>
    /// Gets or sets the method member symbol.
    /// </summary>
    public IMethodSymbol MethodMember { get; set; }

    /// <summary>
    /// Gets or sets methods route.
    /// </summary>
    public string Route { get; set; }

    /// <summary>
    /// Gets or sets the methods content type.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// Gets or sets the request uri.
    /// </summary>
    public string RequestUri { get; set; }

    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    public HttpMethod RequestMethod { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the method should use retry.
    /// </summary>
    public bool HasRetry { get; set; }

    /// <summary>
    /// Gets or sets the a value indicating whether or not the retry wait time should double on each retry.
    /// </summary>
    public bool? DoubleOnRetry { get; set; }

    /// <summary>
    /// Gets or sets the retry limit.
    /// </summary>
    public int? RetryLimit { get; set; }

    /// <summary>
    /// Gets or sets the response processor type for this request.
    /// </summary>
    public string ResponseProcessorType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the request should add authorization.
    /// </summary>
    public bool HasAuthorization { get; set; }

    /// <summary>
    /// Gets or sets the authorization header value.
    /// </summary>
    public string AuthorizationHeaderValue { get; set; }

    /// <summary>
    /// Gets or sets the the authorization factory type.
    /// </summary>
    public string AuthorizationFactoryType { get; set; }

    /// <summary>
    /// Gets or sets the content parameter.
    /// </summary>
    public IParameterSymbol ContentParameter { get; set; }

    /// <summary>
    /// Gets or sets the methods return type symbol.
    /// </summary>
    public INamedTypeSymbol ReturnType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that the method returns a <see cref="Task"/> type.
    /// </summary>
    public bool ReturnsTask { get; set; }

    /// <summary>
    /// Processes the method level attributes.
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void ProcessMethodAttributes()
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
                        nameof(PatchAttribute) => Patch,
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
                        this.RetryLimit = retryCount;
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
                var firstArg = attr.ConstructorArguments.First();
                var secondArg = attr.ConstructorArguments.Skip(1).First();
                this.AddFormUrlProperty(firstArg, secondArg);
            }
        }
    }

    public void ProcessParameters()
    {
        foreach (var parameterSymbol in this.MethodMember.Parameters)
        {
            var attrs = parameterSymbol.GetAttributes();
            foreach (var attr in attrs)
            {
                if (attr.AttributeClass.Name == nameof(SendAsContentAttribute))
                {
                    this.ContentParameter = parameterSymbol;
                    attr.NamedArguments.GetNamedArguments(
                        new (string, string, Action<object>)[]
                        {
                            (nameof(SendAsContentAttribute.ContentType), nameof(String), (o) => this.ContentType = (string)o),
                        });
                }
                else if (attr.AttributeClass.Name == nameof(SendAsContentDispositionAttribute))
                {
                }
                else if (attr.AttributeClass.Name == nameof(SendAsFormUrlAttribute))
                {
                    var key = attr.NamedArguments.GetNamedArgument<String>(nameof(SendAsFormUrlAttribute.Name));
                    var value = $"{{{parameterSymbol.Name}}}";
                    this.AddFormUrlProperty(key, value);
                }
                else if (attr.AttributeClass.Name == nameof(SendAsHeaderAttribute))
                {
                    var key = attr.ConstructorArguments.First().Value.ToString();
                    var format = attr.NamedArguments.GetNamedArgument<String>(nameof(SendAsHeaderAttribute.Format));

                    var value = $"$\"{{{parameterSymbol.Name}}}\"";
                    if (format != null)
                    {
                        value = $"string.Format(\"{format}\", $\"{{{parameterSymbol.Name}}}\")";
                    }

                    this.AddHeader(key, value);
                }
                else if (attr.AttributeClass.Name == nameof(SendAsQueryAttribute))
                {
                    var key = attr.ConstructorArguments.First().Value.ToString();
                    var format = attr.NamedArguments.GetNamedArgument<String>(nameof(SendAsQueryAttribute.Format));
                    ////var encoding = attr.NamedArguments.GetNamedArgument<String>(nameof(SendAsQueryAttribute.Encoding));
                    var base64 = attr.NamedArguments.GetNamedArgument<bool>(nameof(SendAsQueryAttribute.Base64));
                    var typeName = attr.NamedArguments.GetNamedArgument<String>(nameof(SendAsQueryAttribute.SerializerType));

                    var value = $"{{{parameterSymbol.Name}}}";
                    if (format != null)
                    {
                        value = $"string.Format(\\\"{format.Replace("{", "{{").Replace("}", "}}")}\\\", \\\"{{{parameterSymbol.Name}}}\\\")";
                    }

                    this.AddQueryString(key, value);
                }
            }
        }
    }

    /// <summary>
    /// Generates the method.
    /// </summary>
    public void Generate()
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

                // Generate the 'SendAsync' method
                this.GenerateSend(methodSubClassBuilder);

                // Generate 'CreateRequest' method.
                this.GenerateCreateRequest(methodSubClassBuilder, contentParameterSymbol);

                // Generate 'CreateRetry' method.
                this.GenerateCreateRetry(methodSubClassBuilder);

                // Generate 'GetRequestUri' method.
                this.GenerateGetRequestUri(methodSubClassBuilder);

                // Generate 'ExecuteAsync()' method.
                this.GenerateExecute(methodSubClassBuilder, contentParameterSymbol);

                // Generate the 'ProcessResponseAsync()' method.
                this.GenerateProcessResponse(methodSubClassBuilder);
            });

        var parametersStr = this.MethodMember.BuildParametersList();

        this.ClassBuilder.Method(
            this.MethodMember.Name,
            m => m
                .Public()
                .Returns(this.MethodMember.ReturnType.ToString())
                .Params(p => this.AddParameters(p))
                .Body(c => c
                    .Variable("var", "request", $"new {memberClassName}(this.context)")
                    .AddLine($"Console.WriteLine(request.GetRequestUri({parametersStr}));")
                    .ReturnIf(
                        this.ReturnsTask,
                        $"request.ExecuteAsync({parametersStr})",
                        $"request.Execute({parametersStr})")));
    }

    private void AddFormUrlProperty(TypedConstant key, string value)
    {
        this.AddFormUrlProperty(
            key.IsNull ? null : key.Value.ToString(),
            value);
    }

    private void AddFormUrlProperty(TypedConstant key, TypedConstant value)
    {
        this.AddFormUrlProperty(
            key.IsNull ? null : key.Value.ToString(),
            value.IsNull ? null : value.Value.ToString());
    }

    private void AddFormUrlProperty(string key, string value)
    {
        this.formUrlProperties ??= new List<KeyValuePair<string, string>>();
        this.formUrlProperties.Add(new KeyValuePair<string, string>(key, value));
    }

    /// <summary>
    /// Adds a header.
    /// </summary>
    /// <param name="key">The header key.</param>
    /// <param name="value">The header value.</param>
    /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
    private void AddHeader(string key, string value)
    {
        this.headers ??= new Dictionary<string, string>();
        if (this.headers.TryGetValue(key, out string currentValue) == true)
        {
            this.headers[key] = currentValue += $", {value}";
        }
        else
        {
            this.headers.Add(key, value);
        }
    }

    /// <summary>
    /// Adds a query string.
    /// </summary>
    /// <param name="key">The query key.</param>
    /// <param name="value">The query value.</param>
    /// <returns>The <see cref="HttpRequestBuilder"/> instance.</returns>
    private void AddQueryString(string key, string value)
    {
        this.queryStrings = this.queryStrings ?? new Dictionary<string, object>();

        if (this.queryStrings.TryGetValue(key, out object existingValue) == true)
        {
            if (existingValue is string existingString)
            {
                var list = new List<string>();
                list.Add(existingString);
                list.Add(value);

                this.queryStrings[key] = list;
            }
            else if (existingValue is List<string> existingList)
            {
                existingList.Add(value);
            }
        }
        else
        {
            this.queryStrings.Add(key, value);
        }
    }

    private FluentMethodBuilder GenerateSend(
        FluentClassBuilder builder)
    {
        var parametersStr = this.MethodMember.BuildParametersList();

        var sendMethod = builder.Method("SendAsync")
            .Private()
            .Async()
            .Params(p => AddParameters(p, true))
            .Returns("System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>")
            .Body(c => c
                .UsingBlock($"var request = this.CreateRequest({parametersStr})", b => b
                    .Variable("var", "httpClient", "this.context.GetHttpClient()")
                    .Return("await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false)")));


        return sendMethod;
    }

    private FluentMethodBuilder GenerateCreateRequest(
        FluentClassBuilder builder,
        IParameterSymbol contentParameterSymbol)
    {
        void AddAuthorization(FluentCodeBuilder code)
        {
            if (this.HasAuthorization)
            {
                if (this.AuthorizationHeaderValue != null)
                {
                    code.Assign("auth", $"$\"{this.AuthorizationHeaderValue}\"")
                        .AddLine($"request.Headers.Add(\"Authorization\", auth);");
                }
                else if (this.AuthorizationFactoryType != null)
                {

                }
                else
                {
                    var authMethod = builder
                        .Method("GetAuthorizationAsync")
                        .Async()
                        .Public()
                        .Returns<Task<string>>()
                        .Body(c => c
                            .Variable<string>("scheme", "null")
                            .Variable<string>("token", "null")
                            .Variable("RestClientOptions", "options", "this.context.options")
                            .BlankLine());

                    code.Assign("auth", "await GetAuthorizationAsync();")
                        .AddLine("request.Headers.Add(\"Authorization\", auth);");
                }
                    
                code.AddLine("Console.WriteLine($\"{auth}\");");
            }
        }

        var parametersStr = this.MethodMember.BuildParametersList();

        var createRequestMethod = builder.Method("CreateRequest")
            .Private()
            .Params(p => this.AddParameters(p))
            .Returns<HttpRequestMessage>()
            .Body(c => c.AddLine("throw new NotSupportedException();"));

        var code = new FluentCodeBuilder()
            .Variable("string", "auth", "null")
            .Variable("var", "requestUri", $"this.GetRequestUri({parametersStr})");

        if (this.RequestMethod == HttpMethod.Get)
        {
            this.AddHeader("Accept", $"\"{this.ContentType}\"");

            code
                .Variable("var", "request", $"new HttpRequestMessage(HttpMethod.Get, requestUri)")
                .AddHeaders("request", this.headers);

            AddAuthorization(code);

            code
                .Return("request");
        }
        else if (this.RequestMethod == HttpMethod.Post)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Post, requestUri)")
                .AddHeaders("request", this.headers);

            if (this.formUrlProperties != null)
            {
                code
                    .AddLine("var list = new List<KeyValuePair<string, string>>()")
                    .AddLine("{");

                foreach (var kvp in this.formUrlProperties)
                {
                    code
                        .AddLine($"    new KeyValuePair<string, string>(\"{kvp.Key}\", $\"{kvp.Value}\"),");
                }

                code
                    .AddLine("};")
                    .BlankLine()
                    .AddLine($"request.Content = new FormUrlEncodedContent(list);");
            }
            else if (contentParameterSymbol != null)
            {
                code
                    .Variable("var", "json", $"this.context.Serialize({contentParameterSymbol.Name})")
                    .AddLine($"request.Content = new StringContent(json, System.Text.UTF8Encoding.UTF8, \"{this.ContentType}\");");
            }

            AddAuthorization(code);
            code.Return("request");
        }
        else if (this.RequestMethod == HttpMethod.Put)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Put, requestUri)")
                .AddHeaders("request", this.headers);

            AddAuthorization(code);
            code.Return("request");
        }
        else if (this.RequestMethod == Patch)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(new HttpMethod(\"Patch\"), requestUri)")
                .AddHeaders("request", this.headers);

            AddAuthorization(code);
            code.Return("request");
        }
        else if (this.RequestMethod == HttpMethod.Delete)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Delete, requestUri)")
                .AddHeaders("request", this.headers);

            AddAuthorization(code);
            code.Return("request");
        }

        return createRequestMethod.Body(code);
    }

    private FluentMethodBuilder GenerateCreateRetry(FluentClassBuilder builder)
    {
        var retryMethod = builder.Method("CreateRetry")
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

            if (this.RetryLimit.HasValue)
            {
                retryCode.AddLine($".SetRetryLimit({this.RetryLimit.Value})", 1);
            }

            retryCode
                .AppendLine(";")
                .Return("retry");

            return retryMethod.Body(retryCode);
        }

        return retryMethod
            .Body(c => c
                .Return("null"));
    }

    private FluentMethodBuilder GenerateGetRequestUri(FluentClassBuilder builder)
    {
        var requestUriMethod = builder.Method("GetRequestUri")
            .Public()
            .Returns<string>()
            .Params(p => AddParameters(p));

        if (string.IsNullOrWhiteSpace(this.RequestUri) == false)
        {
            requestUriMethod.Body(c => c
                .Variable("var", "baseUrl", "this.context.Options.BaseUrl")
                .AddQueryStrings("queryString", this.queryStrings)
                .Variable("var", "url", $"$\"{this.RequestUri}\"")
                .Variable("var", "fullUrl", "$\"{url}\" + queryString")
                .BlankLine()
                .If("baseUrl != null", c => c
                    .Return("baseUrl.TrimEnd('/') + \"/\" + fullUrl"))
                .BlankLine()
                .Return("fullUrl"));
        }
        else
        {
            requestUriMethod.Body(c => c.Return("null"));
        }

        return requestUriMethod;
    }

    private FluentMethodBuilder GenerateExecute(
        FluentClassBuilder builder,
        IParameterSymbol contentParameterSymbol)
    {
        var parametersStr = this.MethodMember.BuildParametersList(true);
        var contentParameter = contentParameterSymbol != null ? $"{contentParameterSymbol.Name}, " : string.Empty;

        var executeMethod = builder
            .MethodIf(
                this.ReturnsTask,
                "ExecuteAsync",
                "Execute",
                action: m => m.Async())
            .Public()
            .Returns(this.MethodMember.ReturnType.ToString())
            .Params(p => this.AddParameters(p, true));

        var code = new FluentCodeBuilder()
            .Variable<HttpResponseMessage>("response")
            .Variable("var", "retry", "this.CreateRetry()")
            .If("retry != null", m => m
                .AssignIf(
                    this.ReturnsTask,
                    "response",
                    $"await retry.ExecuteAsync(() => {{ return this.SendAsync({parametersStr}); }}).ConfigureAwait(false)",
                    $"retry.ExecuteAsync(() => {{ return this.SendAsync({parametersStr}); }}).Result"))
            .Else(m => m
                .AssignIf(
                    this.ReturnsTask,
                    "response",
                    $"await this.SendAsync({parametersStr}).ConfigureAwait(false)",
                    $"this.SendAsync({parametersStr}).Result"))
            .BlankLine()
            .ReturnIf(
                this.ReturnsTask,
                "await this.ProcessResponseAsync(response).ConfigureAwait(false)",
                "this.ProcessResponse(response)");

        executeMethod.Body(code);
        return executeMethod;
    }

    private FluentMethodBuilder GenerateProcessResponse(FluentClassBuilder builder)
    {
        FluentMethodBuilder processResponseMethod;
        if (this.ReturnsTask == true)
        {
            processResponseMethod = builder
                .Method("ProcessResponseAsync")
                .Async();
        }
        else
        {
            processResponseMethod = builder
                .Method("ProcessResponse");
        }

        processResponseMethod
            .Private()
            .Returns(this.MethodMember.ReturnType.ToString())
            .Param<HttpResponseMessage>("response");

        var returnTypeName = this.ReturnType.GetTypeOrInnerTypeSymbol().ToString();
        var code = new FluentCodeBuilder();

        if (this.ResponseProcessorType == null)
        {
            code.Variable(returnTypeName, "result", "null")
                .AddLine("response.EnsureSuccessStatusCode();")
                .AddLineIf(this.ReturnsTask, "await Task.Yield();");

            if (returnTypeName == nameof(HttpResponseMessage))
            {
                code.Assign("result", "response");
            }
            else
            {
                code.Variable("string", "content", "null");
                if (this.ReturnsTask)
                {
                    code.Assign("content", "await response.Content.ReadAsStringAsync()");
                }
                else
                {
                    code.Assign("content", "response.Content.ReadAsStringAsync().Result");
                }

                if (returnTypeName == nameof(String))
                {
                    code.Assign("result", "response");
                }
                else
                {

                }
            }

            code.Return("result");
        }
        else
        {
            code.Variable("var", "responseProcessor", $"new {this.ResponseProcessorType}()")
                .BlankLine()
                .Variable("var", "result", "await responseProcessor.ProcessResponseAsync(response)")
                .Return("result");
        }

        processResponseMethod.Body(code);

        return processResponseMethod;
    }

    private void AddParameters(
        FluentParametersBuilder p,
        bool addCancellatiomToken = false)
    {
        this.AddParameters(p, this.MethodMember.Parameters, addCancellatiomToken);
    }

    private void AddParameters(
        FluentParametersBuilder p,
        ImmutableArray<IParameterSymbol> parameters,
        bool addCancellatiomToken = false)
    {
        var hasCancellationToken = false;
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var typeName = param.Type.OriginalDefinition.ToString();
                p.Param(param.Name, typeName);
                if (typeName == "System.Threading.CancellationToken")
                {
                    hasCancellationToken = true;
                }
            }
        }

        if (hasCancellationToken == false &&
            addCancellatiomToken == true)
        {
            p.Param<CancellationToken>("cancellationToken", p => p.Default("default"));
        }
    }
}
