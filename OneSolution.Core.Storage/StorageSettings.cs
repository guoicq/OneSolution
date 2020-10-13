using System.Collections.Generic;

namespace OneSolution.Core.Storage
{
    public class StorageSettings: Dictionary<string, BlobContainerSetting>
    {
    }

    public class StorageSettingAccessor
    {
        private StorageSettings settings;
        public StorageSettingAccessor(StorageSettings storageSettings)
        {
            settings = storageSettings;
        }

        public BlobContainerSetting this[string name]
        {
            get {  return settings [name];  }
        }
    }
}
