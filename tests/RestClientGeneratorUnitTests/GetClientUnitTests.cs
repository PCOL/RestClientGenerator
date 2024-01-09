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
}
