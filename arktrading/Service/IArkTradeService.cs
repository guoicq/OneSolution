using System.Threading.Tasks;

namespace ArkTrading.Service
{
    public interface IArkTradeService
    {
        Task ProcessFiles();
    }
}