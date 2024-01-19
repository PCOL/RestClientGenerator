﻿namespace TestClient;

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
        var apiKey = "";

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

        context.Options.AuthorizationHeaderFactory = new AuthorizationFactory("Bearer", tokenResult.Result);

        var result = await context.GetClient().GetWorkloadAsync("55c21b63-099c-4f23-8d06-a56afdfd4d34");
        Console.WriteLine(result.Status);
        Console.WriteLine(result.Result?.Id);
        Console.WriteLine(result.Result?.Name);
    }

}
