using Flurl;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NjuskaloScrapeConsole
{
    public class NjuskaloScraper
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;
        private readonly string _baseUrl = "https://www.njuskalo.hr";
        private readonly string _pathSegment;
        private readonly IDictionary<string, string> _queryParams;
        private readonly bool _scrapeAllPages;

        public NjuskaloScraper(HttpClient client, ILogger logger, string pathSegment, IDictionary<string, string> queryParams, bool scrapeAllPages)
        {
            _client = client;
            _logger = logger;
            _pathSegment = pathSegment;
            _queryParams = queryParams;
            _scrapeAllPages = scrapeAllPages;
        }

        public async Task<ICollection<string>> ScrapeAsync()
        {
            _logger.WriteLine("Started scraping...");


            var entities = new HashSet<string>();

            var continueNextPage = _scrapeAllPages;
            var page = 1;
            while (page == 1 || continueNextPage)
            {
                _logger.WriteLine($"Page: {page}. STARTED.");
                var html = await GetResultPageHtmlAsync(page);

                var pageEntities = ExtractEntitiesFromList(html);
                if (!pageEntities.Any())
                {
                    continueNextPage = false;
                    _logger.WriteLine($"Page: {page}. Nothing found.");
                }
                else
                {
                    _logger.WriteLine($"Page: {page}. Found {pageEntities.Count} items.");
                }

                entities.UnionWith(pageEntities);
                page++;
                _logger.WriteLine();
            }

            foreach (var url in entities)
            {
                _logger.WriteLine(url);
            }

            return entities;
        }

        private async Task<string> GetResultPageHtmlAsync(int? page)
        {
            var pageQueryParams = new Dictionary<string, string>(_queryParams);
            if (page.HasValue()) pageQueryParams["page"] = page.ToString();

            var requestUrl = _baseUrl
                .AppendPathSegment(_pathSegment)
                .SetQueryParams(pageQueryParams);

            _logger.WriteLine($"GET: {requestUrl}");

            var response = await _client.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                _logger.WriteLine($"GET failed with status code: {response.StatusCode}");
                return null;
            }

            return await response.Content.ReadAsStringAsync();
        }

        private ICollection<string> ExtractEntitiesFromList(string html)
        {
            var entities = new HashSet<string>();
            if (html == null)
                return entities;
            if (html.Contains("Trenutno nema oglasa koji zadovoljavaju postavljene kriterije pretrage."))
                return entities;

            var focusRegex = new Regex("EntityList--ListItemRegularAd[^>]*>(.*)adsense_adlist_bottom[^>]*>", RegexOptions.Singleline);
            var entityRegex = new Regex("<li class=\"EntityList-item[^>]*?data-href=\"([^\"]*)\"");

            //extract just the meaningful part where regular adds are
            var focusMatch = focusRegex.Match(html);
            if (!focusMatch.Success)
                return entities;

            //focus on the part where results should be
            html = focusMatch.Value;

            var matches = entityRegex.Matches(html);
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                var url = match.Groups[1].Value;
                if (string.IsNullOrWhiteSpace(url) || url == "/") continue;

                url = FullyQualifyUrl(_baseUrl, url);
                entities.Add(url);
            }
            return entities;
        }

        private string FullyQualifyUrl(string baseUrl, string pathSegment)
        {
            if (pathSegment == null)
                return null;

            if (pathSegment.StartsWith("http"))
                return pathSegment;

            return baseUrl.AppendPathSegment(pathSegment);
        }


    }
}
