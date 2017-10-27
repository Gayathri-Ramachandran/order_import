using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Agr.Int.OrderImport.Common.Data;
using Agr.Int.Orderimport.Data.Access.Constants;
using System.Data;
using Agr.Int.OrderImport.Common.Abstractions.Repositories;
using Agr.Int.OrderImport.Common.Abstractions;
using Agr.Int.OrderImport.Data.Models.Mappers;
using Newtonsoft.Json;
using CRTBase;
using AFM.Commerce.Logging;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using Agr.Int.OrderImport.Framework;
using Agr.Int.OrderImport.Common.Model;
using System.Configuration;

namespace Agr.Int.Orderimport.Data.Access
{
    public class OrderRepository : AsyncDbRepository, IOrderRepository
    {
        private readonly IResourceLoader _resourceLoader;
        List<complexTypeAddress> listShippingAddress = null;
        List<complexTypeAddress> listBillingAddress = null;
        string initialKitNumber = string.Empty;
        int kitSequenceNumber = 0;
        private readonly IAppSettings _settings;
        public string _errorFolderPath = string.Empty;

        public OrderRepository(IDbConnectionFactory connectionFactory, IResourceLoader resourceLoader, IAppSettings settings) : base(connectionFactory)
        {
            if (resourceLoader == null) throw new ArgumentNullException(nameof(resourceLoader));
            if (settings == null)
            {
                //todo: logging
                throw new ArgumentNullException(nameof(settings));
            }

            _settings = settings;
            _errorFolderPath = _settings.SftpFolderLocation + _settings.GetAppSettingValue("Error Directory");
            _resourceLoader = resourceLoader;
        }

        public async Task<bool> WriteOrder(orders orders)
        {
                string logisticPostalAddressID = String.Empty;
                string AXCustomerAccountNum = String.Empty;
                foreach (var order in orders.order)
                {
                    try
                    {                    
                    logisticPostalAddressID = String.Empty;
                    AXCustomerAccountNum = String.Empty;
                    var productLineItem = GetCustomizedProductLineItems(order);
                    var listServiceLineitem = GetCustomizedServiceItems(order, productLineItem.Count);
                    var payments = GetCustomizedPaymentDetails(order);
                    var OnlineDeliveryDateModule = order.shipments != null && order.shipments.FirstOrDefault() != null && order.shipments.FirstOrDefault().customattributes != null ? order.shipments.FirstOrDefault().customattributes.Where(j => j.attributeid == XMLAttributeType.RequestedDeliveryDatetime).FirstOrDefault() : null;
                    var OnlineDeliverDate = OnlineDeliveryDateModule != null ? OnlineDeliveryDateModule.Text[0] : null;
                    listShippingAddress = new List<complexTypeAddress>() { order.shipments[0].shippingaddress }; // Supports multiple shipping 
                    listShippingAddress.ForEach(t =>
                    {
                        if (t.postalcode != null)
                            t.postalcode = t.postalcode.Substring(0, 5);
                    });
                    listBillingAddress = new List<complexTypeAddress>() { order.customer.billingaddress }; // supports multiple addresses for multiple payments
                    listBillingAddress.ForEach(t =>
                    {
                        if (t.postalcode != null)
                            t.postalcode = t.postalcode.Substring(0, 5);
                    });

                        CustomerHandler custHandler = new CustomerHandler();

                    
                        Tuple<string, string> CustomerIDWithPoatalAddress = custHandler.GetCustomerWithPostalAddressID(order.customer, listShippingAddress, listBillingAddress);
                        logisticPostalAddressID = CustomerIDWithPoatalAddress.Item2;
                        AXCustomerAccountNum = CustomerIDWithPoatalAddress.Item1;                
                    await writeOrder(logisticPostalAddressID, AXCustomerAccountNum, productLineItem, listServiceLineitem, listShippingAddress, listBillingAddress, order, payments);
                    }
                    catch (Exception exe)
                    {

                    GetXMLFromObject(order, _errorFolderPath + "//error" + DateTime.Now.Ticks+".xml");

                    AFM.Commerce.Logging.LoggerUtilities.ProcessLogMessage(
                        "Order Import Processor Exception : " + exe.StackTrace);
                        
                    if (exe.InnerException != null)
                        AFM.Commerce.Logging.LoggerUtilities.ProcessLogMessage(exe.InnerException);
                    continue;
                    }
                }      
            return true; /// [TODO] this acknowledgement will be removed with post implementation of Listner module from different console Application.
        }


