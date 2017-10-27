using System;

namespace AEI.Sftp.Configuration
{
    public class CertificateClaim : Claim
    {
        public string PrivateKeyFile { get; }
        public string PrivateKeyPassPhrase { get; }

        public CertificateClaim(string username, string privateKeyFile, string privateKeyPassPhrase) : base(username)
        {
            if(string.IsNullOrWhiteSpace(privateKeyFile))
                throw new ArgumentNullException(nameof(privateKeyFile));

            PrivateKeyFile = privateKeyFile;
            PrivateKeyPassPhrase = privateKeyPassPhrase;
        }

        public override string ToString()
        {
            return
                $"Username: {Username} PrivateKeyFile: {PrivateKeyFile} PrivateKeyPassPhrase: {PrivateKeyPassPhrase}";
        }
    }
}