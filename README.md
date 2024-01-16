# RestClientGenerator
A .Net source generator for generating REST clients from interface definitions

## Installation

[RestClientGenerator is available on Nuget](https://www.nuget.org/packages/RestClientGenerator)

## Defining a client

A client is defined by creating an interface and decorating it with the HttpClientContractAttribute class. This allows you to define the base route and content type to be used by default by all methods on the interface. These can be also be set on a per method basis and so do not have to be configured at this point.

A call to a REST endpoint is defined by adding a method to the interface. The method determines the parameters and return type that will be used to send data to and receive data from the REST endpoint.

Certain special parameters are supported to allow interception of the http request and response at points in the call (More on that later).

How the method is translated into the appropriate REST call is determined by the attributes used to decorate the method.

Methods can be defined as synchronous or asynchronous.

```cs
public interface IClient
{
    [Get("api/v1/wiget/{id}")]
    Task<HttpResponseMessage> GetWidgetAsync(string id);
}
```

To access the clients you must create a partial class that inherits from `RestClientContext`, and decorate it with `RestClientAttribute` attributes for each interface definition:

```cs
[RestClient(typeof(IClient))]
public partial class ClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string content)
    {
        return new JsonContext().Deserialize<T>(content);
    }

    public override string Serialize<T>(T obj)
    {
        return new JsonContext().Serialize<T>(obj);
    }
}
```

The clients can then be used in code as normal:

```cs
public class Program
{

    public static async Task Main(string[] args)
    {
        var context = new ClientContext();
        context.Options = new RestClientOptions()
        {
            BaseUrl = "http://localhost",
            HttpClient = new HttpClient()
        };

        var client = context.GetClient();
        var response = await client.GetWidgetAsync("id");

        response.EnsureSuccessCode();
    }
}
```

### Method Parameters and Return Types

#### Return Types

As mentioned earlier methods can be synchronous or asynchronous so supported return types are:

* `void` - No return value.
* `T` - Any type that the content can be deserialised to.
* `HttpResponseMessage` - The full HttpResponseMessage.
* `Task` - A task that upon completion will have no return value.
* `Task<T>` - A task that upon completion will return any type the content can be deserailised to.
* `Task<HttpResponseMessage>` - A task that upon completion will return the full HttpResponseMethod.
* `Task<Stream>` - A task that upon completion will return the content property as a stream.

Return values can also be decorated with attributes to control their behaviour:

FromJsonAttribute can be used to return a property from json response content.

FromModelAttribute can be used to return a property from response content deserialised as a specific model.

#### Parameters

The methods parameters can be used to define content, query parameters, or headers, and attributes are used to determine their use.

* `SendAsContentAttribute` - Specifies that a parameter is to be used as the requests content.

* `SendAsQueryAttribute` - Specifies that a parameter is to be used as a query parameter.

* `SendAsHeaderAttribute` - Specfies that a parameter is to be used as a request header.

* `SendAsFormUrlAttribute` - Specifies that a paremeter is to be used as a form url content property.

```cs
public interface ICustomerClient
{
    [Get("api/customer/update")]
    Task<HttpResponseMessage> UpdateCustomerAsync(
        [SendAsHeader]string id,
        [SendAsContent]CustomerModel customer);
}
```

#### Special Parameters

Special parameter types are also supported:

* A parameter of type `Action<HttpRequestMessage>` or `Func<HttpRequestMessage, Task>` will be called just before the request is sent.

* A parameter of type `Action<HttpResponseMessage>` or `Func<HttpResponseMessage, Task>` will be called just after the response has been received.

* A parameter of type `Func<HttpResponseMessage, *ReturnType*>` will be called just before the method returns. The method will then return the response from the lambda allowing it to decode the response.

```cs
public interface ICustomerClient
{
    [Get("api/customer/{id}")]
    Task<CustomerModel> GetCustomerAsync(
        string id,
        Func<HttpResponseMessage, CustomerMode> responseFunc);
}
```

### Attributes

Attributes are used to define a methods behaviours:

The http method attributes from Microsoft.AspNetCore.Mvc such as [HttpGet], [HttpPut], [HttpPost], and [HttpDelete] can be used to define the requests HttpMethod type.

_RestClientGenerator_ also provides its own set of attributes as well:

* `PostAttribute` - Specifies that the request has a POST method.
* `GetAttribute` - Specifies that the request has a GET method.
* `PutAttribute` - Specifies that the request has a PUT method.
* `PatchAttribute` - Specifies that the request has a PATCH method.
* `DeleteAttribute` - Specifies that the request has a DELETE method.

`AddHeaderAttribute` can be applied to a method or an interface to add a header value to the request or all requests:

* `AddFormUrlEncodedPropertyAttribute` can be used to add a form url property to a request content.

* `AddAuthorizationHeaderAttribute` can be applied to a method or an interface to add an authorization header to the request or all requests.

One group of attributes can be used to alter how return values or out parameters use the Http responses content to obtain their values.

* The `FromResponseAttribute` class can be used to indicate the return type. This is only required if the methods return type or out parameter is an interface type.

* The `FromJsonAttribute` class can be used to indicate that the value is obtained from a JSON object. It can be used to specify a sub type within the JSON object via a path parameter. It is also possible to indicate a return type (as with `FromResponseAttribute`) if the methods return type is an interface.

* The `FromModelAttribute` class can be used to indicate that the value is obtained from a property of a model, rather than the model itself.

### Http Response Processors

For clients that wish to create a more complex response model there is the `HttpResponseProcessor` type. These types allow the response to be intercepted and converted into much more complex results.

```cs
public interface IResult<T>
{
    int StatusCode { get; }
    T Result { get; }
    string ErrorMessage { get; }
}
```

```cs
public class Result<T>
    : IResult<T>
{
    public int StatusCode { get; set; }
    public T Result { get; set; }
    public string ErrorMessage { get; set; }
}
```

```cs
public class ResultProcessor<T>
    : HttpResponseProcessor<IResult<T>>
{
    public override async Task<IResult<T>> ProcessResponseAsync(HttpResponseMessage httpResponse)
    {
        var result = new Result<T>;
        result.StatusCode = (int)httpResponse.StatusCode;
        if (httpResponse.IsSuccessStatusCode == true)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content) == false)
            {
                result.Result = JsonConvert.DeserializeObject<T>(content);
            }
        }
        else
        {
            result.ErrorMessage = "";
        }

        return result;
    }
}
```

```cs
[HttpClientContract(Route = "api/customers", ContentType = "aplication/json")]
public interface ICustomerClient
{
    [Get("")]
    [HttpResponseProcessor(typeof(ResultProcessor<IEnumerable<CustomerModel>>))]
    Task<IResult<IEnumerable<CustomerModel>>> GetCustomersAsync();
}
```

### Example synchronous client

Create an interface and decorate with attributes to define how the client should interact with the service:

```cs
[HttpClientContract(Route = "api/customers", ContentType = "aplication/json")]
public interface ICustomerClient
{
    [Post("")]
    CreateCustomerResponseModel CreateCustomer(CreateCustomerModel customer);

    [Get("")]
    IEnumerable<CustomerModel> GetCustomers();

    [Get("{name}")]
    CustomerModel GetCustomerByName(string name);
}
```

Create a client context class to access the clients:

```cs
[RestClient(typeof(ICustomerClient))]
public partial ClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string content)
    {
        return new JsonContext().Deserialize<T>(content);
    }

    public override string Serialize<T>(T obj)
    {
        return new JsonContext().Serialize<T>(obj);
    }
}
```

Once the interface is defined a proxy can be generated and then used to call the service:

```cs
var clientContext = new ClientContext();
context.Options = new RestClientOptions()
{
    BaseUrl = "http://localhost",
    HttpClient = new HttpClient()
};

var customerClient = context.GetCustomerClient();

var response = customerClient.CreateCustomer(
    new CreateCustomerModel()
    {
        Name = "Customer",
        Address = "Somewhere",
        PhoneNumber = "123456789"
    });
```

### Example asynchronous client

Create an interface and decorate with attributes to define how the client should interact with the service:

```cs
[HttpClientContract(Route = "api/customers", ContentType = "aplication/json")]
public interface ICustomerClient
{
    [Post("")]
    Task<CreateCustomerResponseModel> CreateCustomerAsync(CreateCustomerModel customer);

    [Get("")]
    Task<IEnumerable<CustomerModel>> GetCustomersAsync();

    [Get("{name}")]
    Task<CustomerModel> GetCustomerByName(string name);
}
```

Create a client context class to access the clients:

```cs
[RestClient(typeof(ICustomerClient))]
public partial ClientContext
    : RestClientContext
{
    public override T Deserialize<T>(string content)
    {
        return new JsonContext().Deserialize<T>(content);
    }

    public override string Serialize<T>(T obj)
    {
        return new JsonContext().Serialize<T>(obj);
    }
}
```

Again, once the interface is defined a proxy can be generated and then used to call the service:

```cs
var clientContext = new ClientContext();
context.Options = new RestClientOptions()
{
    BaseUrl = "http://localhost",
    HttpClient = new HttpClient()
};

var customerClient = context.GetCustomerClient();

var response = await customerClient.CreateCustomerAsync(
    new CreateCustomerModel()
    {
        Name = "Customer",
        Address = "Somewhere",
        PhoneNumber = "123456789"
    });
```