BEGIN TRY
DECLARE @Error_Number  INT,
        @ERROR_SEVERITY INT ,
		@Error_Module NVARCHAR(150),
		@Error_Message NVARCHAR(max),
		@Error_Line INT,
		@Error_State INT,
		@End_Message NVARCHAR(max)
-- Declaring constant values as per the analysis in current system
DECLARE @Channel BIGINT = 5637144576,
        @DataReadID NVARCHAR(10)= 'ecm',
		@InventLocationID nvarchar(20)= '7250',
		@TAXGROUP nvarchar(20) = 'AVATAX',
		@TAXITEMGROUP nvarchar(20) = 'AVATAX-ITM',
		@IsNegativeValue INT = -1,
		@instancerelationtype BIGINT = 2975,
		@Languageid VARCHAR(10) = 'en-us',
		@CustGroup NVARCHAR(10) = 'DEFAULT',
		@PaymMode NVARCHAR(10) = 'CRED', 
		@PaymTermid NVARCHAR(10) = 'CCNet0',
		@AFMServiceItemType INT = 1, -- Default value for service items
		@AFMProductItemType INT = 0, -- Default value for product items
		@partyNumber nvarchar(20),
		@PartyRecID BIGINT,
		@TotalNetAmount NUMERIC,
		@NoofItemLines NUMERIC,
		@NoofPaymentLines INT,
		@GrossAmount NUMERIC,
		@NoofItems NUMERIC,
		@SalesPaymentDifference NUMERIC,
		@DLVMode NVARCHAR(40) = 'HD', -- For now this is constant as i didn't side any other delivery mode in transaction table
		@Tendertype int = 1,
		@TransQty numeric = 1.0000000000000000,
		@DefaultAffiliation BIGINT = 5637144576,
		@Exchange numeric = 100.00,
		@Afrcustomershownatp INT = 0,
		@Afmisavailable INT = 1,
		@Afrisscheduledonline INT = 0,
		@Type INT = 27,
		@AfmisAcceptTermsandConditions INT= 1,
		@AfmIsMultiAuthAllowed INT = 1


-- Declaring auto calculated fields
DECLARE @TranID uniqueidentifier,
        @LocationID BIGINT,
		@postalRecid BIGINT 
IF @@TRANCOUNT = 0  
BEGIN
BEGIN TRANSACTION
SET @TranID = newID()

-- Inserting customer data
IF EXISTS (SELECT TOP 1 * FROM [ax].[RETAILTRANSACTIONTABLE] WHERE CHANNELREFERENCEID = @orderno)
BEGIN
 SET @RES = 'false'
 SELECT @RES
 RETURN
END
ELSE
BEGIN

SET ansi_warnings OFF
-- Update SFCC customer number based on AX customer number
UPDATE [ax].[CUSTTABLE] SET [SFCCACCOUNTNUM] = @customerno WHERE ACCOUNTNUM = @AXCustomerAccountNum

-- Auto calculated fields
SELECT @TotalNetAmount = SUM(plt.netprice) + SUM(slt.netprice) FROM  @ProductLineItems plt, @ServiceLineItem slt
SELECT @NoofItemLines =  count(plt.lineNum)+count(slt.lineNum) FROM  @ProductLineItems plt, @ServiceLineItem slt
SELECT @NoofPaymentLines = COUNT(pmt.lineNum) FROM @Payments pmt
SELECT @GrossAmount = @TotalNetAmount+SUM(plt.tax) + SUM(slt.tax) FROM  @ProductLineItems plt, @ServiceLineItem slt
SELECT @NoofItems =  sum(plt.Quantity) + sum(slt.Quantity) FROM  @ProductLineItems plt, @ServiceLineItem slt
SELECT @SalesPaymentDifference = SUM(AMOUNT) FROM @Payments
 

-- insert transaction data
INSERT INTO [ax].[RETAILTRANSACTIONTABLE](CHANNELREFERENCEID,TYPE,BUSINESSDATE,TRANSDATE,INVENTLOCATIONID,EXCHRATE,CHANNEL,CUSTACCOUNT,RECEIPTEMAIL,CURRENCY,DATAAREAID,NETAMOUNT,NUMBEROFITEMLINES,NUMBEROFPAYMENTLINES,GROSSAMOUNT,NUMBEROFITEMS,SALESPAYMENTDIFFERENCE,LOGISTICSPOSTALADDRESS,DLVMode,TRANSACTIONID,AFRCUSTOMERSHOWNATP,AFMISACCEPTTERMSANDCONDITIONS,AFMISMULTIAUTHALLOWED,AFMCARDTYPE,ShippingDateRequested,CREATEDDATETIME, MODIFIEDDATETIME,ReceiptDateRequested) 
values(@orderno,@Type,getdate(),getdate(),@InventLocationID,@Exchange,@Channel,@AXCustomerAccountNum,@customeremail,@currency,@DataReadID,@IsNegativeValue*@TotalNetAmount,@NoofItemLines,@NoofPaymentLines,@IsNegativeValue*@GrossAmount,@NoofItems,@SalesPaymentDifference,@logisticPostalAddressID,@DLVMode,@TranID,@Afrcustomershownatp,@AfmisAcceptTermsandConditions,@AfmIsMultiAuthAllowed,(SELECT TOP 1 cardtype FROM @Payments),getdate()+10,getdate(),getdate(),getdate()+10);

