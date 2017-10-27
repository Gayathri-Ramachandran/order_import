USE [Your Channel DB Connection string]
GO

/****** Object:  UserDefinedTableType [dbo].[tvpBillingAddress]    Script Date: 10/18/2017 9:18:33 AM ******/
CREATE TYPE [dbo].[tvpBillingAddress] AS TABLE(
	[firstname] [nvarchar](50) NULL,
	[lastname] [nvarchar](50) NULL,
	[address1] [nvarchar](50) NULL,
	[city] [nvarchar](50) NULL,
	[postalcode] [nvarchar](50) NULL,
	[statecode] [nvarchar](50) NULL,
	[countrycode] [nvarchar](50) NULL,
	[phone] [nvarchar](50) NULL
)
GO


CREATE TYPE [dbo].[tvpPayments] AS TABLE(
	[amount] [numeric](18, 0) NULL,
	[cardtype] [nvarchar](50) NULL,
	[cardnumber] [nvarchar](50) NULL,
	[currency] [nvarchar](50) NULL,
	[authorizationId] [nvarchar](50) NULL,
	[lineNum] [int] NULL
)
GO

CREATE TYPE [dbo].[tvpProductLineItmes] AS TABLE(
	[productid] [nvarchar](50) NULL,
	[netprice] [numeric](18, 0) NULL,
	[tax] [numeric](18, 0) NULL,
	[grossprice] [numeric](18, 0) NULL,
	[AFMLineConfirmationID] [nvarchar](50) NULL,
	[DLVMode] [nvarchar](50) NULL,
	[Quantity] [numeric](18, 0) NULL,
	[Unit] [nvarchar](50) NULL,
	[LINENUM] [numeric](18, 0) NULL,
	[FulfillerName] [nvarchar](100) NULL,
	[BestDate] [datetime] NULL,
	[BeginDateRange] [datetime] NULL,
	[EndDateRange] [datetime] NULL,
	[AFMMessage] [nvarchar](100) NULL,
	[AFMVendorID] [nvarchar](20) NULL,
	[AFMKitItemID] [nvarchar](50) NULL,
	[AFMKitSequenceNumber] [int] NULL,
	[AFMKitItemQuantity] [numeric](18, 0) NULL
)
GO

CREATE TYPE [dbo].[tvpServiceLineItmes] AS TABLE(
	[itemId] [nvarchar](50) NULL,
	[netprice] [numeric](18, 0) NULL,
	[tax] [numeric](18, 0) NULL,
	[grossprice] [numeric](18, 0) NULL,
	[AFMLineConfirmationID] [nvarchar](50) NULL,
	[DLVMode] [nvarchar](50) NULL,
	[Quantity] [numeric](18, 0) NULL,
	[Unit] [nvarchar](50) NULL,
	[LINENUM] [numeric](18, 0) NULL,
	[AFMVendorID]  [nvarchar](20) NULL
)
GO

CREATE TYPE [dbo].[tvpShippingAddress] AS TABLE(
	[firstname] [nvarchar](50) NULL,
	[lastname] [nvarchar](50) NULL,
	[address1] [nvarchar](50) NULL,
	[city] [nvarchar](50) NULL,
	[postalcode] [nvarchar](50) NULL,
	[statecode] [nvarchar](50) NULL,
	[countrycode] [nvarchar](50) NULL,
	[phone] [nvarchar](50) NULL
)
GO

CREATE TYPE [dbo].[tvpPayments] AS TABLE(
	[amount] [numeric](18, 0) NULL,
	[cardtype] [nvarchar](50) NULL,
	[cardnumber] [nvarchar](50) NULL,
	[currency] [nvarchar](50) NULL,
	[authorizationId] [nvarchar](50) NULL,
	[lineNum] [int] NULL,
	[PaymentAuthorization] [nvarchar](max) NULL,
	[lineConfirmationId] [nvarchar](100)
)
GO