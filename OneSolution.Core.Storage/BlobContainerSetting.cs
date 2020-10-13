namespace OneSolution.Core.Storage
{
    public class BlobContainerSetting
    {
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
        public string Container { get; set; }
        public bool ReadOnly { get; set; }
    }
}