        public async Task<bool> writeOrder(string logisticPostalAddressID, string AXCustomerAccountNum, List<complexTypeProductLineItem> productLineItem, List<complexTypeShippingLineItem> listServiceLineitem,
            List<complexTypeAddress> listShippingAddress, List<complexTypeAddress> listBillingAddress, complexTypeOrder order, List<TenderLine> payments)
        {
            try
            {
                var sql = _resourceLoader.GetContentResource(Query.WriteOrderSQL);
                var result = await WithConnection(async c => await c.QueryAsync(sql: sql,
                    param: new
                    {
                        logisticPostalAddressID,
                        AXCustomerAccountNum,
                        order.orderno,
                        order.customer.customeremail,
                        order.customer.customerno,
                        order.customer.customername,
                        order.currency,
                        order.orderdate,
                        Payments = payments.AsTableValuedParameter(TableTypes.PaymentTvp, new[] { "amount", "CardType", "CardNumber", "currency", "authorizationId", "lineNum", "PaymentAuthorization", "lineConfirmationId" }),
                        ProductLineItems = productLineItem.AsTableValuedParameter(TableTypes.ProductLineTvp, new[] { "productid", "netprice", "tax", "grossprice", "AFMLineConfirmationID", "DLVMode", "Quantity", "Unit", "Linenum", "FulfillerName", "BestDate", "BeginDateRange", "EndDateRange", "AFMMessage", "AFMVendorID", "AFMKitItemID", "AFMKitSequenceNumber", "AFMKitItemQuantity" }),
                        ServiceLineItem = listServiceLineitem.AsTableValuedParameter(TableTypes.ServiceLineItmesTvp, new[] { "itemid", "netprice", "tax", "grossprice", "AFMLineConfirmationID", "DLVMode", "Quantity", "Unit", "Linenum", "AFMVendorID" }),
                        ShippingAddress = listShippingAddress.AsTableValuedParameter(TableTypes.ShippingAddressTvp, new[] { "firstname", "lastname", "address1", "city", "postalcode", "statecode", "countrycode", "phone" }),
                        BillingAddress = listBillingAddress.AsTableValuedParameter(TableTypes.BillingAddressTvp, new[] { "firstname", "lastname", "address1", "city", "postalcode", "statecode", "countrycode", "phone" }),
                        Res = "true"
                    }, // result value on success of insert
                    commandType: CommandType.Text));
                return true;
            }
            catch (Exception exe)
            {
                /// [TODO] this acknowledgement will be implemented with post implementation of Listner module from different console Application.
                return false;
            }
        }

