using Flurl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NjuskaloScrapeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            ScrapeAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        static async Task ScrapeAsync()
        {
            WriteLine("Started scraping...");

            var client = new HttpClient();

            var entities = new HashSet<string>();

            var html = await GetListPage(client, "2814", "250", "550", "190", 0);

            var pageEntities = ExtractEntitiesFromList(html);
            if (!pageEntities.Any())
            {
                WriteLine("Nothing found");
            }

            entities.UnionWith(pageEntities);

            foreach (var url in entities)
            {
                WriteLine(url);
            }
        }

        static ICollection<string> ExtractEntitiesFromList(string html)
        {
            var entities = new HashSet<string>();
            if (html.Contains("Trenutno nema oglasa koji zadovoljavaju postavljene kriterije pretrage."))
                return entities;

            var focusRegex = new Regex("EntityList--ListItemRegularAd[^>]*>(.*)adsense_adlist_bottom[^>]*>", RegexOptions.Singleline);
            var entityRegex = new Regex("<li class=\"EntityList-item[^>]*?data-href=\"([^\"]*)\"");

            //extract just the meaningful part where regular adds are
            var focusMatch = focusRegex.Match(html);
            if (focusMatch.Success) html = focusMatch.Value;

            //find adds
            var matches = entityRegex.Matches(html);
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                var url = FullyQualifyUrl("https://www.njuskalo.hr", match.Groups[1].Value);
                if (url == null) continue;
                entities.Add(url);
            }
            return entities;
        }

        static string FullyQualifyUrl(string baseUrl, string pathSegment)
        {
            if (pathSegment == null)
                return null;

            if (pathSegment.StartsWith("http"))
                return pathSegment;

            return baseUrl.AppendPathSegment(pathSegment);
        }

        static async Task<string> GetListPage(HttpClient client, string locationId, string priceMin, string priceMax, string roomCountId, int? page)
        {
            var queryParams = new Dictionary<string, string>();

            if (locationId.HasValue()) queryParams["locationId"] = locationId;
            //if (priceMin.HasValue()) queryParams["price%5Bmin%5D"] = priceMin;
            //if (priceMax.HasValue()) queryParams["price%5Bmax%5D"] = priceMax;
            if (priceMin.HasValue()) queryParams["price[min]"] = priceMin;
            if (priceMax.HasValue()) queryParams["price[max]"] = priceMax;

            if (roomCountId.HasValue()) queryParams["roomCountId"] = roomCountId;
            if (page.HasValue()) queryParams["page"] = page.ToString();

            var requestUrl = "https://www.njuskalo.hr"
                .AppendPathSegment("iznajmljivanje-stanova")
                .SetQueryParams(queryParams);

            WriteLine($"GET: {requestUrl}");
            WriteLine();

            var response = await client.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                WriteLine($"response fail: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }


        static void WriteLine(string value = null)
        {
            if (value == null) Console.WriteLine();
            else Console.WriteLine(value);
        }
    }
}
