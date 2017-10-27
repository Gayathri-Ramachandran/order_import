using System.Configuration;
using System.Xml;

namespace AEI.Sftp.Configuration
{
    public class SftpAdapterElement : ConfigurationElement
    {
        private Claim _claim;
        public Claim Claim => _claim;

        [ConfigurationProperty("name", IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            {
                this["name"] = value;
            }
        }

        [ConfigurationProperty("host", IsRequired = true)]
        public string Host
        {
            get { return (string)this["host"]; }
            set { this["host"] = value; }
        }

        [ConfigurationProperty("port", IsRequired = true)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, XmlReader reader)
        {
            if (elementName == "passwordClaim")
            {
                var username = reader.GetAttribute("username");
                if (username == null)
                    throw new SftpAdapterConfigurationException("Missing attribute username for passwordClaim.");

                var password = reader.GetAttribute("password");
                if (password == null)
                    throw new SftpAdapterConfigurationException("Missing attribute password for passwordClaim.");

                if (_claim != null)
                    throw new SftpAdapterConfigurationException($"Cannot set more than one claim ({_claim})");

                _claim = new PasswordClaim(username, password);

                return true;
            }

            if (elementName == "certificateClaim")
            {
                var username = reader.GetAttribute("username");
                if (username == null)
                    throw new SftpAdapterConfigurationException($"Missing attribute username for certificateClaim.");

                var privateKeyFile = reader.GetAttribute("privateKeyFile");
                if (privateKeyFile == null)
                    throw new SftpAdapterConfigurationException($"Missing attribute privateKeyFile for certificateClaim.");

                var privateKeyPassPhrase = reader.GetAttribute("privateKeyPassPhrase");

                if (_claim != null)
                    throw new SftpAdapterConfigurationException($"Cannot set more than one claim ({_claim})");

                _claim = new CertificateClaim(username, privateKeyFile, privateKeyPassPhrase);

                return true;
            }
            return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }
    }
}