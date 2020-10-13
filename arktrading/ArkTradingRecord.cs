using System;
using System.Collections.Generic;
using System.Text;

namespace ArkTrading
{
    public class ArkTradingRecord
    {
        public RecordType RecordType { get; set; }
        public string Fund { get; set; }
        public DateTime Date { get; set; }
        public TradingDirection Direction { get; set; }
        public string Ticker { get; set; }
        public string CusIP { get; set; }
        public string Name { get; set; }
        public int Shares { get; set; }
        public double PercentOfEtf { get; set; }

    }

    public enum TradingDirection
    {
        Buy,
        Sell,
    }

    public enum RecordType
    {
        Daily,
    }
}
