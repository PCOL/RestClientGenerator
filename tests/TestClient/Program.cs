namespace TestClient;

using System;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using RestClient;
using RestClient.Generator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var context = new ClientContext();
        context.Options = new RestClientOptions()
        {
            BaseUrl = "https://dev-eu-west-2-core.ampp.technology",
            HttpClient = new HttpClient()
        };

        ////var result = await context.GetClient().GetWorkloadAsync("Test");
        ////Console.WriteLine(result);

        var serviceResult = await context.GetClient().CreateWorkloadAsync(
            new CreateWorkloadModel()
            {
                Name = "Test"
            });


        var json = JsonSerializer.Serialize<IServiceResult<string>>(
            serviceResult,
            new JsonContext().GetTypeInfo(typeof(IServiceResult<string>)) as JsonTypeInfo<IServiceResult<string>>);

        Console.WriteLine(json);

        Console.WriteLine("--------");

        var @class = new FluentClassBuilder("MyApp.MyClass")
//            .Namespace("MyApp")
            .Using("System")
            .Using("System.Threading")
            .Using("GV.Platform.Logging.LogProperties", true)
            .Public()
            //.Attribute(attr => attr.TypeName("TestAttribute"))
            .Method(
                "GetWigetAsync",
                meth => meth
                    .Public()
                    .Async()
                    .Returns("Task<string>")
                    .Param(p => p
                        .TypeName("string")
                        .Name("id")
                        //.Attribute(a => a.TypeName("Required"))
                    )
                    .Param(p => p
                        .Params()
                        .TypeName("object[]")
                        .Name("properties"))
                    //.Attribute(a => a.TypeName("Required"))
                    .Body(
                        b => b
                            .Comment("Comment")
                            .Variable<int>("number")
                            .Variable<bool>("boolean1")
                            .Variable<bool>("boolean2", true)
                            .Variable<string>("name")
                            .Variable("var", "test", "1")
                            .BlankLine()
                            .If(
                                "test == 1",
                                c => c.Assign("test", "2"))
                            .Else(
                                c => c.Assign("test", "3"))
                            .BlankLine()
                            .Comment("End")))
            .Build();

        ////Console.WriteLine(@class);
    }

}
