using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class DeleteLinkBundle(ILoggerFactory loggerFactory, CosmosClient cosmosClient, Hasher hasher)
    {
        private readonly ILogger _logger = loggerFactory.CreateLogger<DeleteLinkBundle>();

        [Function(nameof(DeleteLinkBundle))]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "links/{vanityUrl}")] HttpRequestData req,
            string vanityUrl)
        {
            ClientPrincipal principal = ClientPrincipalUtility.GetClientPrincipal(req);

            var databaseName = Environment.GetEnvironmentVariable("CosmosDb__Database");
            var collectionName = Environment.GetEnvironmentVariable("CosmosDb__Collection");

            var container = cosmosClient.GetContainer(databaseName, collectionName);

            // get the document id where vanityUrl == vanityUrl
            var query = new QueryDefinition("SELECT TOP 1 * FROM c WHERE c.vanityUrl = @vanityUrl")
                .WithParameter("@vanityUrl", vanityUrl);

            var result = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

            if (result.Count != 0)
            {
                var hashedUsername = hasher.HashString(principal.UserDetails);
                if (hashedUsername != result.First().UserId || principal.IdentityProvider != result.First().Provider)
                {
                    return await req.CreateJsonResponse(System.Net.HttpStatusCode.Unauthorized, message: "Unauthorized");
                }

                var partitionKey = new PartitionKey(vanityUrl);
                await container.DeleteItemAsync<LinkBundle>(result.First().Id, partitionKey);

                return await req.CreateOkResponse("Link deleted");
            }

            return await req.CreateJsonResponse(System.Net.HttpStatusCode.NotFound, message: "Link not found");
        }
    }
}