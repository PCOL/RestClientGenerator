namespace TestClient;

using System;
using System.Threading;
using System.Threading.Tasks;
using RestClient.Generator;

public class Program
{
    public static async Task Main(string[] args)
    {
        var context = new ClientContext();

        var result = await context.IClient.GetWorkloadAsync("Test");

        Console.WriteLine(result);
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
                            .AppendLine("// Comment")))
            .Build();

        Console.WriteLine(@class);
    }

}
