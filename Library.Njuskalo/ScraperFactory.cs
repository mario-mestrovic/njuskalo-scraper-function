using System.Collections.Generic;
using System.Net.Http;

namespace Library.Njuskalo
{
    public static class ScraperFactory
    {
        public static NjuskaloScraper CreateNjuskaloDvosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationIds"] = "2814";
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["numberOfRooms[min]"] = "two-rooms"; //2 sobe
            queryParams["numberOfRooms[max]"] = "two-rooms"; //2 sobe

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }

        public static NjuskaloScraper CreateNjuskaloTrosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationIds"] = "2814"; //tresnjevka
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["numberOfRooms[min]"] = "three-rooms"; //3 sobe
            queryParams["numberOfRooms[max]"] = "three-rooms"; //3 sobe

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }
        public static NjuskaloScraper CreateNjuskaloCetverosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationIds"] = "2814"; //tresnjevka
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["includeOtherCategories"] = "1"; //Prikaži i luksuzne stanove
            queryParams["numberOfRooms[min]"] = "four-rooms"; //4 sobe
            queryParams["numberOfRooms[max]"] = "four-rooms"; //4 sobe

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }

        public static NjuskaloScraper CreateNjuskaloDvosobniMin50KvadrataScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationIds"] = "2814";
            queryParams["price[min]"] = "350";
            queryParams["price[max]"] = "670";
            queryParams["includeOtherCategories"] = "1"; //Prikaži i luksuzne stanove
            queryParams["numberOfRooms[min]"] = "two-rooms"; //2 sobe
            queryParams["numberOfRooms[max]"] = "two-rooms"; //2 sobe
            queryParams["livingArea[min]"] = "60"; //min 60 kvadrata

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }

        public static NjuskaloScraper CreateNjuskaloMinimalnoTrosobniScraper(HttpClient client, ILogger logger)
        {
            var queryParams = new Dictionary<string, string>();
            queryParams["locationIds"] = "2814"; //tresnjevka. to add multiple locations just comma separate them 2814,2815,2816...
            queryParams["price[min]"] = "250";
            queryParams["price[max]"] = "670";
            queryParams["includeOtherCategories"] = "1"; //Prikaži i luksuzne stanove
            queryParams["numberOfRooms[min]"] = "three-rooms"; //3 soba

            var scraper = new NjuskaloScraper(client, logger, "iznajmljivanje-stanova", queryParams, true);
            return scraper;
        }
    }
}
