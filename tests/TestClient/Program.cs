namespace TestClient;

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using RestClient;

public class Program
{
    public static async Task Main(string[] args)
    {
        var apiKey = "NmY2ZWE3MjA2ZjBjNDkyYjlkMTFlYTU1MmRmZjQxYmM6bjNZV21Mb2I2UW5BOVA3ZHVXZkhjS3JuM1ZNcjR6Nkpicmw4TWZuZStoYUVLeGRwR3l6cDA5RmtqMDlJdGoyOEhBU2xPT0xCWFVqU2l2Y2psNXhpeFE9PQ==";

        var context = new ClientContext();
        context.Options = new RestClientOptions()
        {
            BaseUrl = "https://dev-eu-west-2-core.ampp.technology",
            HttpClient = new HttpClient()
        };


        ////var serviceResult = await context.GetClient().CreateWorkloadAsync(
        ////    new CreateWorkloadModel()
        ////    {
        ////        Name = "Test"
        ////    });


        var tokenResult = await context.GetTokenClient().GetTokenAsync(apiKey);


        var json = JsonSerializer.Serialize<IServiceResult<string>>(
            tokenResult,
            new JsonContext().GetTypeInfo(typeof(IServiceResult<string>)) as JsonTypeInfo<IServiceResult<string>>);

        Console.WriteLine(json);
        Console.WriteLine("--------");

        var result = await context.GetClient().GetWorkloadAsync(tokenResult.Result, "55c21b63-099c-4f23-8d06-a56afdfd4d34");
        Console.WriteLine(result.Status);
        Console.WriteLine(result.Result?.Id);
        Console.WriteLine(result.Result?.Name);
    }

}
