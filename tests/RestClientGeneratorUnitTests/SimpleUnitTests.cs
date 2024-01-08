namespace RestClientGeneratorUnitTests;

using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

[TestClass]
public class SimpleUnitTests
{
    #region GetWidget

    [DataRow("test")]
    [TestMethod]
    public void SimpleTestClient_GetWidget(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("GET", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = client.GetWidget(name);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [DataRow("test")]
    [TestMethod]
    public async Task SimpleTestClient_GetWidgetAsync(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("GET", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = await client.GetWidgetAsync(name);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    #endregion

    #region Put Widget

    [DataRow("test")]
    [TestMethod]
    public void SimpleTestClient_PutWidget(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("PUT", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = client.PutWidget(
            name,
            new PutModel()
            {
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    #endregion
}