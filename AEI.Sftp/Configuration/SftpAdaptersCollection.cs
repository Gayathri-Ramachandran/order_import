using System.Configuration;

namespace AEI.Sftp.Configuration
{
    public class SftpAdaptersCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new SftpAdapterElement();
        }

        public new SftpAdapterElement this[string name] => (SftpAdapterElement)BaseGet(name);

        public void Add(SftpAdapterElement element)
        {
            BaseAdd(element);
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((SftpAdapterElement)element).Name;
        }
    }
}