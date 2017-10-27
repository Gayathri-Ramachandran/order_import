using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agr.Int.OrderImport
{
    class ImportOrder
    {
        public static CompositionContainer Container { get; private set; }
        static void Main(string[] args)
        {
            try
            {
                Container = DependencyConfig.Configure();
                var runner = Container.GetExportedValue<OrderImportRunner>();
                runner.Run().Wait();
            }
            catch (Exception exception)
            {
                //log exception
                throw;
            }
        }
    }
}
