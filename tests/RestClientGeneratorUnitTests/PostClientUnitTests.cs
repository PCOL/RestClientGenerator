namespace RestClientGeneratorUnitTests;

using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

[TestClass]
public class PostClientUnitTests
{
    [TestMethod]
    public async Task PostClient_PostWidget_WithResponseAction()
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("POST", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetPostClient();

        var called = false;
        var response = await client.PostWidgetAsync(
            new PostModel()
            {
                Name = "test"
            },
            (HttpResponseMessage response) =>
            {
                called = true;
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.IsTrue(called);
    }

    [TestMethod]
    public async Task PostClient_PostWidget_WithResponseFunc()
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("POST", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetPostClient();

        var called = false;
        var response = await client.PostWidgetAsync(
            new PostModel()
            {
                Name = "test"
            },
            (HttpResponseMessage response) =>
            {
                called = true;
                return Task.CompletedTask;
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
        Assert.IsTrue(called);
    }
}
