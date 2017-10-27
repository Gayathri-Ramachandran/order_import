using System;

namespace AEI.Sftp.Configuration
{
    public class PasswordClaim : Claim
    {
        public PasswordClaim(string username, string password) : base(username)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            Password = password;
        }

        public string Password { get; }

        public override string ToString()
        {
            return $"Username: {Username} Password: {Password}";
        }
    }
}