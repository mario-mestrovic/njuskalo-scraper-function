using System.Collections.Generic;
using System.Net.Http;

namespace Library.Njuskalo
{
    public static class ScraperFactory
    {
        public static NjuskaloScraper CreateNjuskaloDvosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationId"] = "2814";
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["roomCountId"] = "189"; //2-2.5 soba

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }

        public static NjuskaloScraper CreateNjuskaloTrosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationId"] = "2814"; //tresnjevka
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["roomCountId"] = "190"; //3-3.5 soba

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }
        public static NjuskaloScraper CreateNjuskaloCetverosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationId"] = "2814"; //tresnjevka
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["roomCountId"] = "191"; //4+ soba

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }


        public static NjuskaloScraper CreateNjuskaloBasicScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationId"] = "2814";
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "550";

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }

    }
}