        private List<complexTypeProductLineItem> GetCustomizedProductLineItems(complexTypeOrder order)
        {
            try
            {
                var test = order.productlineitems[0].customattributes.Where(j => j.attributeid == XMLAttributeType.PartOfKitNumber).FirstOrDefault();
                int lineNum = 1;
                kitSequenceNumber = 1;
                var KitAttribute = order.productlineitems.FirstOrDefault().customattributes.Where(j => j.attributeid == XMLAttributeType.PartOfKitNumber).FirstOrDefault();
                initialKitNumber = KitAttribute != null ? KitAttribute.Text[0] : string.Empty;
                return order.productlineitems.Select(k => new complexTypeProductLineItem
                {
                    /// [TODO] Optimization needed for below ATP calls 
                    AFMLineConfirmationID = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.AFMLineConfirmationID).FirstOrDefault() != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.AFMLineConfirmationID).FirstOrDefault().Text[0] : String.Empty,
                    DLVMode = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.DeliveryMethod).FirstOrDefault() != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.DeliveryMethod).FirstOrDefault().Text[0] : String.Empty,
                    FulfillerName = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.FulfillerName).FirstOrDefault() != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.FulfillerName).FirstOrDefault().Text[0] : String.Empty,
                    productid = k.productid,
                    netprice = k.netprice,
                    tax = k.tax,
                    grossprice = k.grossprice,
                    Quantity = k.quantity != null ? k.quantity.Value : 0,
                    Unit = k.quantity != null ? (k.quantity.unit == String.Empty ? XMLAttributeType.DefaultUnit : k.quantity.unit) : string.Empty,
                    BestDate = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.AtpResponseBestDate).FirstOrDefault() != null ? Convert.ToDateTime(k.customattributes.Where(j => j.attributeid == XMLAttributeType.AtpResponseBestDate).FirstOrDefault().Text[0]) : XMLAttributeType.DefaultATPDate,
                    BeginDateRange = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.AtpResponseBeginDateRange).FirstOrDefault() != null ? Convert.ToDateTime(k.customattributes.Where(j => j.attributeid == XMLAttributeType.AtpResponseBeginDateRange).FirstOrDefault().Text[0]) : XMLAttributeType.DefaultATPDate,
                    EndDateRange = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.AtpResponseEndDateRange).FirstOrDefault() != null ? Convert.ToDateTime(k.customattributes.Where(j => j.attributeid == XMLAttributeType.AtpResponseEndDateRange).FirstOrDefault().Text[0]) : XMLAttributeType.DefaultATPDate,
                    IsAvailable = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.IsAvailable).FirstOrDefault() != null ? Convert.ToBoolean(k.customattributes.Where(j => j.attributeid == XMLAttributeType.IsAvailable).FirstOrDefault().Text[0]) : true, /// Default value set as true since its a part of order
                    //AFMVendorID = k.customattributes != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.).FirstOrDefault().Text[0] : String.Empty,
                    AFMVendorID = "4444400_100",
                    AFMMessage = k.customattributes != null && k.customattributes.Where(j => j.attributeid == XMLAttributeType.InventoryAvailabilityMessage).FirstOrDefault() != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.InventoryAvailabilityMessage).FirstOrDefault().Text[0] : string.Empty,
                    AFMKitItemID = k.customattributes != null ? (k.customattributes.Where(j => j.attributeid == XMLAttributeType.PartOfKitNumber).FirstOrDefault() != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.PartOfKitNumber).FirstOrDefault().Text[0] : string.Empty) : string.Empty,
                    AFMKitSequenceNumber = k.customattributes != null ? GenerateKitSequenceNumber(k.customattributes.Where(j => j.attributeid == XMLAttributeType.PartOfKitNumber) != null ? k.customattributes.Where(j => j.attributeid == XMLAttributeType.PartOfKitNumber).FirstOrDefault() : null, lineNum) : 0,
                    //AFMKitItemQuantity = k.quantity.Value, // This needs to be changed according to future XML file. Currently Lyons won't send this value
                    Linenum = lineNum++       // We can't use position as a line number from XML
                }).ToList();
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe.StackTrace);
                Console.ReadKey();
                throw;
            }
        }

        private int GenerateKitSequenceNumber(sharedTypeCustomAttribute customAttribute, int lineNum)
        {
            if (customAttribute == null || (customAttribute != null && customAttribute.Text[0] == string.Empty))
                return 0;
            if (customAttribute != null && customAttribute.Text[0] != string.Empty)
                if (lineNum == 1)
                    kitSequenceNumber = 1;
                else if (customAttribute.Text[0] != string.Empty && customAttribute.Text[0] != initialKitNumber)
                {
                    kitSequenceNumber++;
                    initialKitNumber = customAttribute.Text[0];
                }
            return kitSequenceNumber;
        }

        private List<complexTypeShippingLineItem> GetCustomizedServiceItems(complexTypeOrder order, int lineNum)
        {
            try
            {
                List<complexTypeShippingLineItem> listServiceItems = new List<complexTypeShippingLineItem>();
                var shippingLineItems = order.shippinglineitems.FirstOrDefault().customattributes; /// Have to consider first object since we are getting only one shipping line item
                var shippingItems = JsonConvert.DeserializeObject<ServiceLevelDeliveryModes>(shippingLineItems.Where(j => j.attributeid == XMLAttributeType.ServiceItemPriceDetails).FirstOrDefault().Text[0]);
                /// Add HD service item Parameters
                if (shippingItems != null && shippingItems.HD != null && !String.IsNullOrEmpty(shippingItems.HD.lineConfirmationId))
                    listServiceItems.Add(new complexTypeShippingLineItem
                    {
                        AFMLineConfirmationID = shippingItems.HD.lineConfirmationId,
                        DLVMode = XMLAttributeType.HDServiceItemDLVMode,
                        itemid = XMLAttributeType.HDServiceItemID,
                        netprice = shippingItems.HD.netPrice,
                        tax = shippingItems.HD.tax,
                        grossprice = shippingItems.HD.grossPrice,
                        Quantity = XMLAttributeType.DefaultServiceItemQty,
                        Unit = XMLAttributeType.DefaultUnit,
                        Linenum = ++lineNum,
                        AFMVendorID = "4444400_100"
                    });
                /// Add DS service item Parameters
                if (shippingItems != null && shippingItems.DS != null && !String.IsNullOrEmpty(shippingItems.DS.lineConfirmationId))
                    listServiceItems.Add(new complexTypeShippingLineItem
                    {
                        AFMLineConfirmationID = shippingItems.DS.lineConfirmationId,
                        DLVMode = XMLAttributeType.DSServiceItemDLVMode,
                        itemid = XMLAttributeType.DSServiceItemID,
                        netprice = shippingItems.DS.netPrice,
                        tax = shippingItems.DS.tax,
                        grossprice = shippingItems.DS.grossPrice,
                        Quantity = XMLAttributeType.DefaultServiceItemQty,
                        Unit = XMLAttributeType.DefaultUnit,
                        Linenum = ++lineNum,
                        AFMVendorID = "8888300_480"
                    });
                return listServiceItems;
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe.StackTrace);
                Console.ReadKey();
                throw;
            }
        }

        private List<TenderLine> GetCustomizedPaymentDetails(complexTypeOrder order)
        {
            try
            {
                int lineNum = 1;
                var payments = order.payments;
                List<PaymentTransaction> listPaymenttransaction = new List<PaymentTransaction>();
                List<TenderLine> listTenderLine = new List<TenderLine>();

                CustomerHandler custHandler = new CustomerHandler();

                /// Adding credit card Payments
                listPaymenttransaction = payments.Select(k => new PaymentTransaction
                {
                    Amount = k.amount,
                    CardNumber = ((complexTypeCreditCard)k.Item).cardnumber,
                    CardType = ((complexTypeCreditCard)k.Item).cardtype,
                    ExpireYear = ((complexTypeCreditCard)k.Item).expirationyear,
                    ExpireMonth = ((complexTypeCreditCard)k.Item).expirationmonth,
                    TransactionId = k.transactionid,
                    BillingStreet = order.customer.billingaddress.address1,
                    BillingCity = order.customer.billingaddress.city,
                    BillingState = order.customer.billingaddress.statecode,
                    BillingZipcode = order.customer.billingaddress.postalcode,

                CardHolderName = ((complexTypeCreditCard)k.Item).cardholder,
                    LineNum = lineNum++,
                    PaymentType = typeof(complexTypeCreditCard),
                    TenderData = JsonConvert.DeserializeObject<List<TenderLine>>(k.customattributes.Where(j => j.attributeid == XMLAttributeType.AuthorizeData).FirstOrDefault().Text[0])
                }).Where(k => k.PaymentType == typeof(complexTypeCreditCard)).ToList();
                /// Adding custom Payments
                listPaymenttransaction.Union(payments.Select(k => new PaymentTransaction
                {
                    Amount = k.amount,
                    CardNumber = ((complexTypeCreditCard)k.Item).cardnumber,
                    CardType = ((complexTypeCreditCard)k.Item).cardtype,
                    ExpireYear = ((complexTypeCreditCard)k.Item).expirationyear,
                    ExpireMonth = ((complexTypeCreditCard)k.Item).expirationmonth,
                    TransactionId = k.transactionid,
                    CardHolderName = ((complexTypeCreditCard)k.Item).cardholder,
                    BillingStreet = order.customer.billingaddress.address1,
                    BillingCity = order.customer.billingaddress.city,
                    BillingState = order.customer.billingaddress.statecode,
                    BillingZipcode = order.customer.billingaddress.postalcode,
                    LineNum = lineNum++,
                    PaymentType = typeof(complexTypeCustomPaymentMethod),
                    TenderData = JsonConvert.DeserializeObject<List<TenderLine>>(k.customattributes.Where(j => j.attributeid == XMLAttributeType.AuthorizeData).FirstOrDefault().Text[0])
                }).Where(k => k.PaymentType == typeof(complexTypeCustomPaymentMethod)).ToList());
                lineNum = 1;
                /// Customize at each auth level
                foreach (PaymentTransaction paymentTransaction in listPaymenttransaction)
                    foreach (TenderLine tenderline in paymentTransaction.TenderData)
                    {
                        tenderline.CardNumber = paymentTransaction.CardNumber;
                        tenderline.CardType = paymentTransaction.CardType;
                        tenderline.lineNum = lineNum++;
                        var paymentXml = custHandler.BuildPaymentProperties(paymentTransaction);
                        //tenderline.PaymentAuthorization = !string.IsNullOrEmpty(tenderline.PaymentAuthorization) ? string.Format(tenderline.PaymentAuthorization, tenderline.authorizationId, tenderline.lineConfirmationId) : string.Empty;
                        tenderline.PaymentAuthorization = paymentXml;
                        listTenderLine.Add(tenderline);
                    }
                return listTenderLine;
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe.StackTrace);
                Console.ReadKey();
                throw;
            }
        }


        /// <summary>
        /// This function is no longer used as per the new XML structure
        /// </summary>
        /// <param name="orders"></param>
        /// <param name="sku"></param>
        /// <param name="MOD"></param>
        /// <returns></returns>
        private Entity GetCustomizedATPDetails(complexTypeOrder order, String sku, string MOD)
        {
            try
            {
                string AFMDSVendorID = string.Empty; /// This is not acutal ID. It is combination of  Account number and shipTo -> account_shipto for DS
                string AFMHDVendorID = string.Empty; /// This is not acutal ID. It is combination of  Account number and shipTo -> account_shipto for HD
                Entity entity = new Entity();
                /// the term 'actual' used because this is same response we are passing. which is different from ATP based on MOD 
                var actualDSATP = JsonConvert.DeserializeObject<ATP>(order.customattributes.Where(j => j.attributeid == XMLAttributeType.DirectShipATPResponse).FirstOrDefault().Text[0]);
                var actualHDATP = JsonConvert.DeserializeObject<ATP>(order.customattributes.Where(j => j.attributeid == XMLAttributeType.HomeDeliveryATPResponse).FirstOrDefault().Text[0]);
                AFMDSVendorID = String.Format("{0}_{1}", actualDSATP.properties.accountNumber, actualDSATP.properties.shipTo);
                AFMHDVendorID = String.Format("{0}_{1}", actualHDATP.properties.accountNumber, actualHDATP.properties.shipTo);
                if (MOD == DeliveryMode.HDDLVMode)
                {
                    entity = actualHDATP.entities.Where(k => k.sku == sku).FirstOrDefault();
                    entity.AFMVendorID = AFMHDVendorID;
                }
                if (MOD == DeliveryMode.DSDLVMode || MOD == DeliveryMode.DSThirdparty)
                {
                    entity = actualDSATP.entities.Where(k => k.sku == sku).FirstOrDefault();
                    entity.AFMVendorID = AFMDSVendorID;
                }
                return entity;
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe.StackTrace);
                Console.ReadKey();
                throw;
            }
        }

        private complexTypeAddress GetCustomizedAddress(complexTypeOrder order)
        {

            try
            {
                return order.shipments[0].shippingaddress;
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe.StackTrace);
                Console.ReadKey();
                throw;
            }
        }

        public string GetXMLFromObject(object o,string fileName)
        {
            StringWriter sw = new StringWriter();
            try
            {
                orders orders = new orders();
                orders.order = new complexTypeOrder[] { (complexTypeOrder)o };

                XmlTextWriter tw = new XmlTextWriter(sw);
                tw.Formatting = System.Xml.Formatting.Indented;
                tw.Indentation = 4;

                XmlSerializer ser = new XmlSerializer(orders.GetType());
                ser.Serialize(tw, orders);

                tw.Close();
                sw.Close();
                File.WriteAllText(fileName, sw.ToString());
            }
            catch (Exception ex)
            {
                //Handle Exception Code
            }
            return sw.ToString();
        }
    }

}