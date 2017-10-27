using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public partial class complexTypeProductLineItem
{
    public string AFMLineConfirmationID { get; set; }
    public string DLVMode { get; set; }
    public double Quantity { get; set; }
    public string Unit { get; set; }
    public int Linenum { get; set; }
    public string FulfillerName { get; set; }
    public DateTime BestDate { get; set; }
    public DateTime BeginDateRange { get; set; }
    public DateTime EndDateRange { get; set; }
    public bool IsAvailable { get; set; }
    public string AFMVendorID { get; set; }
    public string AFMMessage { get; set; }
    public string AFMKitItemID { get; set; }
    public int AFMKitSequenceNumber { get; set; }
    public Double AFMKitItemQuantity { get; set; }
}
public partial class complexTypeShippingLineItem
{
    public string AFMLineConfirmationID { get; set; }
    public string DLVMode { get; set; }
    public string Quantity { get; set; }
    public string Unit { get; set; }
    public int Linenum { get; set; }
    public string AFMVendorID { get; set; }

}

/// <summary>
/// Below classes generated based on XML structure
/// </summary>
public class ServiceLevelDeliveryModes
{
    public ServiceItem HD { get; set; }
    public ServiceItem DS { get; set; }
}

public class ServiceItem
{
    public string lineConfirmationId { get; set; }
    public decimal netPrice { get; set; }
    public decimal tax { get; set; }
    public decimal grossPrice { get; set; }

}

public class TenderLine
{
    public decimal amount { get; set; }
    public string currency { get; set; }
    public string authorizationId { get; set; }
    public string type { get; set; }
    public string lineConfirmationId { get; set; }
    public int lineNum { get; set; }
    public string CardNumber { get; set; }
    public string CardType { get; set;}
    private string _paymentauthorization = null;

    public string PaymentAuthorization { get; set; }
    /**
    public string PaymentAuthorization
    {
        get
        {

            string fileName = ConfigurationManager.AppSettings["AuthorizeXml"] != null ? ConfigurationManager.AppSettings["AuthorizeXml"].ToString() : string.Empty;
            string text = File.Exists(fileName) ? System.IO.File.ReadAllText(fileName) : string.Empty;

            return text;
        }
        set { _paymentauthorization = value; }
    }*/
}

public partial class PaymentTransaction
{
    public int LineNum;

    public decimal Amount { get; set; }
    public List<TenderLine> TenderData { get; set; }
    public string CardNumber { get; set; }
    public string CardType { get; set; }
    public Type PaymentType { get; set; }

    public decimal ExpireYear { get; set; }

    public decimal ExpireMonth { get; set; }

    public string CardHolderName { get; set; }

    public string TransactionId { get; set; }

    public string BillingStreet { get; set; }

    public string BillingCity { get; set; }

    public string BillingState { get; set; }

    public string BillingZipcode { get; set; }

}

public class Properties
{
    public string accountNumber { get; set; }
    public string shipTo { get; set; }
    public string skus { get; set; }
}

public class Entity
{
    public string sku { get; set; }
    public string message { get; set; }
    public int quantity { get; set; }
    public DateTime bestDate { get; set; }
    public DateTime beginDateRange { get; set; }
    public DateTime endDateRange { get; set; }
    public bool isAvailable { get; set; }
    public string AFMVendorID { get; set; }
}

public class ATP
{
    public Properties properties { get; set; }
    public List<Entity> entities { get; set; }
}