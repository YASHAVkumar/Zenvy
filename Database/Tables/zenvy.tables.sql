CREATE DATABASE Zenvy;
GO

USE Zenvy;
GO

/* =====================================================
   ROLE
===================================================== */
CREATE TABLE Roles
(
    RoleId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Description NVARCHAR(255),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

/* =====================================================
   USER
===================================================== */
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nchar](150) NOT NULL,
	[RoleId] [int] NOT NULL,
	[Role] [nchar](100) NULL,
	[FullName] [nvarchar](150) NOT NULL,
	[Email] [nvarchar](150) NOT NULL,
	[Phone] [nvarchar](20) NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK__Users__1788CC4CCBC9A35C] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__IsActive__4F7CD00D]  DEFAULT ((1)) FOR [IsActive]
GO

ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF__Users__CreatedAt__5070F446]  DEFAULT (getdate()) FOR [CreatedAt]
GO

 ALTER TABLE [dbo].[Users] ADD   CONSTRAINT FK_Users_Roles
        FOREIGN KEY(RoleId)
        REFERENCES Roles(RoleId)

GO
ALTER TABLE users
ADD CONSTRAINT FK_UserId_Roles UNIQUE (userid);
GO

/* =====================================================
   CATEGORY
===================================================== */
CREATE TABLE Categories
(
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

/* =====================================================
   BRAND
===================================================== */
CREATE TABLE Brands
(
    BrandId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

/* =====================================================
   SALES CHANNEL
===================================================== */
CREATE TABLE SalesChannels
(
    ChannelId INT IDENTITY(1,1) PRIMARY KEY,
    ChannelName NVARCHAR(100) NOT NULL,
    ChannelType NVARCHAR(50),
    Status BIT NOT NULL DEFAULT 1
);
GO

/* =====================================================
   CUSTOMER
===================================================== */
CREATE TABLE Customers
(
    CustomerId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address1 NVARCHAR(255),
    Address2 NVARCHAR(255),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Pincode NVARCHAR(20),
    Country NVARCHAR(100),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

/* =====================================================
   SUPPLIER
===================================================== */
CREATE TABLE Suppliers
(
    SupplierId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(150) NOT NULL,
    ContactPerson NVARCHAR(100),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    Status BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

/* =====================================================
   PRODUCT MASTER
===================================================== */
CREATE TABLE ProductMasters
(
    ProductMasterId INT IDENTITY(1,1) PRIMARY KEY,
    ProductCode NVARCHAR(50) NOT NULL UNIQUE,
    ProductName NVARCHAR(250) NOT NULL,
    CategoryId INT NOT NULL,
    BrandId INT NULL,
    Description NVARCHAR(MAX),
    HSNCode NVARCHAR(20),
    GSTPercentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedBy varchar(150) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ProductMaster_Category
        FOREIGN KEY(CategoryId)
        REFERENCES Categories(CategoryId),

    CONSTRAINT FK_ProductMaster_Brand
        FOREIGN KEY(BrandId)
        REFERENCES Brands(BrandId)
);
GO

/* =====================================================
   PRODUCT
===================================================== */
CREATE TABLE Products
(
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    ProductMasterId INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Product_ProductMaster
        FOREIGN KEY(ProductMasterId)
        REFERENCES ProductMasters(ProductMasterId)
);
GO

/* =====================================================
   PRODUCT VARIANT
===================================================== */
CREATE TABLE ProductVariants
(
    VariantId INT IDENTITY(1,1) PRIMARY KEY,
    ProductMasterId INT NOT NULL,
    ProductId INT NOT NULL,

    SKU NVARCHAR(100) NOT NULL UNIQUE,
    Barcode NVARCHAR(100),

    Size NVARCHAR(50),
    Color NVARCHAR(50),
    Material NVARCHAR(50),
    Gender NVARCHAR(20),
    Season NVARCHAR(20),

    CurrentPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_Variant_ProductMaster
        FOREIGN KEY(ProductMasterId)
        REFERENCES ProductMasters(ProductMasterId),

    CONSTRAINT FK_Variant_Product
        FOREIGN KEY(ProductId)
        REFERENCES Products(ProductId)
);
GO

/* =====================================================
   PRODUCT IMAGE
===================================================== */
CREATE TABLE ProductImages
(
    ImageId INT IDENTITY(1,1) PRIMARY KEY,
    ProductMasterId INT NOT NULL,
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsPrimary BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_ProductImages_ProductMaster
        FOREIGN KEY(ProductMasterId)
        REFERENCES ProductMasters(ProductMasterId),

    CONSTRAINT FK_ProductImages_Product
        FOREIGN KEY(ProductId)
        REFERENCES Products(ProductId)
);
GO

CREATE TYPE dbo.ProductVariantType AS TABLE
(
    SKU NVARCHAR(100),
    Barcode NVARCHAR(100),
    Size NVARCHAR(50),
    Color NVARCHAR(50),
    Material NVARCHAR(50),
    Gender NVARCHAR(20),
    Season NVARCHAR(20),
    CurrentPrice DECIMAL(18,2),
    Status BIT
);
GO

CREATE TYPE dbo.ProductImageType AS TABLE
(
    ImageUrl NVARCHAR(500),
    IsPrimary BIT
);
GO

/* =====================================================
   PRICE
===================================================== */
CREATE TABLE Prices
(
    PriceId INT IDENTITY(1,1) PRIMARY KEY,
    VariantId INT NOT NULL,

    CostPrice DECIMAL(18,2) NOT NULL,
    SalePrice DECIMAL(18,2) NOT NULL,

    EffectiveFrom DATETIME2 NOT NULL,
    EffectiveTo DATETIME2 NULL,

    CONSTRAINT FK_Prices_Variant
        FOREIGN KEY(VariantId)
        REFERENCES ProductVariants(VariantId)
);
GO

/* =====================================================
   WAREHOUSE
===================================================== */
CREATE TABLE Warehouses
(
    WarehouseId INT IDENTITY(1,1) PRIMARY KEY,
    WarehouseName NVARCHAR(150) NOT NULL,
    Location NVARCHAR(255),
    Status BIT NOT NULL DEFAULT 1
);
GO

/* =====================================================
   INVENTORY
===================================================== */
CREATE TABLE Inventory
(
    InventoryId INT IDENTITY(1,1) PRIMARY KEY,

    VariantId INT NOT NULL,
    WarehouseId INT NOT NULL,

    OnHandQty INT NOT NULL DEFAULT 0,
    ReservedQty INT NOT NULL DEFAULT 0,
    AvailableQty AS (OnHandQty - ReservedQty),

    CONSTRAINT FK_Inventory_Variant
        FOREIGN KEY(VariantId)
        REFERENCES ProductVariants(VariantId),

    CONSTRAINT FK_Inventory_Warehouse
        FOREIGN KEY(WarehouseId)
        REFERENCES Warehouses(WarehouseId)
);
GO

/* =====================================================
   INVENTORY TRANSACTION
===================================================== */
CREATE TABLE InventoryTransactions
(
    TransactionId BIGINT IDENTITY(1,1) PRIMARY KEY,

    VariantId INT NOT NULL,
    WarehouseId INT NOT NULL,

    TransactionType NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,

    ReferenceType NVARCHAR(50),
    ReferenceId BIGINT,

    CreatedBy varchar(150) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_InventoryTransaction_Variant
        FOREIGN KEY(VariantId)
        REFERENCES ProductVariants(VariantId),

    CONSTRAINT FK_InventoryTransaction_Warehouse
        FOREIGN KEY(WarehouseId)
        REFERENCES Warehouses(WarehouseId)
);
GO

/* =====================================================
   PAYMENT METHOD
===================================================== */
CREATE TABLE PaymentMethods
(
    PaymentMethodId INT IDENTITY(1,1) PRIMARY KEY,
    MethodName NVARCHAR(50) NOT NULL,
    Status BIT NOT NULL DEFAULT 1
);
GO

/* =====================================================
   EXPENSE TYPE
===================================================== */
CREATE TABLE ExpenseTypes
(
    ExpenseTypeId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);
GO

/* =====================================================
   INVESTOR
===================================================== */
CREATE TABLE Investors
(
    InvestorId INT IDENTITY(1,1) PRIMARY KEY,

    Name NVARCHAR(150) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),

    InvestmentAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    OwnershipPercent DECIMAL(5,2) NOT NULL,

    JoinDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status BIT NOT NULL DEFAULT 1
);
GO

/* =====================================================
   SEED DATA
===================================================== */

INSERT INTO Roles(Name)
VALUES
('Admin'),
('Manager'),
('InventoryManager'),
('Accountant'),
('SalesPerson');

INSERT INTO SalesChannels(ChannelName)
VALUES
('Meesho'),
('Myntra'),
('Amazon'),
('Flipkart'),
('Website'),
('Walk-In');

INSERT INTO PaymentMethods(MethodName)
VALUES
('Cash'),
('UPI'),
('Card'),
('Bank Transfer');

INSERT INTO ExpenseTypes(Name)
VALUES
('Rent'),
('Salary'),
('Electricity'),
('Internet'),
('Transport'),
('Packaging'),
('Marketing');

GO

/* ============================================
   PURCHASE ORDER
============================================ */

CREATE TABLE PurchaseOrders
(
    POId BIGINT IDENTITY(1,1) PRIMARY KEY,

    SupplierId INT NOT NULL,
    WarehouseId INT NOT NULL,

    PONumber NVARCHAR(50) NOT NULL UNIQUE,

    OrderDate DATETIME2 NOT NULL,
    ExpectedDate DATETIME2 NULL,

    Status NVARCHAR(50) NOT NULL,

    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    GrandTotal DECIMAL(18,2) NOT NULL DEFAULT 0,

    CreatedBy varchar(150) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_PO_Supplier
        FOREIGN KEY (SupplierId)
        REFERENCES Suppliers(SupplierId),

    CONSTRAINT FK_PO_Warehouse
        FOREIGN KEY (WarehouseId)
        REFERENCES Warehouses(WarehouseId),

    CONSTRAINT FK_PO_User
        FOREIGN KEY (CreatedBy)
        REFERENCES Users(UserId)
);
GO

/* ============================================
   PURCHASE ORDER LINE
============================================ */

CREATE TABLE PurchaseOrderLines
(
    POLineId BIGINT IDENTITY(1,1) PRIMARY KEY,

    POId BIGINT NOT NULL,
    VariantId INT NOT NULL,

    Qty INT NOT NULL,
    UnitCost DECIMAL(18,2) NOT NULL,

    TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

    LineTotal AS ((Qty * UnitCost) + TaxAmount),

    CONSTRAINT FK_POL_PO
        FOREIGN KEY (POId)
        REFERENCES PurchaseOrders(POId),

    CONSTRAINT FK_POL_Variant
        FOREIGN KEY (VariantId)
        REFERENCES ProductVariants(VariantId)
);
GO

/* ============================================
   SALES ORDER
============================================ */

CREATE TABLE SalesOrders
(
    OrderId BIGINT IDENTITY(1,1) PRIMARY KEY,

    CustomerId INT NULL,
    ChannelId INT NOT NULL,

    CreatedBy varchar(150) NOT NULL,

    ExternalOrderId NVARCHAR(100),

    OrderDate DATETIME2 NOT NULL,

    Status NVARCHAR(50) NOT NULL,

    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Tax DECIMAL(18,2) NOT NULL DEFAULT 0,
    ShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,

    GrandTotal DECIMAL(18,2) NOT NULL DEFAULT 0,

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_SO_Customer
        FOREIGN KEY(CustomerId)
        REFERENCES Customers(CustomerId),

    CONSTRAINT FK_SO_Channel
        FOREIGN KEY(ChannelId)
        REFERENCES SalesChannels(ChannelId),

    CONSTRAINT FK_SO_User
        FOREIGN KEY(CreatedBy)
        REFERENCES Users(UserId)
);
GO

/* ============================================
   SALES ORDER LINE
============================================ */

CREATE TABLE SalesOrderLines
(
    OrderLineId BIGINT IDENTITY(1,1) PRIMARY KEY,

    OrderId BIGINT NOT NULL,
    VariantId INT NOT NULL,

    Qty INT NOT NULL,

    UnitPrice DECIMAL(18,2) NOT NULL,

    Discount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Tax DECIMAL(18,2) NOT NULL DEFAULT 0,

    LineTotal AS
    (
        ((Qty * UnitPrice) - Discount + Tax)
    ),

    CONSTRAINT FK_SOL_Order
        FOREIGN KEY(OrderId)
        REFERENCES SalesOrders(OrderId),

    CONSTRAINT FK_SOL_Variant
        FOREIGN KEY(VariantId)
        REFERENCES ProductVariants(VariantId)
);
GO

/* ============================================
   PAYMENT
============================================ */

CREATE TABLE Payments
(
    PaymentId BIGINT IDENTITY(1,1) PRIMARY KEY,

    OrderId BIGINT NOT NULL,

    PaymentMethodId INT NOT NULL,

    Amount DECIMAL(18,2) NOT NULL,

    TransactionRef NVARCHAR(200),

    Status NVARCHAR(50),

    PaymentDate DATETIME2 NOT NULL,

    CONSTRAINT FK_Payment_Order
        FOREIGN KEY(OrderId)
        REFERENCES SalesOrders(OrderId),

    CONSTRAINT FK_Payment_Method
        FOREIGN KEY(PaymentMethodId)
        REFERENCES PaymentMethods(PaymentMethodId)
);
GO

/* ============================================
   SHIPMENT
============================================ */

CREATE TABLE Shipments
(
    ShipmentId BIGINT IDENTITY(1,1) PRIMARY KEY,

    OrderId BIGINT NOT NULL,

    CourierName NVARCHAR(100),

    TrackingNumber NVARCHAR(100),

    ShippedDate DATETIME2 NULL,
    DeliveredDate DATETIME2 NULL,

    Status NVARCHAR(50),

    CONSTRAINT FK_Shipment_Order
        FOREIGN KEY(OrderId)
        REFERENCES SalesOrders(OrderId)
);
GO

/* ============================================
   MARKETPLACE SETTLEMENT
============================================ */

CREATE TABLE MarketplaceSettlements
(
    SettlementId BIGINT IDENTITY(1,1) PRIMARY KEY,

    OrderId BIGINT NOT NULL,
    ChannelId INT NOT NULL,

    SaleAmount DECIMAL(18,2) NOT NULL,

    MarketplaceCommission DECIMAL(18,2) NOT NULL DEFAULT 0,

    ShippingCharge DECIMAL(18,2) NOT NULL DEFAULT 0,

    TaxDeduction DECIMAL(18,2) NOT NULL DEFAULT 0,

    NetReceived DECIMAL(18,2) NOT NULL,

    SettlementDate DATETIME2 NULL,

    Status NVARCHAR(50),

    CONSTRAINT FK_MS_Order
        FOREIGN KEY(OrderId)
        REFERENCES SalesOrders(OrderId),

    CONSTRAINT FK_MS_Channel
        FOREIGN KEY(ChannelId)
        REFERENCES SalesChannels(ChannelId)
);
GO

/* ============================================
   SALES RETURN
============================================ */

CREATE TABLE SalesReturns
(
    ReturnId BIGINT IDENTITY(1,1) PRIMARY KEY,

    OrderId BIGINT NOT NULL,

    ReturnDate DATETIME2 NOT NULL,

    Reason NVARCHAR(500),

    Status NVARCHAR(50) NOT NULL DEFAULT 'REQUESTED',
    RefundStatus NVARCHAR(50) NOT NULL DEFAULT 'PENDING',
    RefundMethod NVARCHAR(50),
    RefundAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    ReturnShippingFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    MarketplaceFee DECIMAL(18,2) NOT NULL DEFAULT 0,
    DeliveryFeeRefunded BIT NOT NULL DEFAULT 0,
    CreatedBy varchar(150) NULL,
    Notes NVARCHAR(500),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Return_Order
        FOREIGN KEY(OrderId)
        REFERENCES SalesOrders(OrderId)
);
GO

/* ============================================
   SALES RETURN LINE
============================================ */

CREATE TABLE SalesReturnLines
(
    ReturnLineId BIGINT IDENTITY(1,1) PRIMARY KEY,

    ReturnId BIGINT NOT NULL,
    OrderLineId BIGINT NOT NULL,

    VariantId INT NOT NULL,

    Qty INT NOT NULL,
    RefundAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Condition] NVARCHAR(50),
    Restock BIT NOT NULL DEFAULT 1,
    RestockWarehouseId INT NULL,

    CONSTRAINT FK_ReturnLine_Return
        FOREIGN KEY(ReturnId)
        REFERENCES SalesReturns(ReturnId),

    CONSTRAINT FK_ReturnLine_OrderLine
        FOREIGN KEY(OrderLineId)
        REFERENCES SalesOrderLines(OrderLineId),

    CONSTRAINT FK_ReturnLine_Variant
        FOREIGN KEY(VariantId)
        REFERENCES ProductVariants(VariantId),

    CONSTRAINT FK_ReturnLine_Warehouse
        FOREIGN KEY(RestockWarehouseId)
        REFERENCES Warehouses(WarehouseId)
);
GO

/* ============================================
   EXPENSE
============================================ */

CREATE TABLE Expenses
(
    ExpenseId BIGINT IDENTITY(1,1) PRIMARY KEY,

    ExpenseTypeId INT NOT NULL,

    Amount DECIMAL(18,2) NOT NULL,

    Description NVARCHAR(500),

    ExpenseDate DATETIME2 NOT NULL,

    CreatedBy varchar(150) NOT NULL,

    CONSTRAINT FK_Expense_Type
        FOREIGN KEY(ExpenseTypeId)
        REFERENCES ExpenseTypes(ExpenseTypeId),

    CONSTRAINT FK_Expense_User
        FOREIGN KEY(CreatedBy)
        REFERENCES Users(UserId)
);
GO

/* ============================================
   EMPLOYEE COMMISSION
============================================ */

CREATE TABLE EmployeeCommissions
(
    CommissionId BIGINT IDENTITY(1,1) PRIMARY KEY,

    UserId varchar(150) NOT NULL,

    OrderId BIGINT NOT NULL,

    CommissionPercent DECIMAL(5,2),

    CommissionAmount DECIMAL(18,2),

    CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_Commission_User
        FOREIGN KEY(UserId)
        REFERENCES Users(UserId),

    CONSTRAINT FK_Commission_Order
        FOREIGN KEY(OrderId)
        REFERENCES SalesOrders(OrderId)
);
GO

/* ============================================
   INVESTOR TRANSACTION
============================================ */

CREATE TABLE InvestorTransactions
(
    TransactionId BIGINT IDENTITY(1,1) PRIMARY KEY,

    InvestorId INT NOT NULL,

    TransactionType NVARCHAR(50) NOT NULL,

    Amount DECIMAL(18,2) NOT NULL,

    TransactionDate DATETIME2 NOT NULL,

    Notes NVARCHAR(500),

    CONSTRAINT FK_InvestorTransaction
        FOREIGN KEY(InvestorId)
        REFERENCES Investors(InvestorId)
);
GO

/* ============================================
   PROFIT DISTRIBUTION
============================================ */

CREATE TABLE ProfitDistributions
(
    DistributionId BIGINT IDENTITY(1,1) PRIMARY KEY,

    InvestorId INT NOT NULL,

    [Month] TINYINT NOT NULL,
    [Year] SMALLINT NOT NULL,

    ProfitAmount DECIMAL(18,2) NOT NULL,

    DistributedDate DATETIME2,

    Notes NVARCHAR(500),

    CONSTRAINT FK_ProfitDistribution
        FOREIGN KEY(InvestorId)
        REFERENCES Investors(InvestorId)
);
GO

/* ============================================
   API AUDIT LOG
============================================ */
CREATE TABLE ApiAuditLogs
(
    AuditId BIGINT IDENTITY(1,1) PRIMARY KEY,
    TraceId NVARCHAR(100) NOT NULL,
    UserId varchar(150) NULL,
    HttpMethod NVARCHAR(10) NOT NULL,
    Path NVARCHAR(2048) NOT NULL,
    QueryString NVARCHAR(2048) NULL,
    StatusCode INT NOT NULL,
    DurationMs BIGINT NOT NULL,
    IpAddress NVARCHAR(64) NULL,
    UserAgent NVARCHAR(512) NULL,
    ExceptionType NVARCHAR(500) NULL,
    ErrorMessage NVARCHAR(2048) NULL,
    ErrorDetails NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_ApiAuditLogs_CreatedAt DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_ApiAuditLogs_User FOREIGN KEY(UserId) REFERENCES Users(UserId)
);
GO

CREATE INDEX IX_ApiAuditLogs_CreatedAt ON ApiAuditLogs(CreatedAt DESC);
CREATE INDEX IX_ApiAuditLogs_TraceId ON ApiAuditLogs(TraceId);
CREATE INDEX IX_ApiAuditLogs_UserId_CreatedAt ON ApiAuditLogs(UserId, CreatedAt DESC);
GO
