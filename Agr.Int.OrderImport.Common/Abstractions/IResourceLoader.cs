using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agr.Int.OrderImport.Common.Abstractions
{
    public interface IResourceLoader
    {
        string GetContentResource(string fileName);
    }
}
