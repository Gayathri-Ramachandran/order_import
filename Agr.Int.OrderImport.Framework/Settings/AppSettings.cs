using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agr.Int.OrderImport.Common.Model;
using Agr.Int.OrderImport.Common.Configurations;
using System.Configuration;

namespace Agr.Int.OrderImport.Framework.Settings
{
    public class AppSettings : IAppSettings
    {
        private readonly SftpLocationConfigurationSection sftpSettings;
        public AppSettings()
        {
            try
            {
                sftpSettings = ConfigurationManager.GetSection("sftpsettings") as SftpLocationConfigurationSection;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public string SftpFolderLocation => sftpSettings.Folder;
        public string GetAppSettingValue(string key) => ConfigurationManager.AppSettings[key] ?? "Not Found";
    }
}
