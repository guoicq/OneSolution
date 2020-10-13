using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Azure.Cosmos.Table;

namespace OneSolution.Storage.Table
{
    public class AzureTableRepository<T> : IAzureTableRepository<T> where T : TableEntity, new()
    {
        private const int DefaultPageSize = 1000;
        private int batchSize = 100;
        private CloudTable table;

        public AzureTableRepository(AzureTableSetting tableSetting)
        {
            var storageAccount = new CloudStorageAccount(new StorageCredentials(tableSetting.AccountName, tableSetting.AccountKey), true);
            var tableClient = storageAccount.CreateCloudTableClient();

            table = tableClient.GetTableReference(tableSetting.TableName);
            table.CreateIfNotExistsAsync().Wait();
        }

        public async Task InsertOrMerge(T entity)
        {
            // Create the TableOperation that inserts the customer entity.
            var insertOperation = TableOperation.InsertOrMerge(entity);
            var result = await table.ExecuteAsync(insertOperation).ConfigureAwait(false);
        }

        public async Task InsertOrMerge(IEnumerable<T> entities)
        {
            var count = 0;
            var batchOperation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                batchOperation.InsertOrMerge(entity);
                count++;

                if (count == batchSize)
                {
                    await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
                    count = 0;
                    batchOperation = new TableBatchOperation();
                }
            }

            // Last batch
            if (count > 0)
                await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);

        }

        public async Task Delete(T entity)
        {
            entity.ETag = "*";
            var operation = TableOperation.Delete(entity);
            await table.ExecuteAsync(operation).ConfigureAwait(false);
        }

        public async Task Delete(IEnumerable<T> entities)
        {
            var count = 0;
            var batchOperation = new TableBatchOperation();
            foreach (var entity in entities)
            {
                entity.ETag = "*";
                batchOperation.Delete(entity);
                count++;

                if (count == batchSize)
                {
                    await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
                    count = 0;
                    batchOperation = new TableBatchOperation();
                }
            }

            // Last batch
            if (count > 0)
                await table.ExecuteBatchAsync(batchOperation).ConfigureAwait(false);
        }

        public async Task Delete(string partitionKey, string rowKeyStart = null, string rowKeyEnd = null)
        {
            IList<string> selectColumns = new List<string> { "PartitionKey", "RowKey" };
            var rangeQuery = CreateQuery(partitionKey, rowKeyStart, rowKeyEnd, selectColumns);
            TableContinuationToken continuationToken = null;

            do
            {
                var retrievedResults = await table.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken).ConfigureAwait(false);

                continuationToken = retrievedResults.ContinuationToken;

                var results = new List<T>();
                foreach (var record in retrievedResults)
                    results.Add(record);

                if (results.Count > 0)
                    await Delete(results).ConfigureAwait(false);
            } while (continuationToken != null);
        }

        public async Task<T> Get(string partitionKey, string rowKey, List<string> selectColumns = null)
        {
            var operation = TableOperation.Retrieve<T>(partitionKey, rowKey, selectColumns);
            var result = await table.ExecuteAsync(operation).ConfigureAwait(false);
            return result.Result as T;
        }

        public async Task<IList<T>> Query(string partitionKey, string rowKeyStart = null, string rowKeyEnd = null, int? count = null, IList<string> selectColumns = null)
        {
            var rangeQuery = CreateQuery(partitionKey, rowKeyStart, rowKeyEnd, selectColumns);

            var results = await ExexuteQuery(table, rangeQuery, count).ConfigureAwait(false);
            return results.Item1;
        }

        public async Task<(IList<T>, string)> QuerySegmented(string partitionKey, string rowKeyStart = null, string rowKeyEnd = null, int? count = null, IList<string> selectColumns = null, string continuationToken = null)
        {
            var rangeQuery = CreateQuery(partitionKey, rowKeyStart, rowKeyEnd, selectColumns);

            var results = await ExexuteQuery(table, rangeQuery, count, continuationToken == null ? null : JsonSerializer.Deserialize<TableContinuationToken>(continuationToken)).ConfigureAwait(false);
            return (results.Item1, results.Item2 == null ? null : JsonSerializer.Serialize(results.Item2));
        }

        private static TableQuery<T> CreateQuery(string partitionKey, string rowKeyStart, string rowKeyEnd, IList<string> selectColumns)
        {
            var query = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            if (!(rowKeyStart == null && rowKeyEnd == null))
                query = TableQuery.CombineFilters(query, TableOperators.And, TableQueryHelper.GenerateFilterBetween("RowKey", rowKeyStart, rowKeyEnd));
            var rangeQuery = new TableQuery<T>().Where(query);
            if (selectColumns != null && selectColumns.Count > 0)
                rangeQuery = rangeQuery.Select(selectColumns);
            return rangeQuery;
        }

        private static async Task<(IList<T>, TableContinuationToken)> ExexuteQuery(CloudTable table, TableQuery<T> rangeQuery, int? count = null, TableContinuationToken continuationToken = null)
        {
            var results = new List<T>();
            do
            {
                if (count.HasValue)
                {
                    var takeCount = count - results.Count;
                    if (takeCount > DefaultPageSize)   // Max page is 1000
                        takeCount = DefaultPageSize;
                    rangeQuery = rangeQuery.Take(takeCount);
                }

                var retrievedResults = await table.ExecuteQuerySegmentedAsync(rangeQuery, continuationToken).ConfigureAwait(false);

                continuationToken = retrievedResults.ContinuationToken;

                foreach (var record in retrievedResults)
                    results.Add(record);

                if (count.HasValue && results.Count >= count.Value)
                    break;
            } while (continuationToken != null);
            return (results, continuationToken);
        }

    }
}
