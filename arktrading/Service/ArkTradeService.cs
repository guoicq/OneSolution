using ArkTrading.DataAccess;
using Microsoft.Extensions.Configuration;
using OneSolution.Core.Log;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArkTrading.Service
{
    public class ArkTradeService : IArkTradeService
    {
        private readonly IConfiguration configuration;
        private readonly IArkTradeDataAccess tradeDataAccess;
        private readonly IArkTradeExcelDataAccess tradeDataExcelAccess;
        public ArkTradeService(IConfiguration configuration, IArkTradeDataAccess tradeDataAccess, IArkTradeExcelDataAccess tradeDataExcelAccess)
        {
            this.tradeDataAccess = tradeDataAccess;
            this.configuration = configuration;
            this.tradeDataExcelAccess = tradeDataExcelAccess;
        }

        public async Task ProcessFiles()
        {
            var folder = configuration.GetValue<string>("ArkTrading.Folder");
            var filePattern = configuration.GetValue<string>("ArkTrading.FilePattern");
            DirectoryInfo d = new DirectoryInfo(folder);
            // ARK_Trade_10092020_0600PM_EST_5f80d45bec705.xls
            FileInfo[] files = d.GetFiles(filePattern);
            Log.Information($"{files.Length} file(s) found.");
            foreach (FileInfo file in files)
            {

                var filename = file.FullName;
                Log.Information($"Process file {filename}");
                await ProcessFile(filename).ConfigureAwait(false);
                var newName = $"{folder}\\Processed_{file.Name}";
                File.Move(filename, newName);
            }

        }

        private async Task ProcessFile(string filename)
        {
            var records = await tradeDataExcelAccess.Parse(filename).ConfigureAwait(false);
            Log.Information($"{records.Count} records found in file {filename}");
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
