using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class ReadLinkBundle(ILoggerFactory loggerFactory, CosmosClient cosmosClient, IConfiguration configuration)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<ReadLinkBundle>();

        [Function(nameof(ReadLinkBundle))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "links/{vanityUrl}")] HttpRequestData req, string vanityUrl)
        {
            if (string.IsNullOrEmpty(vanityUrl))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "vanityUrl is required");
            }

            var databaseName = configuration["COSMOSDB_DATABASE"];
            var containerName = configuration["COSMOSDB_CONTAINER"];

            var database = cosmosClient.GetDatabase(databaseName);
            var container = database.GetContainer(containerName);

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