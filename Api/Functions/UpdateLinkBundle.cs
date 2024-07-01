using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Api
{
    public class UpdateLinkBundle(CosmosClient cosmosClient, Hasher hasher)
    {
        [Function(nameof(UpdateLinkBundle))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "links/{vanityUrl}")] HttpRequestData req,
                       string vanityUrl, FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("UpdateLinks");
            logger.LogInformation("C# HTTP trigger function processed a request.");
            var linkBundle = await req.ReadFromJsonAsync<LinkBundle>();

            if (!ValidatePayLoad(linkBundle))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Invalid payload");
            }

            if (string.IsNullOrEmpty(vanityUrl))
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Invalid vanity url");
            }

            try
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

                    linkBundle.UserId = hashedUsername;
                    linkBundle.Provider = principal.IdentityProvider;
                    var partitionKey = new PartitionKey(vanityUrl);
                    var response = await container.UpsertItemAsync(linkBundle, partitionKey);
                    var responseMessage = req.CreateResponse(HttpStatusCode.OK);
                    var linkDocument = response.Resource;
                    await responseMessage.WriteAsJsonAsync(linkDocument);
                    return responseMessage;
                }

                return await req.CreateJsonResponse(System.Net.HttpStatusCode.NotFound, message: "Link not found");
            }
            catch (Exception ex)
            {
                return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        private static bool ValidatePayLoad(LinkBundle linkDocument)
        {
            return (linkDocument != null) && linkDocument.Links.Count > 0;
        }
    }
}
