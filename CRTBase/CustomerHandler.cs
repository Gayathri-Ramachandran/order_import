using AFM.Commerce.Framework.Extensions.Controllers;
using Microsoft.Dynamics.Retail.Ecommerce.Sdk.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Dynamics.Retail.PaymentSDK;

namespace CRTBase
{
    public class CustomerHandler
    {

        List<KeyValuePair<string, string>> defaultPropertyPairList = new List<KeyValuePair<string, string>>() {
                    new KeyValuePair<string, string>("Connector.ConnectorName", "AshleyPayment"),
                new KeyValuePair<string, string>("Connector.ConnectorVersion", "4.1.2.35"),
                new KeyValuePair<string, string>("PaymentCard.FinancingTerms", string.Empty),
                new KeyValuePair<string, string>("PaymentCard.IsTokenGeneratedByCommerceRuntime", "True"),
                new KeyValuePair<string, string>("PaymentCard.FinancingOptionDescription1", string.Empty),
                new KeyValuePair<string, string>("PaymentCard.EConsentRequired", "False"),
                new KeyValuePair<string, string>("PaymentCard.SupportsMultipleAuthorizations", "True"),
                new KeyValuePair<string, string>("PaymentCard.PaymentConfiguration", "PHASE2"),
                new KeyValuePair<string, string>("PaymentCard.OriginalTransactionId", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.ApplicationId", string.Empty),
                new KeyValuePair<string, string>("PaymentCard.FinancingOptionId", "AFHS-ASHCOMM2-DW"),
                new KeyValuePair<string, string>("PaymentGatewayResponse.HttpStatusCode", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.IsServiceError", "False"),
                new KeyValuePair<string, string>("PaymentGatewayResponse.IsUserError", "False"),
                new KeyValuePair<string, string>("PaymentGatewayResponse.DeviceId", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.ApiResponseTime", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.ApiVersion", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.DeviceCashier", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.ResponseTime", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.HttpStatus", string.Empty),
                new KeyValuePair<string, string>("PaymentGatewayResponse.Environment", string.Empty),
                new KeyValuePair<string, string>("PaymentCard.DeviceIpAddress", string.Empty),
                new KeyValuePair<string, string>("MerchantAccount.ServiceAccountId", string.Empty),
                new KeyValuePair<string, string>("FuseboxResponse.EcommerceGoodsIndicator", "P")
};

        Customer customer = null;
        List<Address> addresses = null;
        AFMCheckoutController checkoutController = null;

        public string BuildPaymentProperties(PaymentTransaction payment)
        {

            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.Name", payment.CardHolderName));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.CardType", payment.CardType));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.Country", "US"));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.StreetAddress", payment.BillingStreet));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.PostalCode", payment.BillingZipcode));

            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.UniqueCardId", "c7352ec8-d6e8-4854-8b6f-349cfbcf80fd"));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.CardToken", payment.TransactionId));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.Last4Digits", payment.CardNumber));

            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.City", payment.BillingCity));
            defaultPropertyPairList.Add(new KeyValuePair<string, string>("PaymentCard.State", payment.BillingState));


            PaymentProperty[] properties = new PaymentProperty[36];

            var x = 0;

            foreach (var propertyPair in defaultPropertyPairList)
            {
                string[] propertyArray = propertyPair.Key.Split('.');
                if (x < (properties.Length -1 ))
                {
                    properties[x] = new PaymentProperty(propertyArray[0], propertyArray[1], propertyPair.Value, Microsoft.Dynamics.Retail.PaymentSDK.SecurityLevel.None);
                    x++;
                }
                else
                {
                    x--;
                    break;
                }                
            }
            properties[x++] = new PaymentProperty("PaymentCard", "ExpirationYear", payment.ExpireYear, Microsoft.Dynamics.Retail.PaymentSDK.SecurityLevel.None);
            properties[x++] = new PaymentProperty("PaymentCard", "ExpirationMonth", payment.ExpireMonth, Microsoft.Dynamics.Retail.PaymentSDK.SecurityLevel.None);

            var xml = PaymentProperty.ConvertPropertyArrayToXML(properties);

            return xml != null ? xml : string.Empty;

        }

        public Tuple<string, string> GetCustomerWithPostalAddressID(complexTypeCustomer SFCCCustomer, List<complexTypeAddress> listShippingAddress, List<complexTypeAddress> listBillingAddress)
        {
            try
            {
                Tuple<string, string> CustomerWithPostalID;
                customer = MapCustomer(SFCCCustomer, listShippingAddress, listBillingAddress);
                checkoutController = new AFMCheckoutController();
                CustomerWithPostalID = checkoutController.ProcessSFCCCustomer(customer);
                return CustomerWithPostalID;
            }
            catch (Exception exe)
            {
                throw;
            }
        }

        internal Customer MapCustomer(complexTypeCustomer SFCCCustomer, List<complexTypeAddress> listShippingAddress, List<complexTypeAddress> listBillingAddress)
        {
            try
            {

                /// Initializing the objects
                customer = new Customer();
                customer.Email = SFCCCustomer.customeremail;
                customer.Phone = listShippingAddress.FirstOrDefault().phone;
                customer.FirstName = SFCCCustomer.customername.Split(' ') != null ? SFCCCustomer.customername.Split(' ').First() : String.Empty;
                customer.LastName = SFCCCustomer.customername.Split(' ') != null ? SFCCCustomer.customername.Split(' ')[1] : String.Empty; /// As we don't have required information in order.xml file we are splitting this way 
                                                                                                                                           /// Assigning address
                customer.Addresses = MapAddress(listShippingAddress, listBillingAddress);
                customer.IsMarketingOptIn = false;
                customer.IsMarketingOptInDate = Convert.ToDateTime("1900-01-01");
                customer.IsGuestCheckout = true;
                customer.IsOlderThan13 = true;
                customer.PrimaryAddress = MapAddress(listShippingAddress, listBillingAddress, true).FirstOrDefault();
                return customer;

            }
            catch (Exception exe)
            {
                throw;
            }
        }

        private List<Address> MapAddress(List<complexTypeAddress> listShippingAddress, List<complexTypeAddress> listBillingAddress, bool isPrimary = false)
        {
            try
            {
                addresses = new List<Address>();
                /// Adding shipping address
                addresses = listShippingAddress.Select(k => new Address()
                {
                    Country = k.countrycode == "US" ? "USA" : k.countrycode, /// As per AX system
                    State = k.statecode,
                    City = k.city,
                    Name = String.Format("{0} {1}", k.firstname, k.lastname),
                    Street = k.address1,
                    ZipCode = k.postalcode,
                    Phone = k.phone,
                    Email = "test@gmail.com", ///[TODO]
                    Deactivate = false,
                    IsPrimary = true,
                    AddressType = AddressType.Delivery,
                    AddressFriendlyName = String.Format("{0} {1}", k.firstname, k.lastname),
                    FirstName = k.firstname,
                    LastName = k.lastname,
                    Street2 = k.address2,
                    Email2 = "test@gmail.com",///[TODO]
                    Phone2 = ""
                }).ToList();
                if (isPrimary)
                    return addresses;
                /// Adding Billing address
                addresses = addresses.Union(listBillingAddress.Select(k => new Address()
                {
                    Country = k.countrycode == "US" ? "USA" : k.countrycode,
                    State = k.statecode,
                    City = k.city,
                    Name = String.Format("{0} {1}", k.firstname, k.lastname),
                    Street = k.address1,
                    ZipCode = k.postalcode,
                    Phone = k.phone,
                    Email = "test@gmail.com", ///[TODO]
                    Deactivate = false,
                    IsPrimary = true,
                    AddressType = AddressType.Invoice,
                    AddressFriendlyName = String.Format("{0} {1}", k.firstname, k.lastname),
                    FirstName = k.firstname,
                    LastName = k.lastname,
                    Street2 = k.address2,
                    Email2 = "test@gmail.com", ///[TODO]
                    Phone2 = ""
                })).ToList();
                return addresses;
            }
            catch (Exception exe)
            {
                throw;
            }
        }
    }
}
