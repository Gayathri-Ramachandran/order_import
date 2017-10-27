using Agr.Int.OrderImport.Common.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Agr.Int.OrderImport.Common.Model;
using System.Xml.Serialization;
using System.IO;
using Agr.Int.OrderImport.Data.Models.Mappers;
using Agr.Int.OrderImport.Common.Abstractions.Services;
using Agr.Int.OrderImport.Common.Abstractions.Repositories;
using System.Configuration;

namespace Agr.Int.OrderImport.Framework
{
    class OrderImportService : IOrderImportService,IOrderWriteService
    {
        private readonly IOrderRepository _OrderRepository;
        private readonly IAppSettings _settings;
        public OrderImportService(IAppSettings settings, IOrderRepository OrderRepository)
        {
            if (settings == null)
            {
                //todo: logging
                throw new ArgumentNullException(nameof(settings));
            }
            if (OrderRepository == null)
            {
                //todo: logging
                throw new ArgumentNullException(nameof(OrderRepository));
            }

            _settings = settings;
            _OrderRepository = OrderRepository;
        }
        public orders ReadOrderData(string fileName)
        {
            try
            {
                var xRoot = new XmlRootAttribute();
                xRoot.ElementName = ConfigurationManager.AppSettings["orders"];
                xRoot.Namespace = ConfigurationManager.AppSettings["PrimaryNamespaceLink"];
                xRoot.IsNullable = true; 
                var serializer = new XmlSerializer(typeof(orders), xRoot);

                using (TextReader reader = new StreamReader(fileName))
                {
                    return (orders)serializer.Deserialize(reader);
                }
            }
            catch (Exception exe)
            {
                throw;
            }
        }

        public async Task<bool> WriteOrderData(orders orderdata)
        {
            /// [TODO] Implement propoer transaction scope for proper DB commits
            await _OrderRepository.WriteOrder(orderdata);

            return true; /// [TODO] Have to change for proper acknowledge
        }
    }
}
