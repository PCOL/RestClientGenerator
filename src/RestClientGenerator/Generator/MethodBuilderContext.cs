namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

/// <summary>
/// A method builder context.
/// </summary>
internal class MethodBuilderContext
{
    /// <summary>
    /// A dictionary of class counts.
    /// </summary>
    private static Dictionary<string, int> classCounts = new Dictionary<string, int>();

    /// <summary>
    /// A patch http method.
    /// </summary>
    private readonly static HttpMethod Patch = new HttpMethod("PATCH");

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
    /// The request action.
    /// </summary>
    private string requestAction;

    /// <summary>
    /// The request async func.
    /// </summary>
    private string requestAsyncFunc;

    /// <summary>
    /// The response action.
    /// </summary>
    private string responsetAction;

    /// <summary>
    /// The response async func.
    /// </summary>
    private string responseAsyncFunc;

    /// <summary>
    /// The uri the request should use.
    /// </summary>
    private string uriParameter;

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
    public string RequestUri { get; private set; }

    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    public HttpMethod RequestMethod { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not the method should use retry.
    /// </summary>
    public bool HasRetry { get; private set; }

    /// <summary>
    /// Gets or sets the retry limit.
    /// </summary>
    public int? RetryLimit { get; private set; }

    /// <summary>
    /// Gets or sets the a value indicating whether or not the retry wait time should double on each retry.
    /// </summary>
    public bool? DoubleOnRetry { get; private set; }

    /// <summary>
    /// Gets or sets the retry wait time.
    /// </summary>
    public int? RetryWaitTime { get; private set; }

    /// <summary>
    /// Gets or sets the retry maximum wait time.
    /// </summary>
    public int? RetryMaxWaitTime { get; private set; }

    /// <summary>
    /// Gets or sets the retry status codes.
    /// </summary>
    public string[] RetryHttpStatusCodes { get; private set; }

    /// <summary>
    /// Gets or sets the retry exception types.
    /// </summary>
    public string[] RetryExceptionTypes { get; private set; }

    /// <summary>
    /// Gets or sets the response processor type for this request.
    /// </summary>
    public string ResponseProcessorType { get; private set; }

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
    /// Gets or sets the content parameter name.
    /// </summary>
    public string ContentParameterName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets or sets the content parameter.
    /// </summary>
    public IParameterSymbol ContentParameterSymbol { get; private set; }

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
                    if (arg.GetValue<int>(nameof(RetryAttribute.RetryLimit), nameof(Int32), out var retryCount))
                    {
                        this.RetryLimit = retryCount;
                    }
                    else if (arg.GetValue<bool>(nameof(RetryAttribute.DoubleWaitTimeOnRetry), nameof(Boolean), out var doubleOnRetry))
                    {
                        this.DoubleOnRetry = doubleOnRetry;
                    }
                    else if (arg.GetValue<int>(nameof(RetryAttribute.WaitTime), nameof(Int32), out var waitTime))
                    {
                        this.RetryWaitTime = waitTime;
                    }
                    else if (arg.GetValue<int>(nameof(RetryAttribute.MaxWaitTime), nameof(Int32), out var maxWaitTime))
                    {
                        this.RetryMaxWaitTime = maxWaitTime;
                    }
                    else if (arg.GetValue<string[]>(nameof(RetryAttribute.HttpStatusCodesToRetry), nameof(HttpStatusCode), out var statusCodes))
                    {
                        this.RetryHttpStatusCodes = statusCodes;
                    }
                    else if (arg.GetValue<string[]>(nameof(RetryAttribute.ExceptionTypesToRetry), nameof(String), out var exceptionTypes))
                    {
                        this.RetryExceptionTypes = exceptionTypes;
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

    /// <summary>
    /// Processes the methods parameters.
    /// </summary>
    public void ProcessParameters()
    {
        foreach (var parameterSymbol in this.MethodMember.Parameters)
        {
            var attrs = parameterSymbol.GetAttributes();
            foreach (var attr in attrs)
            {
                if (attr.AttributeClass.Name == nameof(SendAsContentAttribute))
                {
                    this.ContentParameterSymbol = parameterSymbol;
                    this.ContentParameterName = parameterSymbol.Name;

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
                else if (attr.AttributeClass.Name == nameof(UriAttribute))
                {
                    this.uriParameter = parameterSymbol.Name;
                }
            }

            if (parameterSymbol.Type.ToString() == "System.Action<System.Net.Http.HttpRequestMessage>")
            {
                this.requestAction = parameterSymbol.Name;
            }
            else if (parameterSymbol.Type.ToString() == "System.Func<System.Net.Http.HttpRequestMessage, System.Threading.Tasks.Task>")
            {
                this.requestAsyncFunc = parameterSymbol.Name;
            }
            else if (parameterSymbol.Type.ToString() == "System.Action<System.Net.Http.HttpResponseMessage>")
            {
                this.responsetAction = parameterSymbol.Name;
            }
            else if (parameterSymbol.Type.ToString() == "System.Func<System.Net.Http.HttpResponseMessage, System.Threading.Tasks.Task>")
            {
                this.responseAsyncFunc = parameterSymbol.Name;
            }
        }
    }

    /// <summary>
    /// Generates the method.
    /// </summary>
    public void Generate()
    {
        var memberClassName = $"{GetNextClassName(this.MethodMember.Name)}_class";
        this.ClassBuilder.AddStruct(
            memberClassName,
            methodSubClassBuilder =>
            {
                // Add 'context' field and constructor
                methodSubClassBuilder
                    .Private()
                    .ReadOnly()
                    .Field<RestClientContext>("__context", f => f.ReadOnly())
                    .Constructor(m => m
                        .Public()
                        .Param<RestClientContext>("context")
                        .Body(c => c.Assign("this.__context", "context")));

                // Generate the 'SendAsync' method
                this.GenerateSend(methodSubClassBuilder);

                // Generate 'CreateRequest' method.
                this.GenerateCreateRequest(methodSubClassBuilder);

                // Generate 'CreateRetry' method.
                this.GenerateCreateRetry(methodSubClassBuilder);

                // Generate 'GetRequestUri' method.
                this.GenerateGetRequestUri(methodSubClassBuilder);

                // Generate 'ExecuteAsync()' method.
                this.GenerateExecute(methodSubClassBuilder);

                // Generate the 'ProcessResponseAsync()' method.
                this.GenerateProcessResponse(methodSubClassBuilder);

                // Debugging
                if (this.MethodMember.GetAttributes().Any(a => a.AttributeClass.Name == nameof(OutputCodeAttribute)))
                {
                    Console.WriteLine(methodSubClassBuilder.Build());
                }
            });

        var parametersStr = this.MethodMember.BuildParametersList();

        this.ClassBuilder.Method(
            this.MethodMember.Name,
            m => m
                .Public()
                .Returns(this.MethodMember.ReturnType.ToString())
                .Params(p => this.AddParameters(p))
                .Body(c => c
                    .Variable("var", "request", $"new {memberClassName}(this.__context)")
                    .ReturnIf(
                        this.ReturnsTask,
                        $"request.ExecuteAsync({parametersStr})",
                        $"request.Execute({parametersStr})")));
    }

    /// <summary>
    /// Gets the next class name.
    /// </summary>
    /// <param name="name">The base class name.</param>
    /// <returns>The class name.</returns>
    private string GetNextClassName(string name)
    {
        if (classCounts.TryGetValue(name, out var count) == false)
        {
            count = 0;
        }

        classCounts[name] = ++count;

        return $"{name}_{count}";
    }

    /// <summary>
    /// Adds a form url property.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    private void AddFormUrlProperty(TypedConstant key, string value)
    {
        this.AddFormUrlProperty(
            key.IsNull ? null : key.Value.ToString(),
            value);
    }

    /// <summary>
    /// Adds a form url property.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    private void AddFormUrlProperty(TypedConstant key, TypedConstant value)
    {
        this.AddFormUrlProperty(
            key.IsNull ? null : key.Value.ToString(),
            value.IsNull ? null : value.Value.ToString());
    }

    /// <summary>
    /// Adds a form url property.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
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

    /// <summary>
    /// Generates the 'SendAsync' method.
    /// </summary>
    /// <param name="builder">A class builder.</param>
    /// <returns>The method builder.</returns>
    private FluentMethodBuilder GenerateSend(
        FluentStructBuilder builder)
    {
        var parametersStr = this.MethodMember.BuildParametersList();

        var codeBlock = new FluentCodeBuilder()
            .Variable("var", "__httpClient", "this.__context.GetHttpClient()");

        if (this.requestAction != null)
        {
            codeBlock.AddLine($"{this.requestAction}(__request);");
        }
        else if (this.requestAsyncFunc != null)
        {
            codeBlock.AddLine($"await {this.requestAsyncFunc}(__request).ConfigureAwait(false);");
        }

        codeBlock
            .Variable("var", "__response", "await __httpClient.SendAsync(__request, cancellationToken).ConfigureAwait(false)");

        if (this.responsetAction != null)
        {
            codeBlock.AddLine($"{this.responsetAction}(__response);");
        }
        else if (this.responseAsyncFunc != null)
        {
            codeBlock.AddLine($"await {this.responseAsyncFunc}(__response).ConfigureAwait(false);");
        }

        codeBlock
            .Return("__response");

        var sendMethod = builder.Method("SendAsync")
            .Private()
            .Async()
            .Params(p => AddParameters(p, true))
            .Returns("System.Threading.Tasks.Task<System.Net.Http.HttpResponseMessage>")
            .Body(c => c
                .Variable("HttpRequestMessage", "__request", "null")
                .AssignIf(
                    this.ReturnsTask,
                    "__request",
                    $"await this.CreateRequestAsync({parametersStr})",
                    $"this.CreateRequest({parametersStr})")
                .UsingBlock("__request", codeBlock));

        return sendMethod;
    }

    /// <summary>
    /// Generates the 'CreateRequest' method.
    /// </summary>
    /// <param name="builder">A class builder.</param>
    /// <returns>A method builder.</returns>
    /// <exception cref="NotSupportedException"></exception>
    private FluentMethodBuilder GenerateCreateRequest(
        FluentStructBuilder builder)
    {
        bool addedAuth = false;
        void AddAuthorization(FluentCodeBuilder code)
        {
            if (this.HasAuthorization)
            {
                if (this.AuthorizationHeaderValue != null)
                {
                    code.Assign("auth", $"$\"{this.AuthorizationHeaderValue}\"")
                        .AddLine($"request.Headers.Add(\"Authorization\", auth);");
                }
                else
                {
                    addedAuth = true;
                    var authCode = new FluentCodeBuilder()
                            .Variable<string>("value", "null")
                            .Variable("var", "options", "this.__context.Options")
                            .BlankLine();

                    if (this.AuthorizationFactoryType != null)
                    {
                        authCode
                            .Variable("var", "authFactory", $"new {this.AuthorizationFactoryType}()");
                    }
                    else
                    {
                        authCode
                            .Variable("var", "authFactory", $"options.GetAuthorizationHeaderFactory()");
                    }

                    authCode
                        .AssignIf(
                            this.ReturnsTask,
                            "value",
                            $"await authFactory.{nameof(IAuthorizationHeaderFactory.GetAuthorizationHeaderValueAsync)}()",
                            $"authFactory.{nameof(IAuthorizationHeaderFactory.GetAuthorizationHeaderValue)}()")
                        .Return("value");

                    var authMethod = builder
                        .MethodIf(
                            this.ReturnsTask,
                            "GetAuthorizationAsync",
                            "GetAuthorization",
                            m => m.Async()
                                .Returns("Task<string>"),
                            m => m.Returns("string"))
                        .Public()
                        .Body(authCode);

                    code.Assign("auth", "await GetAuthorizationAsync();")
                        .AddLine("request.Headers.Add(\"Authorization\", auth);");
                }
            }
        }

        void AddContent(FluentCodeBuilder code)
        {
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
            else if (this.ContentParameterSymbol != null)
            {
                if (this.ContentParameterSymbol.Type.HasBaseType(nameof(HttpContent)))
                {
                    code.AddLine($"request.Content = {this.ContentParameterName};");
                }
                else if (this.ContentParameterSymbol.Type.HasBaseType("System.IO", nameof(Stream)))
                {
                    code.AddLine($"request.Content = new System.Net.Http.StreamContent({this.ContentParameterName});");
                }
                else if (this.ContentParameterSymbol.Type.ContainingNamespace.Name == "System" &&
                    this.ContentParameterSymbol.Type.Name == "Func" &&
                    ((INamedTypeSymbol)this.ContentParameterSymbol.Type).Arity == 1)
                {
                    var returnType = ((INamedTypeSymbol)this.ContentParameterSymbol.Type).TypeArguments.Last();

                    if (returnType.HasBaseType(nameof(HttpClient)))
                    {
                        code
                            .Variable($"{returnType.ToDisplayString()}", "__result", $"{this.ContentParameterName}()")
                            .AddLine($"request.Content = __result;");
                    }
                    else if (returnType.HasBaseType("System.IO", nameof(Stream)))
                    {
                        code
                            .Variable($"{returnType.ToDisplayString()}", "__result", $"{this.ContentParameterName}()")
                            .AddLine($"request.Content = new System.Net.Http.StreamContent(__result);");
                    }
                    else
                    {
                        // Error
                    }
                }
                else
                {
                    code.Variable("var", "json", $"this.__context.Serialize({this.ContentParameterName})")
                        .AddLine($"request.Content = new StringContent(json, System.Text.UTF8Encoding.UTF8, \"{this.ContentType}\");");
                }
            }
        }

        var parametersStr = this.MethodMember.BuildParametersList();

        var code = new FluentCodeBuilder()
            .Variable("string", "auth", "null")
            .Variable("var", "requestUri", $"this.GetRequestUri({parametersStr})");

        if (this.RequestMethod == HttpMethod.Get)
        {
            this.AddHeader("Accept", $"\"{this.ContentType}\"");

            code
                .Variable("var", "request", $"new HttpRequestMessage(HttpMethod.Get, requestUri)")
                .AddHeaders("request", this.headers);
        }
        else if (this.RequestMethod == HttpMethod.Post)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Post, requestUri)")
                .AddHeaders("request", this.headers);

            AddContent(code);
        }
        else if (this.RequestMethod == HttpMethod.Put)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Put, requestUri)")
                .AddHeaders("request", this.headers);

            AddContent(code);
        }
        else if (this.RequestMethod == Patch)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(new HttpMethod(\"PATCH\"), requestUri)")
                .AddHeaders("request", this.headers);

            AddContent(code);
        }
        else if (this.RequestMethod == HttpMethod.Delete)
        {
            code
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Delete, requestUri)")
                .AddHeaders("request", this.headers);
        }
        else
        {
            throw new NotSupportedException($"Request Method '{this.RequestMethod}' not supported");
        }

        AddAuthorization(code);

        var createRequestMethod = builder
            .MethodIf(
                this.ReturnsTask,
                "CreateRequestAsync",
                "CreateRequest",
                m => m.Returns("Task<HttpRequestMessage>"),
                m => m.Returns<HttpRequestMessage>())
            .Private()
            .Params(p => this.AddParameters(p))
            .Body(code);

        if (this.ReturnsTask &&
            addedAuth == false)
        {
            code.Return("Task.FromResult(request)");
        }
        else
        {
            if (this.ReturnsTask)
            {
                createRequestMethod.Async();
            }

            code.Return("request");
        }

        return createRequestMethod;
    }

    /// <summary>
    /// Generates the 'CreateRetry' method.
    /// </summary>
    /// <param name="builder">A class builder.</param>
    /// <returns>A method builder.</returns>
    private FluentMethodBuilder GenerateCreateRetry(FluentStructBuilder builder)
    {
        var retryMethod = builder.Method("CreateRetry")
            .Private()
            .Returns<IRetry>();

        if (this.HasRetry == true)
        {
            var retryLimit = this.RetryLimit.HasValue ? this.RetryLimit.Value : 3;
            var waitTime = this.RetryWaitTime.HasValue ? this.RetryWaitTime.Value : 250;
            var maxWaitTimeSpanString = this.RetryMaxWaitTime.HasValue ? $"TimeSpan.FromMilliseconds({this.RetryMaxWaitTime.Value})" : "TimeSpan.MaxValue";
            var doubleOnRetry = this.DoubleOnRetry.HasValue ? this.DoubleOnRetry.Value : false;

            var retryCode = new FluentCodeBuilder()
                .Variable("var", "retryFactory", $"this.__context.GetRetryFactory()")
                .Variable(
                    "HttpStatusCode",
                    "statusCodes",
                    this.RetryHttpStatusCodes,
                    (value) => $"(HttpStatusCode){value}")
                .BlankLine()
                .Variable(
                    "Type",
                    "exceptionTypes",
                    this.RetryExceptionTypes,
                    value => $"typeof({value})")
                .BlankLine()
                .Variable("var", "retry", $"retryFactory.CreateRetry({retryLimit}, TimeSpan.FromMilliseconds({waitTime}), {maxWaitTimeSpanString}, {doubleOnRetry.ToString().ToLower()}, statusCodes, exceptionTypes)")
                .Return("retry");

            return retryMethod.Body(retryCode);
        }

        return retryMethod
            .Body(c => c
                .Return("null"));
    }

    /// <summary>
    /// Generates the 'GetRequestUri' method.
    /// </summary>
    /// <param name="builder">A class builder.</param>
    /// <returns>A method builder.</returns>
    private FluentMethodBuilder GenerateGetRequestUri(FluentStructBuilder builder)
    {
        var requestUriMethod = builder.Method("GetRequestUri")
            .Public()
            .Returns<string>()
            .Params(p => AddParameters(p));

        if (this.uriParameter != null)
        {
            requestUriMethod.Body(c => c
                .Return(this.uriParameter));
        }
        else if (string.IsNullOrWhiteSpace(this.RequestUri) == false)
        {
            requestUriMethod.Body(c => c
                .Variable("var", "baseUrl", "this.__context.Options.BaseUrl")
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

    /// <summary>
    /// Generates the 'Execute' method.
    /// </summary>
    /// <param name="builder">A class builder.</param>
    /// <returns>A method builder.</returns>
    private FluentMethodBuilder GenerateExecute(
        FluentStructBuilder builder)
    {
        var parametersStr = this.MethodMember.BuildParametersList(true);

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
                .Variable("var", "self", "this")
                .AssignIf(
                    this.ReturnsTask,
                    "response",
                    $"await retry.ExecuteAsync(() => {{ return self.SendAsync({parametersStr}); }}).ConfigureAwait(false)",
                    $"retry.ExecuteAsync(() => {{ return self.SendAsync({parametersStr}); }}).Result"))
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

    /// <summary>
    /// Generates the 'ProcessResponse' method.
    /// </summary>
    /// <param name="builder">A class builder.</param>
    /// <returns>A method builder for the new method.</returns>
    private FluentMethodBuilder GenerateProcessResponse(FluentStructBuilder builder)
    {
        var processResponseMethod = builder
            .MethodIf(
                this.ReturnsTask,
                "ProcessResponseAsync",
                "ProcessResponse",
                m => m.Async())
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

            if (returnTypeName == $"System.Net.Http.{nameof(HttpResponseMessage)}")
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

    /// <summary>
    /// Adds the methods parameters to a <see cref="FluentParametersBuilder"/> instance.
    /// </summary>
    /// <param name="p">A <see cref="FluentParametersBuilder"/> instance.</param>
    /// <param name="addCancellationToken">A value indicating whether or not the add a cancellation token.</param>
    private void AddParameters(
        FluentParametersBuilder p,
        bool addCancellationToken = false)
    {
        this.AddParameters(p, this.MethodMember.Parameters, addCancellationToken);
    }

    /// <summary>
    /// Adds a list of parameters to a <see cref="FluentParametersBuilder"/> instance.
    /// </summary>
    /// <param name="p">A <see cref="FluentParametersBuilder"/> instance.</param>
    /// <param name="parameters">A list of parameters.</param>
    /// <param name="addCancellationToken">A value indicating whether or not the add a cancellation token.</param>
    private void AddParameters(
        FluentParametersBuilder p,
        ImmutableArray<IParameterSymbol> parameters,
        bool addCancellationToken = false)
    {
        var hasCancellationToken = false;
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                var typeName = param.Type.ToString();
                p.Param(param.Name, typeName);
                if (typeName == "System.Threading.CancellationToken")
                {
                    hasCancellationToken = true;
                }
            }
        }

        if (hasCancellationToken == false &&
            addCancellationToken == true)
        {
            p.Param<CancellationToken>("cancellationToken", p => p.Default("default"));
        }
    }
}
