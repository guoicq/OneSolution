using OneSolution.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ArkTrading.DataAccess
{
    public class ArkTradeExcel97DataAccess : IArkTradeExcelDataAccess
    {
        public ArkTradeExcel97DataAccess()
        {
        }

        public async Task<List<ArkTradingRecord>> Parse(string fileName)
        {
            var skipRows = 4;
            var records = new List<ArkTradingRecord>(); 
            HSSFWorkbook hssfwb;
            using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                hssfwb = new HSSFWorkbook(file);
            }
            DataFormatter formatter = new DataFormatter();
            ISheet sheet = hssfwb.GetSheet("Sheet1");
            var startRow = skipRows;
            for (int i = startRow; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null)
                    continue;
                
                var record = new ArkTradingRecord
                {
                    Fund = row.GetCell(0).StringCellValue,
                    Date = DateTime.Parse(row.GetCell(1).StringCellValue),
                    Direction = Enum.Parse<TradingDirection>(row.GetCell(2).StringCellValue),
                    Ticker = row.GetCell(3).StringCellValue,
                    CusIP = formatter.FormatCellValue(row.GetCell(4)),
                    Name = row.GetCell(5).StringCellValue,
                    Shares = (int)row.GetCell(6).NumericCellValue,
                    PercentOfEtf = row.GetCell(7).NumericCellValue,
                };
                records.Add(record);
            }

            return await Task.FromResult(records);
        }
    }
}
