using System;
using System.Net.Http;

namespace NjuskaloScrapeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new HttpClient();
            var logger = new DelegateLogger(WriteLine);

            var scraper1 = ScraperFactory.CreateNjuskaloTrosobniScraper(client, logger);
            scraper1.ScrapeAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }


        static void WriteLine(string value = null)
        {
            if (value == null) Console.WriteLine();
            else Console.WriteLine(value);
        }
    }
}
