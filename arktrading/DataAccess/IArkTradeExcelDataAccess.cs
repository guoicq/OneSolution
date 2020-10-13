using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArkTrading.DataAccess
{
    public interface IArkTradeExcelDataAccess
    {
        Task<List<ArkTradingRecord>> Parse(string fileName);
    }
}