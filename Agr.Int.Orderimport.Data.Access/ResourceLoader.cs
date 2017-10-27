using Agr.Int.Orderimport.Data.Access.Constants;
using Agr.Int.OrderImport.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Agr.Int.Orderimport.Data.Access
{
    public class ResourceLoader : IResourceLoader
    {
        /// <summary>
        /// Reads sql file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetContentResource(string fileName)
        {
            try
            {
                string file;
                var assembly = Assembly.GetExecutingAssembly();
                var filePath = assembly.CodeBase.ToLower().Replace(assembly.ManifestModule.Name.ToLower(), "")
                                .Replace("file:///", "") + Resources.ResourceFolder + "/" + fileName;

                using (var streamReader = new StreamReader(filePath, false))
                {
                    file = streamReader.ReadToEnd();
                }

                return file;
            }
            catch (Exception exception)
            {
                throw;
            }
        }

    }
}
