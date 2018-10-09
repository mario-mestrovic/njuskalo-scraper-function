using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Library.Email;
using Library.Njuskalo;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Njuskalo.App.Function
{
    public static class NjuskaloScrapeFunction
    {
        [FunctionName("NjuskaloScrapeFunction")]
        public static async Task Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer, ExecutionContext context, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");

            var configuration = new ConfigurationBuilder()
                  .SetBasePath(context.FunctionAppDirectory)
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

            await ProcessScrapeAndNotifyAsync(sendgridMailSenderOptions, njuskaloNotifierOptions, njuskaloStoreOptions, log);
        }


        private static async Task ProcessScrapeAndNotifyAsync(SendgridMailSenderOptions sendgridMailSenderOptions, NjuskaloNotifierOptions njuskaloNotifierOptions, NjuskaloStoreOptions njuskaloStoreOptions, TraceWriter log)
        {
            var client = new HttpClient();
            var logger = new DelegateLogger(log);
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

    }
}
