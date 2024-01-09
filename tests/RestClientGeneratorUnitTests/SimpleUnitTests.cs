namespace RestClientGeneratorUnitTests;

using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

[TestClass]
public class SimpleUnitTests
{
    #region Get Widget

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

    #region Post Widget

    [DataRow("test")]
    [TestMethod]
    public void SimpleTestClient_PostWidget(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("POST", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget", req.RequestUri.AbsolutePath);
                var content = req.Content.ReadAsStringAsync().Result;
                var model = new JsonContext().Deserialize<PostModel>(content);
                Assert.AreEqual(name, model.Name);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = client.PostWidget(
            new PostModel()
            {
                Name = name
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [DataRow("test")]
    [TestMethod]
    public async Task SimpleTestClient_PostWidgetAsync(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("POST", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget", req.RequestUri.AbsolutePath);

                var content = req.Content.ReadAsStringAsync().Result;
                var model = new JsonContext().Deserialize<PostModel>(content);
                Assert.AreEqual(name, model.Name);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = await client.PostWidgetAsync(
            new PostModel()
            {
                Name = name
            });

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
                var content = req.Content.ReadAsStringAsync().Result;
                var model = new JsonContext().Deserialize<PutModel>(content);
                Assert.AreEqual("newtest", model.Name);
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
                Name = "newtest"
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [DataRow("test")]
    [TestMethod]
    public async Task SimpleTestClient_PutWidgetAsync(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("PUT", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);

                var content = req.Content.ReadAsStringAsync().Result;
                var model = new JsonContext().Deserialize<PutModel>(content);
                Assert.AreEqual("newtest", model.Name);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = await client.PutWidgetAsync(
            name,
            new PutModel()
            {
                Name = "newtest"
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    #endregion

    #region Patch Widget

    [DataRow("test")]
    [TestMethod]
    public void SimpleTestClient_PatchWidget(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("PATCH", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);

                var content = req.Content.ReadAsStringAsync().Result;
                var model = new JsonContext().Deserialize<PatchModel>(content);
                Assert.AreEqual("newtest", model.Name);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = client.PatchWidget(
            name,
            new PatchModel()
            {
                Name = "newtest"
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [DataRow("test")]
    [TestMethod]
    public async Task SimpleTestClient_PatchWidgetAsync(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("PATCH", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);

                var content = req.Content.ReadAsStringAsync().Result;
                var model = new JsonContext().Deserialize<PatchModel>(content);
                Assert.AreEqual("newtest", model.Name);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = await client.PatchWidgetAsync(
            name,
            new PatchModel()
            {
                Name = "newtest"
            });

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    #endregion

    #region Delete Widget

    [DataRow("test")]
    [TestMethod]
    public void SimpleTestClient_DeleteWidget(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("DELETE", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = client.DeleteWidget(name);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    [DataRow("test")]
    [TestMethod]
    public async Task SimpleTestClient_DeleteWidgetAsync(string name)
    {
        var httpHandler = new TestHttpMessageHandler
        {
            Response = (req) =>
            {
                Assert.AreEqual("DELETE", req.Method.Method);
                Assert.AreEqual($"/api/v1/widget/{name}/name", req.RequestUri.AbsolutePath);
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var context = new TestRestClientContext();
        context.Options.BaseUrl = "http://localhost";
        context.Options.HttpClient = new HttpClient(httpHandler);
        var client = context.GetSimpleTestClient();

        var response = await client.DeleteWidgetAsync(name);

        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccessStatusCode);
    }

    #endregion
}