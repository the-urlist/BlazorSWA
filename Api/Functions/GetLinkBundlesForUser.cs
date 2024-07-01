using Api.Utility;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class GetLinkBundlesForUser(CosmosClient cosmosClient, Hasher hasher)
    {
        [Function(nameof(GetLinkBundlesForUser))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] HttpRequestData req)
        {
            try
            {
                var databaseName = Environment.GetEnvironmentVariable("CosmosDb__Database");
                var collectionName = Environment.GetEnvironmentVariable("CosmosDb__Collection");

                var container = cosmosClient.GetContainer(databaseName, collectionName);

                var res = req.CreateResponse();

                ClientPrincipal clientPrincipal = ClientPrincipalUtility.GetClientPrincipal(req);

                if (clientPrincipal != null)
                {
                    string username = hasher.HashString(clientPrincipal.UserDetails);
                    string provider = clientPrincipal.IdentityProvider;

                    Console.WriteLine(clientPrincipal.UserDetails);
                    Console.WriteLine(username);

                    var query = new QueryDefinition("SELECT c.id, c.vanityUrl, c.description, c.links FROM c WHERE c.userId = @username AND c.provider = @provider")
                        .WithParameter("@username", username)
                        .WithParameter("@provider", provider);

                    var response = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

                    if (response.Count == 0)
                    {
                        return await req.CreateJsonResponse(HttpStatusCode.NotFound, "No link bundles found for user");
                    }

                    await res.WriteAsJsonAsync(response);

                    return res;
                }

                // return 401 if no client principal
                await res.WriteAsJsonAsync(new { error = "Unauthorized" }, HttpStatusCode.Unauthorized);

                return res;

            }
            catch (Exception ex)
            {
                var res = req.CreateResponse();
                await res.WriteAsJsonAsync(new { error = ex.Message }, HttpStatusCode.InternalServerError);

                return res;
            }
        }
    }
}
