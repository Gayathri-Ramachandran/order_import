using System;
using System.IO;

namespace AEI.Sftp
{
    public interface ISftpProvider : IDisposable
    {
        void Connect();
        void Upload(string source, string target, Func<string, string> renameCallback, bool deleteSourceFile = false);
        void Disconnect();
        void UploadFolder(string sourceFolder, string searchPattern, SearchOption searchOption, string targetFolder, Func<string, string> renameCallback, bool deleteSourceFiles = false);
        void Download(string source, string target, bool deleteSourceFile = false);
        void DownloadFolder(string sourceFolder, SearchOption searchOption, string targetFolder, bool deleteSourceFiles = false);
        string[] ListFiles(string path, SearchOption searchOption);
        bool IsConnected { get; }
        event UploadingHandler Uploading;
        event DownloadingHandler Downloading;
    }
}
