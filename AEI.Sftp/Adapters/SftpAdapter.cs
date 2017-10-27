using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using AEI.Sftp.Configuration;
using Common.Logging;
using Renci.SshNet;

namespace AEI.Sftp.Adapters
{
    public class SftpAdapter : ISftpAdapter
    {
        private readonly string _host;
        private readonly SftpClient _client;
        private bool _disposed;
        private readonly ILog _log = LogManager.GetLogger<SftpAdapter>();
        private readonly object _createDirLock = new object();
        private readonly SftpAdapterConfigurationReader _reader = new SftpAdapterConfigurationReader();

        public SftpAdapter(string adapterName)
        {
            var config = _reader.Get(adapterName);

            _host = config.Host;

            if (config.Claim is PasswordClaim)
            {
                var passwordClaim = (PasswordClaim) config.Claim;
                _client = new SftpClient(config.Host, config.Port, passwordClaim.Username, passwordClaim.Password);
            }
            else
            {
                var certificateClaim = (CertificateClaim)config.Claim;
                _client = new SftpClient(config.Host, config.Port, certificateClaim.Username, new PrivateKeyFile(certificateClaim.PrivateKeyFile, certificateClaim.PrivateKeyPassPhrase));
            }
        }

        ~SftpAdapter()
        {
            Dispose(false);
        }

        public void Connect()
        {
            try
            {
                if (_client.IsConnected) return;

                _log.Debug(m => m("Connecting to {0}", _host));
                _client.Connect();
                _log.Debug(m => m("Connected to {0}", _host));
            }
            catch (Exception e)
            {
                throw new SftpProviderException(e.Message, e);
            }
        }

        public void Upload(string source, string target, Func<string, string> renameCallback, bool deleteSourceFile = false)
        {
            if(string.IsNullOrWhiteSpace(source))
                throw new ArgumentNullException(nameof(source));

            if (string.IsNullOrWhiteSpace(target))
                throw new ArgumentNullException(nameof(target));

            if (!File.Exists(source))
                throw new SftpProviderException($"{source} does not exist.");

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before upload.");

            var targetPath = target;

            try
            {
                CreateDirectory(targetPath);

                var completed = new ManualResetEvent(false);

                using (var fileStream = File.Open(source, FileMode.Open))
                {
                    _log.Debug(m => m("Uploading {0} to {1}", source, _host));

                    var totalBytes = (ulong)fileStream.Length;

                    if (totalBytes == 0)
                    {
                        if (!_client.Exists(target))
                        {
                            _client.Create(target).Close();
                        }
                        Uploading?.Invoke(target, 0, 0);

                        _log.Info(m => m("Upload {0} to {1} completed.", source, _host));
                    }
                    else
                    {
                        _client.UploadFile(fileStream, target, bytes =>
                        {
                            Uploading?.Invoke(target, bytes, totalBytes);

                            if (bytes == totalBytes)
                            {
                                _log.Info(m => m("Upload {0} to {1} completed.", source, _host));
                                completed.Set();
                            }
                            else
                            {
                                _log.Debug(m => m("Uploading {0} to {1} {2}/{3}", source, _host, bytes, totalBytes));
                            }
                        });

                        completed.WaitOne();
                    }

                    fileStream.Close();

                    if (renameCallback != null)
                    {
                        var renamedFile = renameCallback(targetPath);
                        if (string.CompareOrdinal(targetPath, renamedFile) != 0)
                            _client.RenameFile(targetPath, renamedFile);
                    }

                    if (!deleteSourceFile) return;

                    File.Delete(source);
                    _log.Debug($"{source} deleted.");
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

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before download.");

            if (_client.Get(source) == null)
                throw new SftpProviderException($"{source} does not exist.");

            try
            {
                var completed = new ManualResetEvent(false);

                _log.Debug(m => m("Downloading {0} to {1}", source, target));

                var targetDirectory = Path.GetDirectoryName(target);
                if (!string.IsNullOrEmpty(targetDirectory)) Directory.CreateDirectory(targetDirectory);
                using (var fileStream = File.Open(target, FileMode.Create))
                {
                    var totalBytes = (ulong) _client.Get(source).Length;

                    if (totalBytes == 0)
                    {
                        if (!File.Exists(target))
                        {
                            File.Create(target);
                        }
                        Downloading?.Invoke(target, 0, 0);
                        _log.Info(m => m("Download {0} to {1} completed.", source, target));
                    }
                    else
                    {
                        _client.DownloadFile(source, fileStream, bytes =>
                        {
                            Downloading?.Invoke(target, bytes, totalBytes);

                            if (bytes == totalBytes)
                            {
                                _log.Info(m => m("Download {0} to {1} completed.", source, target));
                                completed.Set();
                            }
                            else
                            {
                                _log.Debug(m => m("Downloading {0} to {1} {2}/{3}", source, target, bytes, totalBytes));
                            }
                        });

                        completed.WaitOne();
                    }

                    fileStream.Close();
                }

                if (!deleteSourceFile) return;

                _client.DeleteFile(source);
                _log.Debug($"{source} deleted.");
            }
            catch (Exception e)
            {
                throw new SftpProviderException(e.Message, e);
            }
        }

        private void CreateDirectory(string targetPath)
        {
            lock (_createDirLock)
            {
                while (targetPath.IndexOf("\\", StringComparison.Ordinal) != -1)
                {
                    var directory = targetPath.Substring(0, targetPath.IndexOf("\\", StringComparison.Ordinal));

                    if (!_client.Exists(directory)) _client.CreateDirectory(directory);

                    _client.ChangeDirectory(directory);

                    targetPath = targetPath.Substring(targetPath.IndexOf("\\", StringComparison.Ordinal) + 1,
                        targetPath.Length - targetPath.IndexOf("\\", StringComparison.Ordinal) - 1);
                }
                _client.ChangeDirectory("/");
            }
        }

        public void Disconnect()
        {
            try
            {
                if (!_client.IsConnected) return;

                _log.Debug(m => m("Disconnecting from {0}", _host));
                if (_client.IsConnected) _client.Disconnect();
                _log.Info(m => m("Disconnected from {0}", _host));
            }
            catch (Exception e)
            {
                throw new SftpProviderException(e.Message, e);
            }
        }

        public string[] ListFiles(string path, SearchOption searchOption)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            if (!IsConnected)
                throw new SftpProviderException("Connect to server before list files.");

            var files = new List<string>();

            var sftpFiles = _client.ListDirectory(path).Where(f => f.Name != "." && f.Name != "..");

            files.AddRange(sftpFiles.Where(s => !s.IsDirectory).Select(s => s.FullName).ToArray());

            if (searchOption == SearchOption.AllDirectories)
            {
                foreach (var directory in sftpFiles.Where(s => s.IsDirectory && s.Name != "." && s.Name != ".."))
                {
                    files.AddRange(ListFiles(directory.FullName, searchOption));
                }
            }

            return files.ToArray();
        }

        public bool IsConnected => _client.IsConnected;
        public event UploadingHandler Uploading;
        public event DownloadingHandler Downloading;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _client?.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}