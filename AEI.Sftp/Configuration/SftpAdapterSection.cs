using System.Configuration;

namespace AEI.Sftp.Configuration
{
    public class SftpAdapterSection : ConfigurationSection
    {
        public const string SectionName = "SftpAdapterSection";

        private const string ProvidersCollectionName = "Adapters";

        [ConfigurationProperty(ProvidersCollectionName)]
        [ConfigurationCollection(typeof(SftpAdaptersCollection), AddItemName = "adapter")]
        public SftpAdaptersCollection SftpAdapters => (SftpAdaptersCollection)base[ProvidersCollectionName];
    }
}