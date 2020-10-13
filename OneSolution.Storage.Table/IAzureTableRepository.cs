using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos.Table;

namespace OneSolution.Storage.Table
{
    public interface IAzureTableRepository<T> where T : TableEntity, new()
    {
        Task Delete(IEnumerable<T> entities);
        Task Delete(T entity);
        Task Delete(string partitionKey, string rowKeyStart = null, string rowKeyEnd = null);
        Task<T> Get(string partitionKey, string rowKey, List<string> selectColumns = null);
        Task InsertOrMerge(IEnumerable<T> entities);
        Task InsertOrMerge(T entity);
        Task<IList<T>> Query(string partitionKey, string rowKeyStart = null, string rowKeyEnd = null, int? count = null, IList<string> selectColumns = null);
        Task<(IList<T>, string)> QuerySegmented(string partitionKey, string rowKeyStart = null, string rowKeyEnd = null, int? count = null, IList<string> selectColumns = null, string continuationToken = null);
    }
}