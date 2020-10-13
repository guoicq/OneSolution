using System.IO;
using System.Threading.Tasks;

namespace OneSolution.Core.Storage
{
    public interface IStorageProvider
    {
        Task<Stream> OpenRead(string fileName);
    }
}