using BlazorApp.Shared;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Functions
{
    public class GetOpenGraphInfo
    {
        [Function(nameof(GetOpenGraphInfo))]
        public static async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "oginfo")] HttpRequestData req)
        {
            var link = await req.ReadFromJsonAsync<Link>();
            if(link is null)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Unable to parse Link");
            }

            if (!link.Url.StartsWith("http://") && !link.Url.StartsWith("https://"))
            {
                link.Url = $"https://{link.Url}";
            }

            var response = await GetResponseFromAsync(link.Url);

            if(response is null)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Unable to load URL");
            }

            // if response status code is 301 or 302, follow the redirect
            if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.MovedPermanently)
            {
                link.Url = response.Headers.Location.AbsoluteUri;
                response = await GetResponseFromAsync(link.Url);
            }

            if (response is null || response.StatusCode != HttpStatusCode.OK)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Unable to load URL");
            }

            // Get the response URI since it may have changed due to redirects
            var baseUri = response.RequestMessage.RequestUri.AbsoluteUri;

            var html = await response.Content.ReadAsStringAsync();
            var parser = new AngleSharp.Html.Parser.HtmlParser();
            var document = parser.ParseDocument(html);
            if (document == null)
            {
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, "Unable to parse document");
            }

            var title = document.QuerySelector("title")?.TextContent
                        ?? document.QuerySelector("meta[property='og:title']")?.GetAttribute("content")
                        ?? document.QuerySelector("meta[property='twitter:title']")?.GetAttribute("content")
                        ?? document.QuerySelector("h1")?.TextContent
                        ?? document.QuerySelector("meta[property='og:site_name']")?.GetAttribute("content")
                        ?? "";

            var description = document.QuerySelector("meta[property='og:description']")?.GetAttribute("content")
                        ?? document.QuerySelector("meta[property='twitter:description']")?.GetAttribute("content")
                        ?? document.QuerySelector("meta[name='description']")?.GetAttribute("content")
                        ?? "";

            var image = document.QuerySelector("meta[property='og:image']")?.GetAttribute("content")
                         ?? document.QuerySelector("meta[property='twitter:image']")?.GetAttribute("content")
                         ?? document.QuerySelector("link[rel='apple-touch-icon']")?.GetAttribute("href")
                         ?? document.QuerySelector("link[rel='mask-icon']")?.GetAttribute("href")
                         ?? document.QuerySelector("link[rel='shortcut icon']")?.GetAttribute("href")
                         ?? document.QuerySelector("meta[itemprop='image']")?.GetAttribute("content")
                         ?? "";

            // If the image is an empty string, try to load favicon.ico from the domain root
            if (string.IsNullOrEmpty(image))
            {
                var uri = new System.Uri(baseUri);
                var faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";
                var faviconResponse = await GetResponseFromAsync(faviconUrl);
                if (faviconResponse is not null && faviconResponse.StatusCode == HttpStatusCode.OK)
                {
                    image = faviconUrl;
                }
            }

            // If the image is a relative URL, make it absolute
            if (!string.IsNullOrEmpty(image) && !image.StartsWith("http"))
            {
                image = new System.Uri(new System.Uri(baseUri), image).AbsoluteUri;
            }

            // Update the link with the new information
            link.Title = title;
            link.Description = description;
            link.Image = image;

            // Return the updated link
            return await req.CreateOkResponse(link);
        }

        private static async Task<HttpResponseMessage> GetResponseFromAsync(string url)
        {
            var httpClient = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });

            // add a header to mimic a browser
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            httpClient.DefaultRequestHeaders.AcceptLanguage.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("en-US", 0.9));
            httpClient.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

            HttpResponseMessage response = null;
            try
            {
                response = await httpClient.GetAsync(url);
            }
            finally
            {
                httpClient.Dispose();
            }

            return response;
        }
    }
}
