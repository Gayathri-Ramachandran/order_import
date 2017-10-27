using System;
using System.IO;

namespace AEI.Sftp.Adapters
{
    public interface ISftpAdapter : IDisposable
    {
        void Connect();
        //void Upload(string source, string target, bool deleteSourceFile = false);
        void Upload(string source, string target, Func<string, string> renameCallback, bool deleteSourceFile = false);
        void Download(string source, string target, bool deleteSourceFile = false);
        void Disconnect();
        string[] ListFiles(string path, SearchOption searchOption);
        bool IsConnected { get; }
        event UploadingHandler Uploading;
        event DownloadingHandler Downloading;
    }
}