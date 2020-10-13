using ArkTrading.DataAccess;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArkTrading.Service
{
    public class ArkTradeService : IArkTradeService
    {
        private readonly IArkTradeDataAccess tradeDataAccess;
        private readonly IArkTradeExcelDataAccess tradeDataExcelAccess;
        public ArkTradeService(IArkTradeDataAccess tradeDataAccess, IArkTradeExcelDataAccess tradeDataExcelAccess)
        {
            this.tradeDataAccess = tradeDataAccess;
            this.tradeDataExcelAccess = tradeDataExcelAccess;
        }

        public async Task ProcessFiles(string folder = @"C:\Users\cguo\downloads")
        {
            var filename = $"{folder}\\ARK_Trade_10092020_0600PM_EST_5f80d45bec705.xls";
            filename = $"{folder}\\ARK_Trade_10122020_0600PM_EST_5f84ced99bb73.xls";
            //filename = $"{folder}\\book3.xls";
            var records = await tradeDataExcelAccess.Parse(filename);
            if (records.Count > 0)
                await RemoveDailyRecords(records[0].Date).ConfigureAwait(false);
            records.ForEach(r =>
            {
                r.RecordType = RecordType.Daily;
            });
            await tradeDataAccess.Save(records).ConfigureAwait(false);

        }

        private async Task RemoveDailyRecords(DateTime date)
        {
            var keyStart = RowKeyHelper.ToRowKeyPrefix(date.AddDays(1).AddTicks(-1)); // latest at top
            var keyEnd = RowKeyHelper.ToRowKeyPrefix(date.AddTicks(-1));
            await tradeDataAccess.Delete(RecordType.Daily, keyStart, keyEnd).ConfigureAwait(false);
        }
    }
}
