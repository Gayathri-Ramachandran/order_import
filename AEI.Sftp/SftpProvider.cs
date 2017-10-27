using System;
using System.IO;
using AEI.Sftp.Adapters;
using Common.Logging;

namespace AEI.Sftp
{
    public delegate void UploadingHandler(string file, ulong uploaded, ulong totalBytes);
    public delegate void DownloadingHandler(string file, ulong downloaded, ulong totalBytes);

    public class SftpProvider : ISftpProvider
    {
        private readonly ISftpAdapter _adapter;
        private bool _disposed;
        private readonly ILog _log;

        public SftpProvider(ISftpAdapter adapter)
        {
            _adapter = adapter;
            _adapter.Uploading += _adapter_Uploading;
            _adapter.Downloading += _adapter_Downloading;
            _log = LogManager.GetLogger<SftpAdapter>();
        }

        public void UploadFolder(string sourceFolder, string searchPattern, SearchOption searchOption, string targetFolder, Func<string, string> renameCallback, bool deleteSourceFiles = false)
        {
            _log.Trace(m => m("Start UploadFolder on {0}", _adapter.GetType()));

            if (string.IsNullOrWhiteSpace(sourceFolder))
                throw new ArgumentNullException(nameof(sourceFolder));

            if (string.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentNullException(nameof(targetFolder));

            Connect();

            var files = Directory.GetFiles(sourceFolder, searchPattern, searchOption);

            _log.Info($"{files.Length} found to be uploaded using {_adapter.GetType()}");

            foreach (var file in files)
            {
                var sftpRelativefolder = Path.GetDirectoryName(file)?.Replace(sourceFolder, string.Empty);
                var target =
                    $"{targetFolder}\\{sftpRelativefolder}\\{Path.GetFileName(file)}".Replace(".\\", string.Empty);
                if (target.StartsWith("\\"))
                    target = target.Substring(1);

                Upload(file, target, renameCallback, deleteSourceFiles);
            }

            Disconnect();

            _log.Trace(m => m("End UploadFolder on {0}", _adapter.GetType()));
        }

        public void Download(string source, string target, bool deleteSourceFile = false)
        {
            _log.Trace(m => m("Start Download on {0}", _adapter.GetType()));
            _adapter.Download(source, target, deleteSourceFile);
            _log.Trace(m => m("End Download on {0}", _adapter.GetType()));
        }

        public void DownloadFolder(string sourceFolder, SearchOption searchOption, string targetFolder, bool deleteSourceFiles = false)
        {
            _log.Trace(m => m("Start DownloadFolder on {0}", _adapter.GetType()));

            if (string.IsNullOrWhiteSpace(targetFolder))
                throw new ArgumentNullException(nameof(targetFolder));

            if (string.IsNullOrWhiteSpace(sourceFolder))
                throw new ArgumentNullException(nameof(sourceFolder));

            Connect();

            var files = _adapter.ListFiles(sourceFolder, searchOption);

            foreach (var file in files)
            {
                _adapter.Download(file, $"{targetFolder}{file}", deleteSourceFiles);
            }

            Disconnect();

            _log.Trace(m => m("End DownloadFolder on {0}", _adapter.GetType()));
        }

        public string[] ListFiles(string path, SearchOption searchOption)
        {
            return _adapter.ListFiles(path, searchOption);
        }

        public bool IsConnected => _adapter.IsConnected;

        public event UploadingHandler Uploading;
        public event DownloadingHandler Downloading;

        private void _adapter_Uploading(string file, ulong uploaded, ulong totalBytes)
        {
            Uploading?.Invoke(file, uploaded, totalBytes);
        }

        private void _adapter_Downloading(string file, ulong downloaded, ulong totalBytes)
        {
            Downloading?.Invoke(file, downloaded, totalBytes);
        }

        ~SftpProvider()
        {
            Dispose(false);
        }

        public void Disconnect()
        {
            _log.Trace(m => m("Start Disconnect on {0}", _adapter.GetType()));
            _adapter.Disconnect();
            _log.Trace(m => m("End Disconnect on {0}", _adapter.GetType()));
        }
        
        public void Connect()
        {
            _log.Trace(m => m("Start Connect on {0}", _adapter.GetType()));
            _adapter.Connect();
            _log.Trace(m => m("End Connect on {0}", _adapter.GetType()));
        }

        public void Upload(string source, string target, Func<string, string> renameCallback, bool deleteSourceFile = false)
        {
            _log.Trace(m => m("Start Upload on {0}", _adapter.GetType()));
            _adapter.Upload(source, target, renameCallback, deleteSourceFile);
            _log.Trace(m => m("End Upload on {0}", _adapter.GetType()));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _adapter?.Dispose();
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