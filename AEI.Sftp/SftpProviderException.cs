using System;
using System.Runtime.Serialization;

namespace AEI.Sftp
{
    [Serializable]
    public class SftpProviderException : Exception
    {
        public SftpProviderException()
        {
        }

        public SftpProviderException(string message) : base(message)
        {
        }

        public SftpProviderException(string message, Exception inner) : base(message, inner)
        {
        }

        protected SftpProviderException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}