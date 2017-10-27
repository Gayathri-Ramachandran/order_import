using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agr.Int.OrderImport.Framework;

namespace Agr.Int.OrderImport
{
    public class OrderImportRunner
    {
        private readonly Import _OrderImportRunner;

        public OrderImportRunner(Import orderImportRunner)
        {
            if (orderImportRunner == null)
            {
                //todo: add logging
                throw new ArgumentNullException(nameof(Import));
            }

            _OrderImportRunner = orderImportRunner;
        }
        public async Task Run()
        {
            await _OrderImportRunner.ImportOrder();
        }

    }
}