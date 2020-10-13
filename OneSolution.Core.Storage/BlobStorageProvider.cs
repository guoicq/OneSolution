using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace OneSolution.Core.Storage
{
    public class BlobStorageProvider : IStorageProvider
    {
        private readonly BlobContainerClient containerClient;

        public BlobStorageProvider(BlobContainerSetting setting)
        {
            var connectionString = BuildConnectionString(setting.AccountName, setting.AccountKey);
            var blobServiceClient = new BlobServiceClient(connectionString);
            containerClient = blobServiceClient.GetBlobContainerClient(setting.Container);
        }

        public async Task<Stream> OpenRead(string fileName)
        {
            var blobClient = containerClient.GetBlobClient(fileName);
            BlobDownloadInfo download = await blobClient.DownloadAsync().ConfigureAwait(false);
            return download.Content;
        }

        private static string BuildConnectionString(string accountName, string storageKey)
        {
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=" + accountName + ";AccountKey=" + storageKey;
            return connectionString;
        }
    }
}
