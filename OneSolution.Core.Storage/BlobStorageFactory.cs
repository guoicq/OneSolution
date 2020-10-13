using System.Collections.Generic;

namespace OneSolution.Core.Storage
{
    public class BlobStorageFactory : IBlobStorageFactory
    {
        private readonly StorageSettings settings;
        private static readonly Dictionary<string, IStorageProvider> providers = new Dictionary<string, IStorageProvider>();

        public BlobStorageFactory(StorageSettings settings)
        {
            this.settings = settings;
        }

        public IStorageProvider GetProvider(string name)
        {
            if (providers.TryGetValue(name, out var storageProvider))
                return storageProvider;

            lock (providers)
            {
                if (providers.TryGetValue(name, out storageProvider))
                    return storageProvider;

                storageProvider = new BlobStorageProvider(settings[name]);
                providers.Add(name, storageProvider);
            }

            return storageProvider;
        }
    }
}
