using System.Threading.Tasks;

namespace ArkTrading.Service
{
    public interface IArkTradeService
    {
        Task ProcessFiles(string folder = "C:\\Users\\cguo\\downloads");
    }
}