using Library.Email;
using Library.Njuskalo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Njuskalo.App.Function
{
    public static class OptionsFactory
    {
        public static NjuskaloStoreOptions CreateNjuskaloStoreOptions()
        {
            var njuskaloStoreOptions = new NjuskaloStoreOptions
            {
                StorageName = Environment.GetEnvironmentVariable("NJ_StorageName"),
                StorageKey = Environment.GetEnvironmentVariable("NJ_StorageKey"),
                TableName = Environment.GetEnvironmentVariable("NJ_TableName"),
                PartitionKey = Environment.GetEnvironmentVariable("NJ_StoragePartitionKey")
            };
            return njuskaloStoreOptions;
        }

        public static SendgridMailSenderOptions CreateSendgridMailSenderOptions()
        {
            var sendgridMailSenderOptions = new SendgridMailSenderOptions
            {
                ApiKey = Environment.GetEnvironmentVariable("SG_ApiKey"),
                EmailSender = Environment.GetEnvironmentVariable("SG_EmailSender"),
            };
            return sendgridMailSenderOptions;
        }

        public static NjuskaloNotifierOptions CreateNjuskaloNotifierOptions()
        {
            var njuskaloNotifierOptions = new NjuskaloNotifierOptions
            {
                Emails = Environment.GetEnvironmentVariable("NJ_Emails")
            };
            return njuskaloNotifierOptions;
        }

        public static ICollection<string> GetScraperKeys()
        {
            var keys = new HashSet<string>();
            foreach (var key in Environment.GetEnvironmentVariables().Keys.OfType<string>().Where(x => x.StartsWith("NJ_Scraper", StringComparison.OrdinalIgnoreCase)))
            {
                keys.Add(key);
            }
            return keys;
        }

        public static ICollection<string> GetScraperQueryParamKeys()
        {
            var keys = new HashSet<string>();
            foreach (var key in Environment.GetEnvironmentVariables().Keys.OfType<string>().Where(x => x.StartsWith("NJ_QueryParams_", StringComparison.OrdinalIgnoreCase)))
            {
                keys.Add(key);
            }
            return keys;
        }

        public static ICollection<NjuskaloScraperOptions> CreateNjuskaloScraperOptions()
        {
            var scraperKeys = GetScraperKeys();
            var scraperQueryParamKeys = GetScraperQueryParamKeys();
            var scraperOptions = new List<NjuskaloScraperOptions>();

            if (!scraperKeys.Any())
                return scraperOptions;

            foreach (var key in scraperKeys)
            {
                var queryParamKey = scraperQueryParamKeys.FirstOrDefault(x => x.EndsWith(key.Replace("NJ_", string.Empty)));
                var o = CreateNjuskaloScraperOptions(key, queryParamKey);
                if (o == null) continue;
                scraperOptions.Add(o);
            }
            return scraperOptions;
        }

        public static NjuskaloScraperOptions CreateNjuskaloScraperOptions(string scraperKey, string scraperQueryParamKey)
        {
            var scraperOptionValue = Environment.GetEnvironmentVariable(scraperKey) ?? string.Empty;
            var scraperOptionArray = scraperOptionValue.Split(new[] { "||" }, StringSplitOptions.None).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (scraperOptionArray.Length < 3)
                return null;

            var queryParamValue = Environment.GetEnvironmentVariable(scraperQueryParamKey) ?? string.Empty;
            var queryParamArray = queryParamValue.Split(new[] { "||" }, StringSplitOptions.None).Select(x => x?.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
            var dictionary = new Dictionary<string, string>();
            foreach (var item in queryParamArray)
            {
                var itemArray = item.Split('=');
                if (itemArray.Length != 2)
                    continue;

                dictionary[itemArray[0]] = itemArray[1];
            }

            if (!dictionary.Any())
                return null;

            var njuskaloScraperOptions = new NjuskaloScraperOptions
            {
                Name = scraperOptionArray[0],
                BaseUrl = scraperOptionArray[1],
                PathSegment = scraperOptionArray[2],
                QueryParams = dictionary
            };

            return njuskaloScraperOptions;
        }
    }
}
