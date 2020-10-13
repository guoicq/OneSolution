using OneSolution.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using System.Linq;

namespace ArkTrading.DataAccess
{
    public class ArkTradeExcelDataAccess : IArkTradeExcelDataAccess
    {
        public ArkTradeExcelDataAccess()
        {
        }

        public async Task<List<ArkTradingRecord>> Parse(string fileName)
        {
            var records = new List<ArkTradingRecord>();
            using (var package = new ExcelPackage())
            {
                using (var stream = File.OpenRead(fileName))
                {
                    package.Load(stream);
                }
                var ws = package.Workbook.Worksheets[0];
                var skipRows = 4;
                var startRow = skipRows + 1;
                for (var i = startRow; i <= ws.Dimension.End.Row; i++)
                {
                    var record = new ArkTradingRecord
                    {
                        Fund = ws.Cells[i, 1].Text,
                        Date = DateTime.Parse(ws.Cells[i, 2].Text),
                        Direction = Enum.Parse<TradingDirection>(ws.Cells[i, 3].Text),
                        Ticker = ws.Cells[i, 1].Text,
                        CusIP = ws.Cells[i, 1].Text,
                        Name = ws.Cells[i, 1].Text,
                        Shares = int.Parse(ws.Cells[i, 1].Text),
                        PercentOfEtf = double.Parse(ws.Cells[i, 1].Text),
                    };
                    records.Add(record);
                }
            }
            return await Task.FromResult(records);
        }
    }
}
