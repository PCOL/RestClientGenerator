namespace RestClientGeneratorUnitTests;

using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;

[TestClass]
public class GetClientUnitTests
{
    [TestMethod]
    public async Task GetClient_GetWidgets()
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("GET", req.Method.Method);
                Assert.AreEqual($"/api/v1/widgets", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetGetClient();

        var response = await client.GetWidgetsAsync();

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [TestMethod]
    [DataRow("test")]
    [DataRow("00e7b19c-df3a-470e-ba92-c4bcc1712022")]
    public async Task GetClient_GetWidgets_WithQueryParameter(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("GET", req.Method.Method);
                Assert.AreEqual($"/api/v1/widgets", req.RequestUri.AbsolutePath);
                Assert.AreEqual($"?name={name}", req.RequestUri.Query);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetGetClient();

        var response = await client.GetWidgetsAsync(name);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }
    [TestMethod]
    [DataRow("test", TestOptions.Option1)]
    [DataRow("test1", TestOptions.Option2)]
    [DataRow("test2", TestOptions.Option3)]
    public async Task GetClient_GetWidgets_WithQueryParameters(string name, TestOptions option)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("GET", req.Method.Method);
                Assert.AreEqual($"/api/v1/widgets", req.RequestUri.AbsolutePath);
                Assert.AreEqual($"?name={name}&option={option}", req.RequestUri.Query);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetGetClient();

        var response = await client.GetWidgetsAsync(name, option);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }
}
