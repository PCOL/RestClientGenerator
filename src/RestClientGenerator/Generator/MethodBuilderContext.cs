namespace RestClient.Generator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Microsoft.CodeAnalysis;

internal class MethodBuilderContext
{
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

                    var value = $"{{{parameterSymbol.Name}}}";
                    if (format != null)
                    {
                        value = $"string.Format(\\\"{format.Replace("{", "{{").Replace("}", "}}")}\\\", \\\"{{{parameterSymbol.Name}}}\\\")";
                    }

                    this.AddHeader(key, value);
                }
                else if (attr.AttributeClass.Name == nameof(SendAsQueryAttribute))
                {
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
                this.GenerateSendAsync(methodSubClassBuilder);

                // Generate 'CreateRequest' method.
                this.GenerateCreateRequest(methodSubClassBuilder, contentParameterSymbol);

                // Generate 'CreateRetry' method.
                this.GenerateCreateRetry(methodSubClassBuilder);

                // Generate 'GetRequestUri' method.
                this.GenerateGetRequestUri(methodSubClassBuilder);

                // Generate 'ExecuteAsync()' method.
                this.GenerateExecuteAsync(methodSubClassBuilder, contentParameterSymbol);

                // Generate the 'ProcessResponseAsync()' method.
                this.GenerateProcessResponseAsync(methodSubClassBuilder);
            });

        this.ClassBuilder.Method(
            this.MethodMember.Name,
            m => m
                .Public()
                .Returns(this.MethodMember.ReturnType.ToString())
                .Params(p => this.AddParameters(p, this.MethodMember.Parameters))
                .Body(c => c
                    .Variable("var", "request", $"new {memberClassName}(this.context)")
                    .AddLine("Console.WriteLine(request.GetRequestUri());")
                    .Return($"request.ExecuteAsync({this.BuildParametersList()})")));
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

    private FluentMethodBuilder GenerateSendAsync(
        FluentClassBuilder builder)
    {
        var parametersStr = this.BuildParametersList();

        var sendMethod = builder.Method("SendAsync")
            .Private()
            .Async()
            .Params(p => AddParameters(p, this.MethodMember.Parameters, true))
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
                code.AddLine($"request.Headers.Add(\"Authorization\", $\"{this.AuthorizationHeaderValue}\");");
                code.AddLine($"Console.WriteLine($\"{this.AuthorizationHeaderValue}\");");
            }
        }

        var createRequestMethod = builder.Method("CreateRequest")
            .Private()
            .Params(p =>
            {
                foreach (var param in this.MethodMember.Parameters)
                {
                    p.Param(param.Name, param.Type.OriginalDefinition.ToString());
                }
            })
            .Returns<HttpRequestMessage>()
            .Body(c => c.AddLine("throw new NotSupportedException();"));

        var createRequestCode = new FluentCodeBuilder();
        if (this.RequestMethod == HttpMethod.Get)
        {
            this.AddHeader("Accept", this.ContentType);

            createRequestCode
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Get, this.GetRequestUri())")
                .AddHeaders("request", this.headers);

            AddAuthorization(createRequestCode);
            
            createRequestCode
                .Return("request");
        }
        else if (this.RequestMethod == HttpMethod.Post)
        {
            createRequestCode
                .Variable("var", "request", "new HttpRequestMessage(HttpMethod.Post, this.GetRequestUri())")
                .AddHeaders("request", this.headers);

            if (this.formUrlProperties != null)
            {
                createRequestCode
                    .AddLine("var list = new List<KeyValuePair<string, string>>()")
                    .AddLine("{");

                foreach (var kvp in this.formUrlProperties)
                {
                    createRequestCode
                        .AddLine($"    new KeyValuePair<string, string>(\"{kvp.Key}\", $\"{kvp.Value}\"),");
                }

                createRequestCode
                    .AddLine("};")
                    .BlankLine()
                    .AddLine($"request.Content = new FormUrlEncodedContent(list);");
            }
            else if (contentParameterSymbol != null)
            {
                createRequestCode
                    .Variable("var", "json", $"this.context.Serialize({contentParameterSymbol.Name})")
                    .AddLine($"request.Content = new StringContent(json, System.Text.UTF8Encoding.UTF8, \"{this.ContentType}\");");
            }

            AddAuthorization(createRequestCode);
            createRequestCode.Return("request");
        }

        return createRequestMethod.Body(createRequestCode);
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
            .Returns<string>();

        if (string.IsNullOrWhiteSpace(this.RequestUri) == false)
        {
            requestUriMethod.Body(c => c
                .Variable("var", "baseUrl", "this.context.Options.BaseUrl")
                .AddQueryStrings("queryString", this.queryStrings)
                .BlankLine()
                .If("baseUrl != null", c => c
                    .Return($"baseUrl.TrimEnd('/') + \"/{this.RequestUri}\""))
                .BlankLine()
                .AddLine($"return \"{this.RequestUri}\";")); ;
        }
        else
        {
            requestUriMethod.Body(c => c.Return("null"));
        }

        return requestUriMethod;
    }

    private FluentMethodBuilder GenerateExecuteAsync(
        FluentClassBuilder builder,
        IParameterSymbol contentParameterSymbol)
    {
        var parametersStr = this.BuildParametersList(true);
        var contentParameter = contentParameterSymbol != null ? $"{contentParameterSymbol.Name}, " : string.Empty;

        var executeMethod = builder.Method("ExecuteAsync")
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
                    .Assign("response", $"await retry.ExecuteAsync(() => {{ return this.SendAsync({parametersStr}); }}).ConfigureAwait(false);"))
                .Else(m => m
                    .Assign("response", $"await this.SendAsync({parametersStr}).ConfigureAwait(false)"))
                .BlankLine()
                .Return("await this.ProcessResponseAsync(response)"));

        return executeMethod;
    }

    private FluentMethodBuilder GenerateProcessResponseAsync(FluentClassBuilder builder)
    {
        var processResponseMethod = builder.Method("ProcessResponseAsync")
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

        return processResponseMethod;
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

    private string BuildParametersList(bool includeCancellationToken = false)
    {
        var paramsList = this
            .MethodMember
            .Parameters
            .Select(p => p.Name)
            .ToList();

        if (includeCancellationToken)
        {
            if (paramsList.Contains("cancellationToken") == false)
            {
                paramsList.Add("cancellationToken");
            }
        }

        return string.Join(", ", paramsList);
    }
}
