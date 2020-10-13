namespace OneSolution.Core.Storage
{
    public interface IBlobStorageFactory
    {
        IStorageProvider GetProvider(string name);
    }
}