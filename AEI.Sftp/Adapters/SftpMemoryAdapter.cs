using System;
using System.IO;
using Common.Logging;

namespace AEI.Sftp.Adapters
{
    public class SftpMemoryAdapter : ISftpAdapter
    {
        private bool _connected;
        private readonly ILog _log;

        public SftpMemoryAdapter()
        {
            _log = LogManager.GetLogger<SftpFileSystemAdapter>();
        }

        public void Connect()
        {
            if (_connected) return;

            _log.Debug(m => m("Connecting {0}", GetType()));
            _connected = true;
            _log.Info(m => m("Connected {0}", GetType()));
        }

        public void Upload(string source, string target, Func<string, string> renameCallback, bool deleteSourceFile = false)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(target));

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before upload.");

            var random = new Random();
            var totalBytes = (ulong) random.Next(1024 * 10, 1024 * 1024);
            ulong bytes = 0;
            while (bytes < totalBytes)
            {
                _log.Debug(m => m("Uploading {0} to {1}", source, target));
                var uploaded = (ulong) random.Next(1024, 2048);
                if (bytes + uploaded < totalBytes)
                {
                    bytes += uploaded;
                }
                else
                {
                    bytes = totalBytes;
                }

                if (bytes < totalBytes)
                {
                    _log.Debug(m => m("Uploading {0} to {1} {2}/{3}", source, target, bytes, totalBytes));
                }
                else
                {
                    _log.Info(m => m("Upload {0} to {1} completed.", source, target));
                }
                Uploading?.Invoke(target, bytes, totalBytes);
                renameCallback?.Invoke(target);
            }

            if (!deleteSourceFile) return;

            _log.Debug($"{source} deleted.");
        }

        public void Download(string source, string target, bool deleteSourceFile = false)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(target));

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before download.");
        }

        public void Disconnect()
        {
            if (!_connected) return;

            _log.Debug(m => m("Disconnecting {0}", GetType()));
            _connected = false;
            _log.Info(m => m("Disconnected {0}", GetType()));
        }

        public string[] ListFiles(string path, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected => _connected;
        public event UploadingHandler Uploading;
        public event DownloadingHandler Downloading;

        public void Dispose()
        {
        }
    }
}
