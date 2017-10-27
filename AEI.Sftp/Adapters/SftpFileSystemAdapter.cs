using System;
using System.IO;
using Common.Logging;

namespace AEI.Sftp.Adapters
{
    public class SftpFileSystemAdapter : ISftpAdapter
    {
        private bool _connected;
        private readonly ILog _log;

        public SftpFileSystemAdapter()
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

            if (!File.Exists(source))
                throw new SftpProviderException($"{source} does not exist.");

            if (string.CompareOrdinal(source, target) == 0)
                throw new SftpProviderException("Target cannot be same as source.");

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before upload.");

            try
            {
                if (target.IndexOf("\\", StringComparison.InvariantCulture) != -1)
                    Directory.CreateDirectory(Path.GetDirectoryName(target));

                _log.Debug(m => m("Uploading {0} to {1}", source, target));
                var buffer = new byte[2048];
                ulong bytes = 0;

                using (var fileStream = File.Open(source, FileMode.Open))
                {
                    var totalBytes = (ulong)fileStream.Length;

                    if (totalBytes == 0)
                    {
                        File.Create(target).Close();
                        Uploading?.Invoke(target, 0, 0);
                        _log.Info(m => m("Upload {0} to {1} completed.", source, target));
                    }
                    else
                    {
                        using (var targetFileStream = File.Create(target))
                        {
                            long bytesRead;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bytes += (ulong) bytesRead;
                                targetFileStream.Write(buffer, 0, buffer.Length);

                                _log.Debug(m => m("Uploading {0} to {1} {2}/{3}", source, target, bytes, totalBytes));
                                Uploading?.Invoke(target, bytes, totalBytes);
                            }
                            _log.Info(m => m("Upload {0} to {1} completed.", source, target));
                            Uploading?.Invoke(target, bytes, totalBytes);

                            targetFileStream.Close();
                        }
                        fileStream.Close();

                        if (renameCallback != null)
                        {
                            File.Move(target, renameCallback(target));
                        }

                        if (!deleteSourceFile) return;

                        File.Delete(source);
                        _log.Debug($"{source} deleted.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new SftpProviderException(e.Message, e);
            }
        }

        public void Download(string source, string target, bool deleteSourceFile = false)
        {
            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(target));

            if (!File.Exists(source))
                throw new SftpProviderException($"{source} does not exist.");

            if (string.CompareOrdinal(source, target) == 0)
                throw new SftpProviderException("Target cannot be same as source.");

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before download.");

            try
            {
                if (target.IndexOf("\\", StringComparison.InvariantCulture) != -1)
                    Directory.CreateDirectory(Path.GetDirectoryName(target));

                _log.Debug(m => m("Downloading {0} to {1}", source, target));
                var buffer = new byte[2048];
                ulong bytes = 0;

                using (var fileStream = File.Open(source, FileMode.Open))
                {
                    var totalBytes = (ulong)fileStream.Length;

                    if (totalBytes == 0)
                    {
                        File.Create(target).Close();
                        Downloading?.Invoke(target, 0, 0);
                        _log.Info(m => m("Download {0} to {1} completed.", source, target));
                    }
                    else
                    {
                        using (var targetFileStream = File.Create(target))
                        {
                            long bytesRead;
                            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                bytes += (ulong)bytesRead;
                                targetFileStream.Write(buffer, 0, buffer.Length);

                                _log.Debug(m => m("Downloading {0} to {1} {2}/{3}", source, target, bytes, totalBytes));
                                Downloading?.Invoke(target, bytes, totalBytes);
                            }
                            _log.Info(m => m("Download {0} to {1} completed.", source, target));
                            Downloading?.Invoke(target, bytes, totalBytes);

                            targetFileStream.Close();
                        }
                        fileStream.Close();

                        if (!deleteSourceFile) return;

                        File.Delete(source);
                        _log.Debug($"{source} deleted.");
                    }
                }
            }
            catch (Exception e)
            {
                throw new SftpProviderException(e.Message, e);
            }
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
            return Directory.GetFiles(path, "*.*", searchOption);
        }

        public bool IsConnected => _connected;
        public event UploadingHandler Uploading;
        public event DownloadingHandler Downloading;

        public void Dispose()
        {
        }
    }
}