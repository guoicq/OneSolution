using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;
using ArkTrading;

namespace ArkTrading.DataAccess
{
    public class ArkTradingEntity : TableEntity
    {
        public string RecordType { get; set; }
        public string Fund { get; set; }
        public DateTime Date { get; set; }
        public string Direction { get; set; }
        public string Ticker { get; set; }
        public string CusIP { get; set; }
        public string Name { get; set; }
        public int Shares { get; set; }
        public double PercentOfEtf { get; set; }

        public ArkTradingRecord ToArkTradingRecord()
        {
            return new ArkTradingRecord
            {
                RecordType = Enum.Parse<RecordType>(this.RecordType),
                Fund = this.Fund,
                Date = this.Date,
                Direction = Enum.Parse<TradingDirection>(this.Direction),
                Ticker = this.Ticker,
                CusIP = this.CusIP,
                Name = this.Name,
                Shares = this.Shares,
                PercentOfEtf = this.PercentOfEtf,
            };
        }
        public static ArkTradingEntity FromArkTradingRecord(ArkTradingRecord record)
        {
            return new ArkTradingEntity
            {
                PartitionKey = record.RecordType.ToString().ToLowerInvariant(),
                RowKey = RowKeyHelper.ToRowKey(record.Date),
                RecordType = record.RecordType.ToString(),
                Fund = record.Fund,
                Date = record.Date,
                Direction = record.Direction.ToString(),
                Ticker = record.Ticker,
                CusIP = record.CusIP,
                Name = record.Name,
                Shares = record.Shares,
                PercentOfEtf = record.PercentOfEtf,
            };
        }
    }


}
