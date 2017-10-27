using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agr.Int.Orderimport.Data.Access.Constants
{
    internal static class XMLAttributeType
    {
        public const string AFMLineConfirmationID = "apgLineConfirmationId";
        public const string DeliveryMethod = "deliveryMethod";
        public const string HDServiceItemID = "Home Delivery";
        public const string DSServiceItemID = "DS Small Parcel";
        public const string HDServiceItemDLVMode = "HD";
        public const string DSServiceItemDLVMode = "DS";
        public const string DefaultUnit = "EA";
        public const string DefaultServiceItemQty = "1";
        public const string ServiceItemPriceDetails = "priceDetails";
        public const string FulfillerName = "fulfillerDisplayName";
        public const string AuthorizeData = "apgMultipleAuths";
        public const string DirectShipATPResponse = "atpDirectShipResponse";
        public const string HomeDeliveryATPResponse = "atpHomeDeliveryResponse";
        public static DateTime DefaultATPDate = new DateTime(1900,01,01);
        public const string AtpResponseBestDate = "atpResponseBestDate";
        public const string AtpResponseBeginDateRange = "atpResponseBestDate";
        public const string AtpResponseEndDateRange = "atpResponseBestDate";
        public const string InventoryAvailabilityMessage = "atpResponseBestDate";
        public const string IsAvailable = "isAvailable";
        public const string PartOfKitNumber = "partOfKitNumber";
        public const string RequestedDeliveryDatetime = "requestedDeliveryDatetime";
    }
}