--Insert payment transactions
INSERT INTO  [ax].[RETAILTRANSACTIONPAYMENTTRANS] (TRANSACTIONID,LINENUM,CARDTYPEID,AMOUNTTENDERED,CURRENCY,CARDORACCOUNT,QTY,TENDERTYPE,DATAAREAID,CHANNEL,PAYMENTAUTHORIZATION,AFMLINECONFIRMATIONID) 
SELECT @TranID,lineNum,cardtype,amount,currency,cardnumber,@TransQty,@Tendertype,@DataReadID,@Channel,PaymentAuthorization,lineConfirmationId  FROM @Payments

-- Insert product line items
INSERT INTO [ax].[RETAILTRANSACTIONSALESTRANS](CHANNEL,CUSTACCOUNT,INVENTLOCATIONID,ITEMID,AFMItemtype,LINENUM,PRICE,TAXAMOUNT,NETAMOUNT,NETAMOUNTINCLTAX,QTY,TAXGROUP,TAXITEMGROUP,TRANSACTIONID,UNIT,AFMLINECONFIRMATIONID,AFMBEGINDATERANGE,
AFMBESTDATE,
AFMENDDATERANGE,
AFMMESSAGE,AFMVENDACCOUNT,DATAAREAID,AFMISAVAILABLE,AFRISSCHEDULEDONLINE,DLVMODE,LOGISTICSPOSTALADDRESS,AFMKITITEMID,AFMKITSEQUENCENUMBER,AFMKITITEMQUANTITY,ShippingDateRequested,ReceiptDateRequested) 
SELECT @Channel,@AXCustomerAccountNum,@InventLocationID,productid,@AFMProductItemType,Linenum,netprice,@IsNegativeValue*tax,@IsNegativeValue*grossprice,@IsNegativeValue*grossprice,@IsNegativeValue*Quantity,@TAXGROUP,@TAXITEMGROUP,@TranID,Unit,AFMLineConfirmationID,
BeginDateRange,
BestDate,
EndDateRange,
AFMMessage,AFMVendorID,@DataReadID,@Afmisavailable,@Afrisscheduledonline,DLVMode,@logisticPostalAddressID,ISNULL(AFMKitItemID,''),ISNULL(AFMKitSequenceNumber,0),ISNULL(AFMKitItemQuantity,0),getdate()+10,getdate()+10 from @ProductLineItems

-- Insert service items
INSERT INTO [ax].[RETAILTRANSACTIONSALESTRANS](CHANNEL,CUSTACCOUNT,INVENTLOCATIONID,ITEMID,AFMItemtype,LINENUM,NETAMOUNT,NETPRICE,PRICE,TAXAMOUNT,NETAMOUNTINCLTAX,QTY,TAXGROUP,TAXITEMGROUP,TRANSACTIONID,UNIT,AFMLINECONFIRMATIONID,DATAAREAID,TRANSDATE,AFMVENDACCOUNT,AFMISAVAILABLE,AFRISSCHEDULEDONLINE,DLVMODE,LOGISTICSPOSTALADDRESS,ShippingDateRequested,ReceiptDateRequested) 
SELECT @Channel,@AXCustomerAccountNum,@InventLocationID,itemid,@AFMServiceItemType,Linenum,@IsNegativeValue*grossprice,netprice,netprice,@IsNegativeValue*tax,@IsNegativeValue*grossprice,@IsNegativeValue*1,@TAXGROUP,@TAXITEMGROUP,@TranID,Unit,AFMLineConfirmationID,@DataReadID,getdate(),AFMVendorID,@Afmisavailable,@Afrisscheduledonline,DLVMode,@logisticPostalAddressID,getdate()+10,getdate()+10 from @ServiceLineItem

--Insert Affiliation details
INSERT INTO [ax].[RETAILTRANSACTIONAFFILIATIONTRANS] (AFFILIATION,CHANNEL,TRANSACTIONID,CREATEDDATETIME,MODIFIEDDATETIME,DATAAREAID)
SELECT @DefaultAffiliation,@Channel,@TranID,getdate(),getdate(),@DataReadID

END
COMMIT 
IF( @@Trancount <> 0 )
SET @RES = 'false';
SELECT @RES
END
END TRY
BEGIN CATCH
IF @@TRANCOUNT>0
ROLLBACK
SELECT @RES
          --Setting the error details into variables                         
          SELECT @Error_Number = Error_number(), 
                 @Error_Severity = Error_severity(), 
                 @Error_Module = 'Agr.OrderImport - '+@orderno, 
                 @Error_Message = Error_message(), 
                 @Error_Line = Error_line(), 
                 @Error_State = Error_state() 

         --Executing the Sproc to insert the logs details in [dbo].[ERRORLogDetail] table    
          EXEC [dbo].[LOGERRORDETAILS] 
            @Error_Number, 
            @Error_Severity, 
            @Error_Module, 
            @Error_Message, 
            @Error_Line, 
            @Error_State, 
            @End_Message out 

RAISERROR (@Error_Message,@Error_Severity,@Error_State) 
END CATCH

-- BELOW ARE HARDCODED AS WE DON'T HAVE LATEST XML DATA
---------------------------------------------------------
---------------------------------------------------------
--COMMENTS
-- AFMLineConfirmationID hard coded for now
-- lINENUM in paymenttrans is hard coded
-- lINENUM in RETAILTRANSACTIONSALESTRANS is hard coded
-- UNIT in RETAILTRANSACTIONSALESTRANS is hard coded
-- Quantity in a product line hard coded for now as structure needs to be changed from Lyon team 
-- CARD TYPEID is hard coded
-- Negative sign appended hard coded as those are receivables from the system
-- Transaction ID generated here temporarly it should read from lyons if they share it in XML file
---------------------------------------------------------
---------------------------------------------------------

