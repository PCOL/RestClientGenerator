namespace TestClient;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using RestClient;

public class Program
{
    public static async Task Main(string[] args)
    {
        var appSettingsPath = GetAppSettingsPath(args);

        var builder = new ConfigurationBuilder()
            .AddJsonFile(appSettingsPath, optional: true, reloadOnChange: false);

        var config = builder.Build();

        var generalConfig = new GeneralConfiguration();
        config.GetSection("Startup:General").Bind(generalConfig);

        var context = new ClientContext();
        context.Options = new RestClientOptions()
        {
            BaseUrl = generalConfig.Url,
            HttpClient = new HttpClient()
        };


        ////var serviceResult = await context.GetClient().CreateWorkloadAsync(
        ////    new CreateWorkloadModel()
        ////    {
        ////        Name = "Test"
        ////    });


        var tokenResult = await context.GetTokenClient().GetTokenAsync(generalConfig.ApiKey);


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

    /// <summary>
    /// Get location of appsettings.json file.
    /// </summary>
    /// <returns>Path to appsettings file.</returns>
    /// <param name="args">The command line arguments.</param>
    public static string GetAppSettingsPath(string[] args)
    {
        const string appSettingsKey = "appsettings";

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrWhiteSpace(environment) == false)
        {
            environment = "." + environment;
        }

        var settingFilesConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    { appSettingsKey, $"appsettings{environment}.json" },
                })
            .AddCommandLine(
                args,
                new Dictionary<string, string>
                {
                    { "--appsettings", appSettingsKey }
                })
            .Build();

        return Path.Combine(AppContext.BaseDirectory, settingFilesConfiguration[appSettingsKey]);
    }
}
