using AFM.Commerce.Framework.Extensions.Controllers;
using Microsoft.Dynamics.Retail.Ecommerce.Sdk.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRTBase
{
    public class CustomerHandler
    {
        Customer customer = null;
        List<Address> addresses = null;
        AFMCheckoutController checkoutController = null;

        public Tuple<long,long> GetCustomerWithPostalAddressID(complexTypeCustomer SFCCCustomer,List<complexTypeAddress> listShippingAddress, List<complexTypeAddress> listBillingAddress)
        {
            try
            {
                long j = 234212;
                customer = MapCustomer(SFCCCustomer, listShippingAddress, listBillingAddress);
                checkoutController.ProcessOfflinePendingOrder(customer,"test");
                return new Tuple<long, long>(j, 2);
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
                customer.FirstName = SFCCCustomer.customername.Split(' ')!=null ? SFCCCustomer.customername.Split(' ').First() : String.Empty;
                customer.LastName = SFCCCustomer.customername.Split(' ') != null ? SFCCCustomer.customername.Split(' ')[1] : String.Empty; /// As we don't have required information in order.xml file we are splitting this way 
                /// Assigning address
                customer.Addresses = MapAddress(listShippingAddress,listBillingAddress);
                customer.IsMarketingOptIn = false;
                customer.IsGuestCheckout = true; //[TODO] - Need to work on this
                customer.IsOlderThan13 = true;
                customer.PrimaryAddress = MapAddress(listShippingAddress, listBillingAddress, true).FirstOrDefault();
                return customer;

            }
            catch (Exception exe)
            {
                throw;
            }
        }

        private List<Address> MapAddress(List<complexTypeAddress> listShippingAddress, List<complexTypeAddress> listBillingAddress,bool isPrimary = false)
        {
            try
            {
                addresses = new List<Address>();
                /// Adding shipping address
                addresses = listShippingAddress.Select(k => new Address()
                {
                    Country = k.countrycode,
                    State = k.statecode,
                    City = k.city,
                    Name = String.Format("{0} {1}", k.firstname, k.lastname),
                    Street = k.address1,
                    ZipCode = k.postalcode,
                    Phone = k.phone,
                    Email = "praveen.bendi@gmail.com", ///[TODO]
                    Deactivate = false,
                    IsPrimary = true,
                    AddressType = AddressType.Delivery,
                    AddressFriendlyName = String.Format("{0} {1}", k.firstname, k.lastname),
                    FirstName = k.firstname,
                    LastName = k.lastname,
                    Street2 = k.address2,
                    Email2 = "praveen.bendi@gmail.com",///[TODO]
                    Phone2 = ""                 
                }).ToList();
                if (isPrimary)
                    return addresses;
                /// Adding Billing address
                addresses = addresses.Union(listBillingAddress.Select(k => new Address()
                {
                    Country = k.countrycode,
                    State = k.statecode,
                    City = k.city,
                    Name = String.Format("{0} {1}", k.firstname, k.lastname),
                    Street = k.address1,
                    ZipCode = k.postalcode,
                    Phone = k.phone,
                    Email = "praveen.bendi@gmail.com", ///[TODO]
                    Deactivate = false,
                    IsPrimary = true,
                    AddressType = AddressType.Delivery,
                    AddressFriendlyName = String.Format("{0} {1}", k.firstname, k.lastname),
                    FirstName = k.firstname,
                    LastName = k.lastname,
                    Street2 = k.address2,
                    Email2 = "praveen.bendi@gmail.com", ///[TODO]
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
