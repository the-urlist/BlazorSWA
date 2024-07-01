using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class ReadLinkBundle(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ReadLinkBundle>();
        private readonly CosmosClient _cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));

        [Function(nameof(ReadLinkBundle))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "links/{vanityUrl}")] HttpRequestData req, string vanityUrl)
        {
            if (string.IsNullOrEmpty(vanityUrl))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "vanityUrl is required");
            }

            var databaseName = Environment.GetEnvironmentVariable("CosmosDb__Database");
            var collectionName = Environment.GetEnvironmentVariable("CosmosDb__Collection");

            var container = cosmosClient.GetContainer(databaseName, collectionName);

            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                            .WithParameter("@vanityUrl", vanityUrl);

            var result = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

            if (result.Count == 0)
            {
                return await req.CreateJsonResponse(HttpStatusCode.NotFound, "No LinkBundle found for this vanity url");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(result.First());
            return response;
        }
    }
}