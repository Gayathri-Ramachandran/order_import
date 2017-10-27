using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agr.Int.OrderImport.Common.Model
{
    public  interface IAppSettings
    {
        string SftpFolderLocation { get; }

        string GetAppSettingValue(string key);
    }
}
