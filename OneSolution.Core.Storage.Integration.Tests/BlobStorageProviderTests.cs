using System.Threading.Tasks;
using Xunit;

namespace OneSolution.Core.Storage.Integration.Tests
{
    public class BlobStorageProviderTests
    {
        private readonly BlobContainerSetting blobContainerSetting;

        public BlobStorageProviderTests()
        {
            blobContainerSetting = ConfigBuilder.GetRadioPPMBlobContainerSetting();
        }

        private BlobStorageProvider CreateProvider()
        {
            return new BlobStorageProvider(blobContainerSetting);
        }

        [Fact]
        public async Task Should_Able_To_OpenRead_NetworkReference_BlobContent()
        {
            // Arrange
            var provider = CreateProvider();
            var fileName = "Reference/NetworkReference.bin";

            // Act
            var result = await provider.OpenRead(fileName);

            // Assert
            Assert.True(result.CanRead);
        }
    }
}
