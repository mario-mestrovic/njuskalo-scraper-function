using Library.Email;
using Library.Njuskalo;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Njuskalo.App.Function
{
    public static class NjuskaloScrapeFunction
    {
        [FunctionName("NjuskaloScrapeFunction")]
        public static async Task RunAsync([TimerTrigger("%NJ_Timer%")]TimerInfo myTimer, ExecutionContext context, Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var sendgridMailSenderOptions = new SendgridMailSenderOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("SG_ApiKey"),
                EmailSender = Environment.GetEnvironmentVariable("SG_EmailSender"),
            };
            var njuskaloStoreOptions = new NjuskaloStoreOptions
            {
                StorageName = Environment.GetEnvironmentVariable("NJ_StorageName"),
                StorageKey = Environment.GetEnvironmentVariable("NJ_StorageKey"),
                TableName = Environment.GetEnvironmentVariable("NJ_TableName"),
                PartitionKey = Environment.GetEnvironmentVariable("NJ_StoragePartitionKey")
            };
            var njuskaloNotifierOptions = new NjuskaloNotifierOptions
            {
                Emails = Environment.GetEnvironmentVariable("NJ_Emails")
            };

            try
            {
                await ProcessScrapeAndNotifyAsync(sendgridMailSenderOptions, njuskaloNotifierOptions, njuskaloStoreOptions, log);
            }
            catch (Exception exception)
            {
                log.LogError(default(EventId), exception, exception.Message);
            }
        }


        private static async Task ProcessScrapeAndNotifyAsync(SendgridMailSenderOptions sendgridMailSenderOptions, NjuskaloNotifierOptions njuskaloNotifierOptions, NjuskaloStoreOptions njuskaloStoreOptions, Microsoft.Extensions.Logging.ILogger log)
        {
            var client = new HttpClient();
            var logger = new DelegateLogger(log);
            var emailSender = new SendgridMailSender(Options.Create(sendgridMailSenderOptions));
            var notifier = new NjuskaloNotifier(Options.Create(njuskaloNotifierOptions), emailSender);
            var store = new NjuskaloStore(Options.Create(njuskaloStoreOptions));

            var t2 = ScraperFactory.CreateNjuskaloDvosobniScraper(client, logger).ScrapeAsync();
            var t3 = ScraperFactory.CreateNjuskaloTrosobniScraper(client, logger).ScrapeAsync();
            var t4 = ScraperFactory.CreateNjuskaloCetverosobniScraper(client, logger).ScrapeAsync();

            await Task.WhenAll(t2, t3, t4);
            var entities = new HashSet<string>(t2.Result.Union(t3.Result));
            logger.WriteLine($"Found {entities.Count} entities.");

            await store.InitStorageAsync();
            await store.PersistAsync(entities);

            var toNotify = await store.GetUnnotifiedAsync();
            logger.WriteLine($"To notify: {toNotify.Count}.");
            if (toNotify.Any())
            {
                var success = await notifier.NotifyAboutEntitiesAsync(toNotify, store.PartitionKey);
                if (success)
                {
                    await store.MarkNotifiedAsync(toNotify);
                    logger.WriteLine($"Notified about {toNotify.Count} entities.");
                }
            }
        }
    }
}
