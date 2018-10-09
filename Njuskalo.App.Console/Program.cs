using Library.Email;
using Library.Njuskalo;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Njuskalo.App.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
            System.Console.ReadLine();
        }


        static async Task MainAsync()
        {
            var configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                  .AddEnvironmentVariables()
                  .Build();

            var sendgridMailSenderOptions = new SendgridMailSenderOptions
            {
                ApiKey = configuration.GetSection("SendgridMailSenderOptions").GetValue<string>("ApiKey"),
                EmailSender = configuration.GetSection("SendgridMailSenderOptions").GetValue<string>("EmailSender"),
            };
            var njuskaloStoreOptions = new NjuskaloStoreOptions
            {
                StorageName = configuration.GetSection("NjuskaloStoreOptions").GetValue<string>("StorageName"),
                StorageKey = configuration.GetSection("NjuskaloStoreOptions").GetValue<string>("StorageKey"),
                TableName = configuration.GetSection("NjuskaloStoreOptions").GetValue<string>("TableName")
            };
            var njuskaloNotifierOptions = new NjuskaloNotifierOptions
            {
                Emails = configuration.GetSection("NjuskaloNotifierOptions").GetValue<string>("Emails")
            };

            var client = new HttpClient();
            var logger = new DelegateLogger(WriteLine);
            var emailSender = new SendgridMailSender(Options.Create(sendgridMailSenderOptions));
            var notifier = new NjuskaloNotifier(Options.Create(njuskaloNotifierOptions), emailSender);
            var store = new NjuskaloStore(Options.Create(njuskaloStoreOptions));


            var t2 = ScraperFactory.CreateNjuskaloDvosobniScraper(client, logger).ScrapeAsync();
            var t3 = ScraperFactory.CreateNjuskaloTrosobniScraper(client, logger).ScrapeAsync();

            await Task.WhenAll(t2, t3);
            var entities = new HashSet<string>(t2.Result.Union(t3.Result));

            await store.InitStorageAsync();
            await store.PersistAsync(entities, "njuskalo.hr");

            var toNotify = await store.GetUnnotifiedAsync("njuskalo.hr");
            if (toNotify.Any())
            {
                var success = await notifier.NotifyAboutEntitiesAsync(toNotify);
                if (success)
                {
                    await store.MarkNotifiedAsync(toNotify, "njuskalo.hr");
                }
            }
        }

        static void WriteLine(string value = null)
        {
            if (value == null) System.Console.WriteLine();
            else System.Console.WriteLine(value);
        }

    }
}
