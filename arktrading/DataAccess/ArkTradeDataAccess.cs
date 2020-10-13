using OneSolution.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkTrading.DataAccess
{
    public class ArkTradeDataAccess : IArkTradeDataAccess
    {
        private readonly IAzureTableRepository<ArkTradingEntity> repository;
        public ArkTradeDataAccess(IAzureTableRepository<ArkTradingEntity> repository)
        {
            this.repository = repository;
        }

        public async Task<(IList<ArkTradingRecord>, string)> Get(string partitionKey, string dateStart, string dateEnd, int count = 1000, string continuationToken = null)
        {
            var result = await repository.QuerySegmented(partitionKey, dateStart, dateEnd, count, null, continuationToken).ConfigureAwait(false);
            var list = new List<ArkTradingRecord>();
            if (result.Item1 != null)
                foreach (var item in result.Item1)
                    list.Add(item.ToArkTradingRecord());
            return (list, result.Item2);

        }
        public async Task Save(ArkTradingRecord record)
        {
            var entity = ArkTradingEntity.FromArkTradingRecord(record);

            await repository.InsertOrMerge(entity).ConfigureAwait(false);
        }
        public async Task Save(IList<ArkTradingRecord> stats)
        {
            var list = new List<ArkTradingEntity>();
            foreach (var item in stats)
                list.Add(ArkTradingEntity.FromArkTradingRecord(item));

            await repository.InsertOrMerge(list).ConfigureAwait(false);
        }

        public async Task Delete(RecordType recordType, string dateStart, string dateEnd)
        {
            await repository.Delete(recordType.ToString().ToLowerInvariant(), dateStart, dateEnd).ConfigureAwait(false);
        }
    }
}
