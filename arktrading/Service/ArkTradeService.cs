using ArkTrading.DataAccess;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
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
            DirectoryInfo d = new DirectoryInfo(folder);
            // ARK_Trade_10092020_0600PM_EST_5f80d45bec705.xls
            FileInfo[] Files = d.GetFiles("ARK_Trade_*.xls");
            foreach (FileInfo file in Files)
            {

                var filename = file.FullName;
                Console.WriteLine($"Process file {filename}");
                await ProcessFile(filename).ConfigureAwait(false);
                var newName = $"{folder}\\Processed_{file.Name}";
                File.Move(filename, newName);
            }

        }

        private async Task ProcessFile(string filename)
        {
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
