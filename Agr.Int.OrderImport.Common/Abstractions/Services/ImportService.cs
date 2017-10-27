using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agr.Int.OrderImport.Data.Models;

namespace Agr.Int.OrderImport.Common.Abstractions.Services
{
    public interface IOrderImportService
    {
      orders ReadOrderData(string fileName);
    }
}
