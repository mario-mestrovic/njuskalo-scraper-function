using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.Njuskalo
{
    public class NjuskaloStore
    {
        private readonly NjuskaloStoreOptions _options;

        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudTableClient _tableClient;
        private readonly CloudTable _table;

        public NjuskaloStore(IOptions<NjuskaloStoreOptions> options)
        {
            _options = options.Value;
            var storageCredentials = new StorageCredentials(_options.StorageName, _options.StorageKey);
            _storageAccount = new CloudStorageAccount(storageCredentials, true);
            _tableClient = _storageAccount.CreateCloudTableClient();
            _table = _tableClient.GetTableReference(_options.TableName);
        }

        public async Task InitStorageAsync()
        {
            await _table.CreateIfNotExistsAsync();
        }

        public async Task PersistAsync(ICollection<string> entities, string partitionKey)
        {
            var batch = new TableBatchOperation();
            foreach (var url in entities)
            {
                var rowKey = GetRowKey(url);
                var row = await _table.ExecuteAsync(TableOperation.Retrieve(partitionKey, rowKey, new List<string>()));
                if (row.Result == null)
                {
                    var persistedEntity = new PersistedEntity
                    {
                        PartitionKey = partitionKey,
                        RowKey = rowKey,
                        Url = url,
                        Notified = false
                    };
                    batch.InsertOrMerge(persistedEntity);
                }
            }

            if (batch.Any())
            {
                await _table.ExecuteBatchAsync(batch);
            }
        }

        public async Task MarkNotifiedAsync(ICollection<string> entities, string partitionKey)
        {
            var batch = new TableBatchOperation();
            foreach (var url in entities)
            {
                var persistedEntity = new PersistedEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = GetRowKey(url),
                    Url = url,
                    Notified = true
                };
                batch.InsertOrMerge(persistedEntity);
            }

            if (batch.Any())
            {
                await _table.ExecuteBatchAsync(batch);
            }
        }

        public async Task<ICollection<string>> GetUnnotifiedAsync(string partitionKey)
        {
            var entityFilter = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(nameof(PersistedEntity.PartitionKey), QueryComparisons.Equal, partitionKey),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForBool(nameof(PersistedEntity.Notified), QueryComparisons.NotEqual, true));

            var employeeQuery = new TableQuery<PersistedEntity>().Where(entityFilter);
            var resultEntities = new HashSet<string>();
            TableContinuationToken continuationToken = null;
            do
            {
                var entitySegment = await _table.ExecuteQuerySegmentedAsync(employeeQuery, continuationToken);
                resultEntities.UnionWith(entitySegment.Select(x => x.Url));
                continuationToken = entitySegment.ContinuationToken;
            }
            while (continuationToken != null);

            return resultEntities;
        }

        /// <summary>
        /// Because RowKey doesnt support some characters
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/understanding-the-table-service-data-model
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetRowKey(string url)
        {
            if (url == null) return null;
            return url.Replace('/', '.').Replace('\\', '.').Replace('?', '-').Replace('#', '-');
        }
    }
}
