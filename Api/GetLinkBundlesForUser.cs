using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorApp.Shared;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class GetLinkBundlesForUser
    {
        private readonly ILogger _logger;
        private readonly CosmosClient _cosmosClient;

        public GetLinkBundlesForUser(ILoggerFactory loggerFactory, CosmosClient cosmosClient)
        {
            _logger = loggerFactory.CreateLogger<GetLinkBundlesForUser>();
            _cosmosClient = cosmosClient ?? throw new System.ArgumentNullException(nameof(cosmosClient));
        }

        [Function("GetLinkBundlesForUser")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user")] HttpRequestData req)
        {
            try
            {
                var container = _cosmosClient.GetContainer("TheUrlist", "linkbundles");
                var res = req.CreateResponse();

                ClientPrincipal principal = null;

                if (req.Headers.TryGetValues("x-ms-client-principal", out var header))
                {
                    var data = header.FirstOrDefault();
                    var decoded = Convert.FromBase64String(data);
                    var json = Encoding.UTF8.GetString(decoded);
                    principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (principal != null)
                    {
                        Hasher hasher = new Hasher();
                        string username = hasher.HashString(principal.UserDetails);
                        string provider = principal.IdentityProvider;

                        var query = new QueryDefinition("SELECT c.id, c.vanityUrl, c.description, c.links FROM c WHERE c.userId = @username AND c.provider = @provider")
                            .WithParameter("@username", username)
                            .WithParameter("@provider", provider);

                        var response = await container.GetItemQueryIterator<LinkBundle>(query).ReadNextAsync();

                        if (!response.Any())
                        {
                            return req.CreateResponse(HttpStatusCode.NotFound);
                        }

                        await res.WriteAsJsonAsync(response);

                        return res;
                    }
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
