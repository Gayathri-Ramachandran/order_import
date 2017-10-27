using System;

namespace AEI.Sftp.Configuration
{
    public class Claim
    {
        public Claim(string username)
        {
            if(string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; }
    }
}