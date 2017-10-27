using System;
using System.IO;

namespace AEI.Sftp.Callbacks
{
    public class SftpCallbacks
    {
        public static readonly Func<string, string> NullCallback = input => input;
        public static readonly Func<string, string> StripFinalExtensionCallback = input =>
        {
            if (string.IsNullOrWhiteSpace(input) || input.IndexOf(".", StringComparison.Ordinal) < 0) return input;

            var indexOf = input.LastIndexOf(Path.GetExtension(input), StringComparison.Ordinal);
            return indexOf < 0 ? input : input.Remove(indexOf);
        };
    }
}