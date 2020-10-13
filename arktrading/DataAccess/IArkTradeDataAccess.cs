using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArkTrading.DataAccess
{
    public interface IArkTradeDataAccess
    {
        Task Delete(RecordType recordType, string dateStart, string dateEnd);
        Task<(IList<ArkTradingRecord>, string)> Get(string partitionKey, string dateStart, string dateEnd, int count = 1000, string continuationToken = null);
        Task Save(ArkTradingRecord record);
        Task Save(IList<ArkTradingRecord> stats);
    }
}