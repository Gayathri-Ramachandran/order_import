using System;
using System.IO;
using Common.Logging;
using AEI.Sftp;
using AEI.Sftp.Adapters;
using AEI.Sftp.Callbacks;
using Agr.Int.OrderImport.Common.Model;

namespace Agr.Int.HDShippingChargesFeed.Framework
{
    public class SftpAdapterClient : ISFTPAdapter
    {
        //private Process _process;
        private SftpProvider _provider;

        /// <summary>
        /// The application settings
        /// </summary>
        private readonly IAppSettings _appSettings;
        private readonly ILog _logger;

        public SftpAdapterClient(IAppSettings appSettings)
        {
            _logger = LogManager.GetLogger<SftpAdapterClient>();
            _logger.Trace(m => m($"Started constructor SftpAdapterClient"));
            if (appSettings == null)
            {
                _logger.Error(m => m($"Passed an invalid parameter: {nameof(appSettings)}. Parameter is null."));
                throw new ArgumentNullException(nameof(appSettings));
            }

            _appSettings = appSettings;
            _logger.Trace(m => m($"Finished constructor SftpAdapterClient"));
        }

        public void Upload()
        {
            _logger.Trace(m => m($"Started processing SFTP "));
            var adapter = new SftpAdapter(_appSettings.GetAppSettingValue("sftpAdapterName"));
            _provider = new SftpProvider(adapter);
            bool deleteSourceFile;
            Boolean.TryParse(_appSettings.GetAppSettingValue("sftpDeleteSourceFile"), out deleteSourceFile);
            _logger.Trace(m => m($"Started uploading file from application to SFTP "));
            _provider.UploadFolder(_appSettings.SftpFolderLocation, "*.tmp", SearchOption.TopDirectoryOnly, ".", SftpCallbacks.StripFinalExtensionCallback, deleteSourceFile);
            _logger.Trace(m => m($"Finished SFTP upload"));
            _logger.Trace(m => m($"Finished processing SFTP "));
        }


        public void Dispose()
        {
            if (_provider != null)
            {
                if (_provider.IsConnected) _provider.Disconnect();
                _provider.Dispose();
                _provider = null;
            }

        }
    }
}
