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
        public static async Task RunAsync([TimerTrigger("%NJ_Timer%", RunOnStartup = true)]TimerInfo myTimer, ExecutionContext context, Microsoft.Extensions.Logging.ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var mailSenderOptions = OptionsFactory.CreateSendgridMailSenderOptions();
            var storeOptions = OptionsFactory.CreateNjuskaloStoreOptions();
            var notifierOptions = OptionsFactory.CreateNjuskaloNotifierOptions();
            var scraperOptions = OptionsFactory.CreateNjuskaloScraperOptions();
            if (!scraperOptions.Any())
            {
                log.LogError("Scraper options are not configured.");
                return;
            }

            try
            {
                await ProcessScrapeAndNotifyAsync(mailSenderOptions, notifierOptions, storeOptions, scraperOptions, log);
            }
            catch (Exception exception)
            {
                log.LogError(default(EventId), exception, exception.Message);
            }
        }


        private static async Task ProcessScrapeAndNotifyAsync(SendgridMailSenderOptions sendgridMailSenderOptions, NjuskaloNotifierOptions njuskaloNotifierOptions, NjuskaloStoreOptions njuskaloStoreOptions, ICollection<NjuskaloScraperOptions> scraperOptions, Microsoft.Extensions.Logging.ILogger log)
        {
            using (var client = new HttpClient())
            {
                var logger = new DelegateLogger(log);
                var emailSender = new SendgridMailSender(Options.Create(sendgridMailSenderOptions));
                var notifier = new NjuskaloNotifier(Options.Create(njuskaloNotifierOptions), emailSender);
                var store = new NjuskaloStore(Options.Create(njuskaloStoreOptions));

                var scrapeTasks = new List<Task<ICollection<string>>>();
                foreach (var options in scraperOptions)
                {
                    var scraper = new NjuskaloScraper(Options.Create(options), client, logger);
                    scrapeTasks.Add(scraper.ScrapeAsync());
                }
                await Task.WhenAll(scrapeTasks);

                var entities = new HashSet<string>(scrapeTasks.SelectMany(x => x.Result));
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
}
