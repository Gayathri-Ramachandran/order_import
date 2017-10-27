using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agr.Int.OrderImport.Common.Abstractions.Services;
using Agr.Int.OrderImport.Common.Model;
using System.IO;

namespace Agr.Int.OrderImport.Framework
{
    public class Import
    {
        private readonly IOrderImportService _importService;
        private readonly IOrderWriteService _writeOrderService;
        private readonly string Inputdir;
        private readonly string InprocessDir;
        private readonly string ErrorDir;
        private readonly string SuccessDir;
        private readonly IAppSettings _settings;

        public Import(IOrderImportService importService, IOrderWriteService writeOrderService, IAppSettings settings)
        {
            if (importService == null)
            {
                //todo: logging
                throw new ArgumentNullException(nameof(IOrderImportService));
            }
            if (writeOrderService == null)
            {
                //todo: logging
                throw new ArgumentNullException(nameof(IOrderWriteService));
            }
            if (settings == null)
            {
                //todo: logging
                throw new ArgumentNullException(nameof(settings));
            }

            _settings = settings;
            this._importService = importService;
            this._writeOrderService = writeOrderService;
            this.Inputdir = _settings.SftpFolderLocation + _settings.GetAppSettingValue("Input Directory");
            this.InprocessDir = _settings.SftpFolderLocation + _settings.GetAppSettingValue("InProcess Directory");
            this.ErrorDir = _settings.SftpFolderLocation + _settings.GetAppSettingValue("Error Directory");
            this.SuccessDir = _settings.SftpFolderLocation + _settings.GetAppSettingValue("Success Directory");
        }

        public async Task ImportOrder()
        {
            foreach (string fileName in Directory.EnumerateFiles(Inputdir))
            {
                string destFileName = InprocessDir + "\\"+"inprocess_" + DateTime.Now.Ticks;
                try
                {
                    File.Move(fileName, destFileName);
                    var order = _importService.ReadOrderData(destFileName);
                    await _writeOrderService.WriteOrderData(order);
                    string successFileName = SuccessDir + "\\" + Path.GetFileName(fileName) + "_" + DateTime.Now.Ticks;
                    File.Move(destFileName, successFileName);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.StackTrace);
                    Console.ReadKey();
                    string errorFileName = ErrorDir +"\\"+ Path.GetFileName(fileName) + "_" + DateTime.Now.Ticks;
                    File.Move(destFileName, errorFileName);
                }
            }
        }
    }
}
