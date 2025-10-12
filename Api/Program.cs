using Api;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// Setup custom serializer to use System.Text.Json
JsonSerializerOptions jsonSerializerOptions = new()
{
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};
CosmosSystemTextJsonSerializer cosmosSystemTextJsonSerializer = new(jsonSerializerOptions);
CosmosClientOptions cosmosClientOptions = new()
{
    ApplicationName = "SystemTextJson",
    Serializer = cosmosSystemTextJsonSerializer
};

var host = new HostBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<CosmosClient>(sp => new CosmosClient(
            context.Configuration["COSMOSDB_ENDPOINT"],
            context.Configuration["COSMOSDB_KEY"],
            cosmosClientOptions));
        services.AddSingleton<Hasher>(services => new Hasher(
            context.Configuration["HASHER_KEY"],
            context.Configuration["HASHER_SALT"]));
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

// Initialize database and container in development
var environment = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT");
if (!string.IsNullOrWhiteSpace(environment) && environment == "Development")
{
    await InitializeCosmosDbAsync(host.Services);
}

await host.RunAsync();

static async Task InitializeCosmosDbAsync(IServiceProvider services)
{
    var cosmosClient = services.GetRequiredService<CosmosClient>();

    var databaseName = Environment.GetEnvironmentVariable("COSMOSDB_DATABASE") ?? "UrlList";
    var containerName = Environment.GetEnvironmentVariable("COSMOSDB_CONTAINER") ?? "Links";
    var partitionKeyPath = Environment.GetEnvironmentVariable("COSMOSDB_PARTITION_KEY") ?? "/vanityUrl";

    try
    {
        // Create database if it doesn't exist
        var databaseResponse = await cosmosClient.CreateDatabaseIfNotExistsAsync(
            databaseName,
            throughput: 400 // Minimum throughput for dev
        );

        var database = databaseResponse.Database;
        Console.WriteLine($"Database '{databaseName}' created or already exists.");

        // Create container if it doesn't exist
        var containerResponse = await database.CreateContainerIfNotExistsAsync(
            containerName,
            partitionKeyPath,
            throughput: 400
        );

        Console.WriteLine($"Container '{containerName}' created or already exists.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing Cosmos DB: {ex.Message}");
        throw;
    }
}