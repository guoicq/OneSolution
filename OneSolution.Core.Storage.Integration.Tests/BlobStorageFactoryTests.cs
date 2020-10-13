using Xunit;

namespace OneSolution.Core.Storage.Integration.Tests
{
    public class BlobStorageFactoryTests
    {
        private readonly StorageSettings storageSettings;

        public BlobStorageFactoryTests()
        {
            storageSettings = ConfigBuilder.GetStorageSettings();
        }

        private BlobStorageFactory CreateFactory()
        {
            return new BlobStorageFactory(storageSettings);
        }

        [Fact]
        public void Should_Able_To_GetProvider()
        {
            // Arrange
            var factory = this.CreateFactory();
            var name = "RadioPPMBlob";

            // Act
            var result = factory.GetProvider(name);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Should_Able_To_Existing_GetProvider()
        {
            // Arrange
            var factory = this.CreateFactory();
            var name = "RadioPPMBlob";
            var initialResult = factory.GetProvider(name);

            // Act
            var finalResult = factory.GetProvider(name);

            // Assert
            Assert.True(initialResult == finalResult);
        }
    }
}
