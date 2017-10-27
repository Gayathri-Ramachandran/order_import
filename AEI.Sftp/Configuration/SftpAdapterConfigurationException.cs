using System;
using System.Runtime.Serialization;

namespace AEI.Sftp.Configuration
{
    [Serializable]
    public class SftpAdapterConfigurationException : Exception
    {
        public SftpAdapterConfigurationException()
        {
        }

        public SftpAdapterConfigurationException(string message) : base(message)
        {
        }

        public SftpAdapterConfigurationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SftpAdapterConfigurationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}