using System;
using System.Configuration;

namespace AEI.Sftp.Configuration
{
    public class SftpAdapterConfigurationReader
    {
        protected System.Configuration.Configuration _config;
        public SftpAdapterConfiguration Get(string sftpAdapterName)
        {
            if (string.IsNullOrWhiteSpace(sftpAdapterName))
                throw new ArgumentNullException(nameof(sftpAdapterName));

            var section = _config != null
                ? (SftpAdapterSection)_config.GetSection(SftpAdapterSection.SectionName)
                : (SftpAdapterSection)ConfigurationManager.GetSection(SftpAdapterSection.SectionName);

            if (section == null)
                throw new ConfigurationErrorsException($"Configuration Section {SftpAdapterSection.SectionName} is not set.");

            var elementConfiguration = section.SftpAdapters[sftpAdapterName];

            if(elementConfiguration == null)
                throw new SftpAdapterConfigurationException($"The adapter {sftpAdapterName} was not found");

            var config = new SftpAdapterConfiguration
            {
                Host = elementConfiguration.Host,
                Port = elementConfiguration.Port,
                Claim = elementConfiguration.Claim
            };

            return config;
        }
    }

    public class SftpAdapterConfiguration
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public Claim Claim { get; set; }
    }
}