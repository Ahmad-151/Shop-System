USE [master]
GO
/****** Object:  Database [ShopDB]    Script Date: 09/10/2026 10:53:21 م ******/

USE [master]
GO
IF DB_ID(N'ShopDB') IS NULL
    CREATE DATABASE [ShopDB];
GO
ALTER DATABASE [ShopDB] SET RECOVERY SIMPLE;
ALTER DATABASE [ShopDB] SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO
USE [ShopDB]
GO

ALTER DATABASE [ShopDB] SET COMPATIBILITY_LEVEL = 170
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ShopDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ShopDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ShopDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ShopDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ShopDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ShopDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [ShopDB] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ShopDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ShopDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ShopDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ShopDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ShopDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ShopDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ShopDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ShopDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ShopDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [ShopDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ShopDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ShopDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ShopDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ShopDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ShopDB] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [ShopDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ShopDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ShopDB] SET  MULTI_USER 
GO
ALTER DATABASE [ShopDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ShopDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ShopDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ShopDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ShopDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ShopDB] SET OPTIMIZED_LOCKING = OFF 
GO
ALTER DATABASE [ShopDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [ShopDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [ShopDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ShopDB]
GO
/****** Object:  UserDefinedTableType [dbo].[OrderTableType]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE TYPE [dbo].[OrderTableType] AS TABLE(
	[Product_ID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[SFunc_GetCategoryOFProductByID]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   FUNCTION [dbo].[SFunc_GetCategoryOFProductByID](@Product_ID int)
RETURNS nvarchar(100)
AS
BEGIN
    DECLARE @CategoryName nvarchar(100);

    SELECT @CategoryName = c.CategoryName
    FROM dbo.Categories c
    INNER JOIN dbo.Products p ON p.Category_ID = c.Category_ID
    WHERE p.Product_ID = @Product_ID;

    RETURN @CategoryName;
END;
GO
/****** Object:  UserDefinedFunction [dbo].[SFunc_GetDisCount]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   FUNCTION [dbo].[SFunc_GetDisCount](@Order_ID int)
RETURNS int
AS
BEGIN
    DECLARE @Discount int;

    SELECT @Discount = f.DisCount
    FROM dbo.Orders o
    INNER JOIN dbo.FavouriteCustomers f ON f.Person_ID = o.Person_ID
    WHERE o.Order_ID = @Order_ID;

    RETURN COALESCE(@Discount, 0);
END;
GO
/****** Object:  Table [dbo].[OrderItems]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItems](
	[OrderItem_ID] [int] IDENTITY(1,1) NOT NULL,
	[Order_ID] [int] NOT NULL,
	[Product_ID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[TotalPrice]  AS (CONVERT([decimal](18,2),[Quantity]*[UnitPrice])) PERSISTED,
 CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED 
(
	[OrderItem_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_OrderItems_Order_Product] UNIQUE NONCLUSTERED 
(
	[Order_ID] ASC,
	[Product_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  UserDefinedFunction [dbo].[Get_OrderItems_ByOrderID]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   FUNCTION [dbo].[Get_OrderItems_ByOrderID](@Order_ID int)
RETURNS TABLE
AS
RETURN
(
    SELECT OrderItem_ID, Order_ID, Product_ID, Quantity, UnitPrice, TotalPrice
    FROM dbo.OrderItems
    WHERE Order_ID = @Order_ID
);
GO
/****** Object:  Table [dbo].[AuditEvents]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AuditEvents](
	[AuditEventId] [bigint] IDENTITY(1,1) NOT NULL,
	[OccurredAt] [datetime2](3) NOT NULL,
	[ActorId] [int] NULL,
	[ActorRole] [nvarchar](50) NULL,
	[Action] [nvarchar](100) NOT NULL,
	[ResourceType] [nvarchar](100) NOT NULL,
	[ResourceId] [nvarchar](100) NULL,
	[Outcome] [nvarchar](30) NOT NULL,
	[HttpMethod] [nvarchar](10) NOT NULL,
	[Path] [nvarchar](500) NOT NULL,
	[StatusCode] [int] NOT NULL,
	[IpAddress] [nvarchar](64) NULL,
	[CorrelationId] [nvarchar](64) NOT NULL,
	[Details] [nvarchar](max) NULL,
 CONSTRAINT [PK_AuditEvents] PRIMARY KEY CLUSTERED 
(
	[AuditEventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BannedPersons]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BannedPersons](
	[BP_ID] [int] IDENTITY(1,1) NOT NULL,
	[Person_ID] [int] NOT NULL,
	[Reason] [nvarchar](200) NOT NULL,
	[BannedAt] [datetime2](0) NOT NULL,
 CONSTRAINT [PK_BannedPersons] PRIMARY KEY CLUSTERED 
(
	[BP_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[Category_ID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](100) NOT NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[Category_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Categories_CategoryName] UNIQUE NONCLUSTERED 
(
	[CategoryName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Employees]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Employees](
	[Employee_ID] [int] IDENTITY(1,1) NOT NULL,
	[Person_ID] [int] NOT NULL,
	[Section_ID] [int] NOT NULL,
	[Salary] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_Employees] PRIMARY KEY CLUSTERED 
(
	[Employee_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Employees_Person] UNIQUE NONCLUSTERED 
(
	[Person_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FavouriteCustomers]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FavouriteCustomers](
	[FC_ID] [int] IDENTITY(1,1) NOT NULL,
	[Person_ID] [int] NOT NULL,
	[DisCount] [int] NOT NULL,
 CONSTRAINT [PK_FavouriteCustomers] PRIMARY KEY CLUSTERED 
(
	[FC_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_FavouriteCustomers_Person] UNIQUE NONCLUSTERED 
(
	[Person_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InventoryLog]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InventoryLog](
	[Log_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Operation_ID] [int] NOT NULL,
	[Order_ID] [int] NULL,
	[Product_ID] [int] NOT NULL,
	[QuantityChange] [int] NOT NULL,
	[OldQuantity] [int] NOT NULL,
	[NewQuantity] [int] NOT NULL,
	[OldStatus] [nvarchar](20) NULL,
	[NewStatus] [nvarchar](20) NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[Reason] [nvarchar](200) NULL,
 CONSTRAINT [PK_InventoryLog] PRIMARY KEY CLUSTERED 
(
	[Log_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OperationTypes]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OperationTypes](
	[Operation_ID] [int] IDENTITY(1,1) NOT NULL,
	[OperationName] [nvarchar](30) NOT NULL,
 CONSTRAINT [PK_OperationTypes] PRIMARY KEY CLUSTERED 
(
	[Operation_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_OperationTypes_Name] UNIQUE NONCLUSTERED 
(
	[OperationName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.OperationTypes
    WHERE OperationName = N'Sale'
)
BEGIN
    INSERT INTO dbo.OperationTypes (OperationName)
    VALUES (N'Sale');
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.OperationTypes
    WHERE OperationName = N'Restock'
)
BEGIN
    INSERT INTO dbo.OperationTypes (OperationName)
    VALUES (N'Restock');
END;

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.OperationTypes
    WHERE OperationName = N'Return'
)
BEGIN
    INSERT INTO dbo.OperationTypes (OperationName)
    VALUES (N'Return');
END;
GO

/****** Object:  Table [dbo].[OrderIdempotencyKeys]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderIdempotencyKeys](
	[IdempotencyKey_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[Person_ID] [int] NOT NULL,
	[IdempotencyKey] [nvarchar](100) NOT NULL,
	[Order_ID] [int] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[RequestHash] [char](64) NULL,
 CONSTRAINT [PK_OrderIdempotencyKeys] PRIMARY KEY CLUSTERED 
(
	[IdempotencyKey_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_OrderIdempotencyKeys_Person_Key] UNIQUE NONCLUSTERED 
(
	[Person_ID] ASC,
	[IdempotencyKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Order_ID] [int] IDENTITY(1,1) NOT NULL,
	[Person_ID] [int] NOT NULL,
	[CreatedAt] [datetime2](0) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[DeliveredAt] [datetime2](0) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Order_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Persons]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Persons](
	[Person_ID] [int] IDENTITY(1,1) NOT NULL,
	[PersonName] [nvarchar](150) NOT NULL,
	[PhoneNumber] [char](10) NOT NULL,
	[Address] [nvarchar](250) NULL,
	[PasswordHash] [char](60) NULL,
	[Role] [nvarchar](20) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Persons] PRIMARY KEY CLUSTERED 
(
	[Person_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Persons_PhoneNumber] UNIQUE NONCLUSTERED 
(
	[PhoneNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Product_ID] [int] IDENTITY(1,1) NOT NULL,
	[ProductName] [nvarchar](150) NOT NULL,
	[Category_ID] [int] NOT NULL,
	[Description] [nvarchar](1000) NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[RemainingQuantity] [int] NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Product_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RefreshTokens]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RefreshTokens](
	[Token_ID] [int] IDENTITY(1,1) NOT NULL,
	[Person_ID] [int] NOT NULL,
	[ExpiresAt] [datetime2](7) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[RevokedAt] [datetime2](7) NULL,
	[TokenHash] [char](64) NOT NULL,
	[ReplacedByTokenHash] [char](64) NULL,
PRIMARY KEY CLUSTERED 
(
	[Token_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sections]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sections](
	[Section_ID] [int] IDENTITY(1,1) NOT NULL,
	[SectionName] [nvarchar](100) NOT NULL,
	[MaxEmp] [int] NOT NULL,
 CONSTRAINT [PK_Sections] PRIMARY KEY CLUSTERED 
(
	[Section_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Sections_SectionName] UNIQUE NONCLUSTERED 
(
	[SectionName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditEvents_ActorId_OccurredAt]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_AuditEvents_ActorId_OccurredAt] ON [dbo].[AuditEvents]
(
	[ActorId] ASC,
	[OccurredAt] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_AuditEvents_CorrelationId]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_AuditEvents_CorrelationId] ON [dbo].[AuditEvents]
(
	[CorrelationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AuditEvents_OccurredAt]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_AuditEvents_OccurredAt] ON [dbo].[AuditEvents]
(
	[OccurredAt] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_BannedPersons_Person_ID_BannedAt]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_BannedPersons_Person_ID_BannedAt] ON [dbo].[BannedPersons]
(
	[Person_ID] ASC,
	[BannedAt] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_InventoryLog_Product_ID_CreatedAt]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_InventoryLog_Product_ID_CreatedAt] ON [dbo].[InventoryLog]
(
	[Product_ID] ASC,
	[CreatedAt] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItems_Product_ID]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_OrderItems_Product_ID] ON [dbo].[OrderItems]
(
	[Product_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_Person_ID_CreatedAt]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_Orders_Person_ID_CreatedAt] ON [dbo].[Orders]
(
	[Person_ID] ASC,
	[CreatedAt] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_Active_Category]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_Products_Active_Category] ON [dbo].[Products]
(
	[IsActive] ASC,
	[Category_ID] ASC
)
INCLUDE([ProductName],[UnitPrice],[RemainingQuantity]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_Category_ID]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_Products_Category_ID] ON [dbo].[Products]
(
	[Category_ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_Category_ID_IsActive]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_Products_Category_ID_IsActive] ON [dbo].[Products]
(
	[Category_ID] ASC,
	[IsActive] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_RefreshTokens_Person_ID_ExpiresAt]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Person_ID_ExpiresAt] ON [dbo].[RefreshTokens]
(
	[Person_ID] ASC,
	[ExpiresAt] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UX_RefreshTokens_TokenHash]    Script Date: 09/10/2026 10:53:21 م ******/
CREATE UNIQUE NONCLUSTERED INDEX [UX_RefreshTokens_TokenHash] ON [dbo].[RefreshTokens]
(
	[TokenHash] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AuditEvents] ADD  CONSTRAINT [DF_AuditEvents_OccurredAt]  DEFAULT (sysutcdatetime()) FOR [OccurredAt]
GO
ALTER TABLE [dbo].[BannedPersons] ADD  CONSTRAINT [DF_BannedPersons_BannedAt]  DEFAULT (sysutcdatetime()) FOR [BannedAt]
GO
ALTER TABLE [dbo].[Employees] ADD  CONSTRAINT [DF_Employees_Salary]  DEFAULT ((0)) FOR [Salary]
GO
ALTER TABLE [dbo].[InventoryLog] ADD  CONSTRAINT [DF_InventoryLog_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[OrderIdempotencyKeys] ADD  CONSTRAINT [DF_OrderIdempotencyKeys_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_CreatedAt]  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Orders] ADD  CONSTRAINT [DF_Orders_Status]  DEFAULT (N'Pending') FOR [Status]
GO
ALTER TABLE [dbo].[Persons] ADD  CONSTRAINT [DF_Persons_Role]  DEFAULT (N'Customer') FOR [Role]
GO
ALTER TABLE [dbo].[Persons] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_RemainingQuantity]  DEFAULT ((0)) FOR [RemainingQuantity]
GO
ALTER TABLE [dbo].[Products] ADD  CONSTRAINT [DF_Products_IsActive]  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[RefreshTokens] ADD  DEFAULT (sysutcdatetime()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Sections] ADD  CONSTRAINT [DF_Sections_MaxEmp]  DEFAULT ((0)) FOR [MaxEmp]
GO
ALTER TABLE [dbo].[BannedPersons]  WITH CHECK ADD  CONSTRAINT [FK_BannedPersons_Persons] FOREIGN KEY([Person_ID])
REFERENCES [dbo].[Persons] ([Person_ID])
GO
ALTER TABLE [dbo].[BannedPersons] CHECK CONSTRAINT [FK_BannedPersons_Persons]
GO
ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [FK_Employees_Persons] FOREIGN KEY([Person_ID])
REFERENCES [dbo].[Persons] ([Person_ID])
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [FK_Employees_Persons]
GO
ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [FK_Employees_Sections] FOREIGN KEY([Section_ID])
REFERENCES [dbo].[Sections] ([Section_ID])
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [FK_Employees_Sections]
GO
ALTER TABLE [dbo].[FavouriteCustomers]  WITH CHECK ADD  CONSTRAINT [FK_FavouriteCustomers_Persons] FOREIGN KEY([Person_ID])
REFERENCES [dbo].[Persons] ([Person_ID])
GO
ALTER TABLE [dbo].[FavouriteCustomers] CHECK CONSTRAINT [FK_FavouriteCustomers_Persons]
GO
ALTER TABLE [dbo].[InventoryLog]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLog_OperationTypes] FOREIGN KEY([Operation_ID])
REFERENCES [dbo].[OperationTypes] ([Operation_ID])
GO
ALTER TABLE [dbo].[InventoryLog] CHECK CONSTRAINT [FK_InventoryLog_OperationTypes]
GO
ALTER TABLE [dbo].[InventoryLog]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLog_Orders] FOREIGN KEY([Order_ID])
REFERENCES [dbo].[Orders] ([Order_ID])
GO
ALTER TABLE [dbo].[InventoryLog] CHECK CONSTRAINT [FK_InventoryLog_Orders]
GO
ALTER TABLE [dbo].[InventoryLog]  WITH CHECK ADD  CONSTRAINT [FK_InventoryLog_Products] FOREIGN KEY([Product_ID])
REFERENCES [dbo].[Products] ([Product_ID])
GO
ALTER TABLE [dbo].[InventoryLog] CHECK CONSTRAINT [FK_InventoryLog_Products]
GO
ALTER TABLE [dbo].[OrderIdempotencyKeys]  WITH CHECK ADD  CONSTRAINT [FK_OrderIdempotencyKeys_Order] FOREIGN KEY([Order_ID])
REFERENCES [dbo].[Orders] ([Order_ID])
GO
ALTER TABLE [dbo].[OrderIdempotencyKeys] CHECK CONSTRAINT [FK_OrderIdempotencyKeys_Order]
GO
ALTER TABLE [dbo].[OrderIdempotencyKeys]  WITH CHECK ADD  CONSTRAINT [FK_OrderIdempotencyKeys_Person] FOREIGN KEY([Person_ID])
REFERENCES [dbo].[Persons] ([Person_ID])
GO
ALTER TABLE [dbo].[OrderIdempotencyKeys] CHECK CONSTRAINT [FK_OrderIdempotencyKeys_Person]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Orders] FOREIGN KEY([Order_ID])
REFERENCES [dbo].[Orders] ([Order_ID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Orders]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Products] FOREIGN KEY([Product_ID])
REFERENCES [dbo].[Products] ([Product_ID])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Products]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Persons] FOREIGN KEY([Person_ID])
REFERENCES [dbo].[Persons] ([Person_ID])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Persons]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories] FOREIGN KEY([Category_ID])
REFERENCES [dbo].[Categories] ([Category_ID])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories]
GO
ALTER TABLE [dbo].[RefreshTokens]  WITH CHECK ADD FOREIGN KEY([Person_ID])
REFERENCES [dbo].[Persons] ([Person_ID])
GO
ALTER TABLE [dbo].[BannedPersons]  WITH CHECK ADD  CONSTRAINT [CK_BannedPersons_Reason_NotBlank] CHECK  ((len(ltrim(rtrim([Reason])))>=(3)))
GO
ALTER TABLE [dbo].[BannedPersons] CHECK CONSTRAINT [CK_BannedPersons_Reason_NotBlank]
GO
ALTER TABLE [dbo].[Categories]  WITH CHECK ADD  CONSTRAINT [CK_Categories_CategoryName_NotBlank] CHECK  ((len(ltrim(rtrim([CategoryName])))>(0)))
GO
ALTER TABLE [dbo].[Categories] CHECK CONSTRAINT [CK_Categories_CategoryName_NotBlank]
GO
ALTER TABLE [dbo].[Employees]  WITH CHECK ADD  CONSTRAINT [CK_Employees_Salary_NonNegative] CHECK  (([Salary]>=(0)))
GO
ALTER TABLE [dbo].[Employees] CHECK CONSTRAINT [CK_Employees_Salary_NonNegative]
GO
ALTER TABLE [dbo].[FavouriteCustomers]  WITH CHECK ADD  CONSTRAINT [CK_FavouriteCustomers_Discount] CHECK  (([DisCount]>=(1) AND [DisCount]<=(100)))
GO
ALTER TABLE [dbo].[FavouriteCustomers] CHECK CONSTRAINT [CK_FavouriteCustomers_Discount]
GO
ALTER TABLE [dbo].[InventoryLog]  WITH CHECK ADD  CONSTRAINT [CK_InventoryLog_Quantities_NonNegative] CHECK  (([OldQuantity]>=(0) AND [NewQuantity]>=(0)))
GO
ALTER TABLE [dbo].[InventoryLog] CHECK CONSTRAINT [CK_InventoryLog_Quantities_NonNegative]
GO
ALTER TABLE [dbo].[InventoryLog]  WITH CHECK ADD  CONSTRAINT [CK_InventoryLog_QuantityChange_NotZero] CHECK  (([QuantityChange]<>(0)))
GO
ALTER TABLE [dbo].[InventoryLog] CHECK CONSTRAINT [CK_InventoryLog_QuantityChange_NotZero]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [CK_OrderItems_Quantity_Positive] CHECK  (([Quantity]>(0)))
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [CK_OrderItems_Quantity_Positive]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [CK_OrderItems_UnitPrice_Positive] CHECK  (([UnitPrice]>(0)))
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [CK_OrderItems_UnitPrice_Positive]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [CK_Orders_DeliveredAt_AfterCreated] CHECK  (([DeliveredAt] IS NULL OR [DeliveredAt]>=[CreatedAt]))
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [CK_Orders_DeliveredAt_AfterCreated]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [CK_Orders_Status] CHECK  (([Status]=N'Cancelled' OR [Status]=N'Delivered' OR [Status]=N'Shipped' OR [Status]=N'Confirmed' OR [Status]=N'Pending'))
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [CK_Orders_Status]
GO
ALTER TABLE [dbo].[Persons]  WITH CHECK ADD  CONSTRAINT [CK_Persons_PasswordHash_Length] CHECK  (([PasswordHash] IS NULL OR len([PasswordHash])=(60)))
GO
ALTER TABLE [dbo].[Persons] CHECK CONSTRAINT [CK_Persons_PasswordHash_Length]
GO
ALTER TABLE [dbo].[Persons]  WITH CHECK ADD  CONSTRAINT [CK_Persons_PersonName_NotBlank] CHECK  ((len(ltrim(rtrim([PersonName])))>=(3)))
GO
ALTER TABLE [dbo].[Persons] CHECK CONSTRAINT [CK_Persons_PersonName_NotBlank]
GO
ALTER TABLE [dbo].[Persons]  WITH CHECK ADD  CONSTRAINT [CK_Persons_PhoneNumber_Format] CHECK  (([PhoneNumber] like '09[0-9][0-9][0-9][0-9][0-9][0-9][0-9][0-9]'))
GO
ALTER TABLE [dbo].[Persons] CHECK CONSTRAINT [CK_Persons_PhoneNumber_Format]
GO
ALTER TABLE [dbo].[Persons]  WITH CHECK ADD  CONSTRAINT [CK_Persons_Role] CHECK  (([Role]=N'Admin' OR [Role]=N'Customer'))
GO
ALTER TABLE [dbo].[Persons] CHECK CONSTRAINT [CK_Persons_Role]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_ProductName_NotBlank] CHECK  ((len(ltrim(rtrim([ProductName])))>=(1)))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_ProductName_NotBlank]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_RemainingQuantity_NonNegative] CHECK  (([RemainingQuantity]>=(0)))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_RemainingQuantity_NonNegative]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [CK_Products_UnitPrice_Positive] CHECK  (([UnitPrice]>(0)))
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [CK_Products_UnitPrice_Positive]
GO
ALTER TABLE [dbo].[Sections]  WITH CHECK ADD  CONSTRAINT [CK_Sections_MaxEmp_NonNegative] CHECK  (([MaxEmp]>=(0)))
GO
ALTER TABLE [dbo].[Sections] CHECK CONSTRAINT [CK_Sections_MaxEmp_NonNegative]
GO
/****** Object:  StoredProcedure [dbo].[_SP_AddNewProduct]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* ================================================================
   9. إجراءات المنتجات
   ================================================================ */

CREATE   PROCEDURE [dbo].[SP_RevokeAllRefreshTokensForPerson]
    @Person_ID INT,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.RefreshTokens
    SET RevokedAt = SYSUTCDATETIME()
    WHERE Person_ID = @Person_ID
      AND RevokedAt IS NULL;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO

CREATE   PROCEDURE [dbo].[SP_CancelOrder]
    @Order_ID int,
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @RowsAffected = 0;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @CurrentStatus nvarchar(20);
        SELECT @CurrentStatus = Status FROM dbo.Orders WITH (UPDLOCK, HOLDLOCK) WHERE Order_ID = @Order_ID;
        IF @CurrentStatus IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END;
        IF @CurrentStatus IN (N'Shipped', N'Delivered') THROW 52004, 'Orders cannot be cancelled after shipping or delivery.', 1;
        IF @CurrentStatus = N'Cancelled'
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END;
        IF @CurrentStatus NOT IN (N'Pending', N'Confirmed') THROW 52005, 'Order status does not allow cancellation.', 1;
        DECLARE @Items TABLE (Product_ID int NOT NULL PRIMARY KEY, Quantity int NOT NULL);
        INSERT INTO @Items SELECT Product_ID, SUM(Quantity) FROM dbo.OrderItems WHERE Order_ID = @Order_ID GROUP BY Product_ID;
        DECLARE @ReturnOperation_ID int = (SELECT Operation_ID FROM dbo.OperationTypes WHERE OperationName = N'Return');
        IF @ReturnOperation_ID IS NULL THROW 52006, 'Return operation type is not configured.', 1;
        UPDATE p WITH (UPDLOCK, HOLDLOCK)
        SET RemainingQuantity = RemainingQuantity + i.Quantity
        FROM dbo.Products p INNER JOIN @Items i ON i.Product_ID = p.Product_ID;
        INSERT INTO dbo.InventoryLog (Operation_ID, Order_ID, Product_ID, QuantityChange, OldQuantity, NewQuantity, OldStatus, NewStatus, CreatedAt, Reason)
        SELECT @ReturnOperation_ID, @Order_ID, i.Product_ID, i.Quantity, p.RemainingQuantity - i.Quantity, p.RemainingQuantity, N'Sold', N'Returned', SYSUTCDATETIME(), N'Order cancellation'
        FROM @Items i INNER JOIN dbo.Products p ON p.Product_ID = i.Product_ID;
        UPDATE dbo.Orders SET Status = N'Cancelled', DeliveredAt = NULL WHERE Order_ID = @Order_ID;
        SET @RowsAffected = @@ROWCOUNT;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO


CREATE   PROCEDURE [dbo].[_SP_AddNewProduct]
    @Des nvarchar(1000),
    @ProductName nvarchar(150),
    @UnitPrice decimal(18,2),
    @Category_ID int,
    @New_ID int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @New_ID = NULL;

    INSERT INTO dbo.Products
    (
        ProductName, Description, UnitPrice, Category_ID,
        RemainingQuantity, IsActive
    )
    VALUES
    (
        @ProductName, @Des, @UnitPrice, @Category_ID,
        0, 1
    );

    SET @New_ID = CONVERT(int, SCOPE_IDENTITY());
END;
GO
/****** Object:  StoredProcedure [dbo].[AddListOfOrderItemToOrder]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[AddListOfOrderItemToOrder]
    @T dbo.OrderTableType READONLY,
    @Person_ID int,
    @CreatedAt datetime2(0),
    @IdempotencyKey nvarchar(100) = NULL,
    @RequestHash char(64) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    IF NOT EXISTS (SELECT 1 FROM @T) THROW 50001, 'Order must contain at least one item.', 1;
    IF EXISTS (SELECT 1 FROM @T WHERE Product_ID <= 0 OR Quantity <= 0) THROW 50002, 'Product IDs and quantities must be positive.', 1;
    IF NOT EXISTS (SELECT 1 FROM dbo.Persons WHERE Person_ID = @Person_ID AND IsActive = 1) THROW 50003, 'Person does not exist or is inactive.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        IF @IdempotencyKey IS NOT NULL
        BEGIN
            DECLARE @ExistingOrder_ID int;
            DECLARE @ExistingRequestHash char(64);
            SELECT
                @ExistingOrder_ID = Order_ID,
                @ExistingRequestHash = RequestHash
            FROM dbo.OrderIdempotencyKeys WITH (UPDLOCK, HOLDLOCK)
            WHERE Person_ID = @Person_ID AND IdempotencyKey = @IdempotencyKey;

            IF @ExistingOrder_ID IS NOT NULL
            BEGIN
                IF @ExistingRequestHash IS NOT NULL AND @RequestHash IS NOT NULL AND @ExistingRequestHash <> @RequestHash
                    THROW 52012, 'The Idempotency-Key was already used with a different request.', 1;

                COMMIT TRANSACTION;
                SELECT @ExistingOrder_ID AS NewOrder_ID;
                RETURN;
            END;
        END;
        DECLARE @Requested TABLE (Product_ID int NOT NULL PRIMARY KEY, Quantity int NOT NULL);
        INSERT INTO @Requested SELECT Product_ID, SUM(Quantity) FROM @T GROUP BY Product_ID;
        IF EXISTS (SELECT 1 FROM @Requested r LEFT JOIN dbo.Products p ON p.Product_ID = r.Product_ID WHERE p.Product_ID IS NULL OR p.IsActive = 0) THROW 50004, 'One or more products do not exist or are inactive.', 1;
        UPDATE p WITH (UPDLOCK, HOLDLOCK)
        SET RemainingQuantity = RemainingQuantity - r.Quantity
        FROM dbo.Products p INNER JOIN @Requested r ON r.Product_ID = p.Product_ID
        WHERE p.IsActive = 1 AND p.RemainingQuantity >= r.Quantity;
        IF @@ROWCOUNT <> (SELECT COUNT(*) FROM @Requested) THROW 50005, 'Insufficient stock for one or more products.', 1;
        DECLARE @SaleOperation_ID int = (SELECT Operation_ID FROM dbo.OperationTypes WHERE OperationName = N'Sale');
        IF @SaleOperation_ID IS NULL THROW 50006, 'Sale operation type is not configured.', 1;
        DECLARE @NewOrder_ID int;
        INSERT INTO dbo.Orders (Person_ID, CreatedAt, Status) VALUES (@Person_ID, COALESCE(@CreatedAt, SYSUTCDATETIME()), N'Pending');
        SET @NewOrder_ID = CONVERT(int, SCOPE_IDENTITY());
        INSERT INTO dbo.OrderItems (Order_ID, Product_ID, Quantity, UnitPrice)
        SELECT @NewOrder_ID, r.Product_ID, r.Quantity, p.UnitPrice FROM @Requested r INNER JOIN dbo.Products p ON p.Product_ID = r.Product_ID;
        INSERT INTO dbo.InventoryLog (Operation_ID, Order_ID, Product_ID, QuantityChange, OldQuantity, NewQuantity, OldStatus, NewStatus, CreatedAt, Reason)
        SELECT @SaleOperation_ID, @NewOrder_ID, r.Product_ID, -r.Quantity, p.RemainingQuantity + r.Quantity, p.RemainingQuantity, N'Available', N'Sold', SYSUTCDATETIME(), N'Order creation'
        FROM @Requested r INNER JOIN dbo.Products p ON p.Product_ID = r.Product_ID;
        IF @IdempotencyKey IS NOT NULL
            INSERT INTO dbo.OrderIdempotencyKeys (Person_ID, IdempotencyKey, RequestHash, Order_ID)
            VALUES (@Person_ID, @IdempotencyKey, @RequestHash, @NewOrder_ID);
        COMMIT TRANSACTION;
        SELECT @NewOrder_ID AS NewOrder_ID;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[BannPerson]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[BannPerson]
    @Person_ID int,
    @Reason nvarchar(200),
    @BannedAt datetime2(0),
    @RAff int OUTPUT,
    @ActorPerson_ID int = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @RAff = 0;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @TargetRole nvarchar(20), @TargetActive bit;
        SELECT @TargetRole = Role, @TargetActive = IsActive FROM dbo.Persons WITH (UPDLOCK, HOLDLOCK) WHERE Person_ID = @Person_ID;
        IF @TargetRole IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END;
        IF @ActorPerson_ID IS NOT NULL AND @ActorPerson_ID = @Person_ID THROW 52010, 'An administrator cannot ban their own account.', 1;
        IF @TargetRole = N'Admin' AND @TargetActive = 1 AND (SELECT COUNT(*) FROM dbo.Persons WITH (UPDLOCK, HOLDLOCK) WHERE Role = N'Admin' AND IsActive = 1) <= 1 THROW 52011, 'The last active administrator cannot be banned.', 1;
        UPDATE dbo.Persons SET IsActive = 0 WHERE Person_ID = @Person_ID;
        INSERT INTO dbo.BannedPersons (Person_ID, Reason, BannedAt) VALUES (@Person_ID, @Reason, COALESCE(@BannedAt, SYSUTCDATETIME()));
        SET @RAff = @@ROWCOUNT;
        DECLARE @TokensRevoked int;
        EXEC dbo.SP_RevokeAllRefreshTokensForPerson @Person_ID = @Person_ID, @RowsAffected = @TokensRevoked OUTPUT;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[DeleteOrder_ItsItems]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[DeleteOrder_ItsItems]
    @Order_ID int,
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    EXEC dbo.SP_CancelOrder @Order_ID = @Order_ID, @RowsAffected = @RowsAffected OUTPUT;
END;
GO
/****** Object:  StoredProcedure [dbo].[DeleteProduct]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 8. Disable a product instead of physically deleting its history. */
CREATE   PROCEDURE [dbo].[DeleteProduct]
    @Product_ID int,
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Products
    SET IsActive = 0
    WHERE Product_ID = @Product_ID AND IsActive = 1;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO
/****** Object:  StoredProcedure [dbo].[GetOrderOwnerId]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrderOwnerId]
    @OrderId INT
AS
BEGIN
    SELECT Person_ID FROM Orders WHERE Order_ID = @OrderId;
END
GO
/****** Object:  StoredProcedure [dbo].[RefreshTokens_Revoke]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE   PROCEDURE [dbo].[RefreshTokens_Revoke]
    @TokenHash            CHAR(64),
    @ReplacedByTokenHash  CHAR(64) = NULL,
    @RowsAffected         INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.RefreshTokens
    SET RevokedAt           = SYSUTCDATETIME(),
        ReplacedByTokenHash = @ReplacedByTokenHash
    WHERE TokenHash = @TokenHash
      AND RevokedAt IS NULL;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_AddNewPerson]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_AddNewPerson]
    @PersonName nvarchar(150),
    @PhoneNumber nvarchar(20),
    @Address nvarchar(250),
    @PasswordHash char(60),
    @Role nvarchar(20) = N'Customer',
    @NewID int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @NewID = NULL;

    INSERT INTO dbo.Persons (PersonName, PhoneNumber, Address, PasswordHash, Role)
    VALUES (@PersonName, @PhoneNumber, @Address, @PasswordHash, @Role);

    SET @NewID = CONVERT(int, SCOPE_IDENTITY());
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_AddRefreshToken]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_AddRefreshToken]
    @TokenHash CHAR(64),
    @Person_ID INT,
    @ExpiresAt DATETIME2,
    @NewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    INSERT INTO dbo.RefreshTokens
    (
        TokenHash,
        Person_ID,
        ExpiresAt,
        CreatedAt
    )
    VALUES
    (
        @TokenHash,
        @Person_ID,
        @ExpiresAt,
        SYSUTCDATETIME()
    );

    SET @NewID = CONVERT(INT, SCOPE_IDENTITY());
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_CancelOrder]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Object:  StoredProcedure [dbo].[SP_DeletePerson]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/* 5. Soft-delete a person and revoke all sessions. */
CREATE   PROCEDURE [dbo].[SP_DeletePerson]
    @Person_ID int,
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @RowsAffected = 0;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Persons
        SET IsActive = 0
        WHERE Person_ID = @Person_ID AND IsActive = 1;

        SET @RowsAffected = @@ROWCOUNT;

        IF @RowsAffected > 0
        BEGIN
            DECLARE @TokensRevoked int;
            EXEC dbo.SP_RevokeAllRefreshTokensForPerson
                @Person_ID = @Person_ID,
                @RowsAffected = @TokensRevoked OUTPUT;
        END;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_GetRemainingQuantity]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_GetRemainingQuantity]
    @productId int
AS
BEGIN
    SET NOCOUNT ON;

    SELECT RemainingQuantity
    FROM dbo.Products
    WHERE Product_ID = @productId;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_MakeDisCountFor]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_MakeDisCountFor]
    @DC int,
    @Person_ID int
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF EXISTS (SELECT 1 FROM dbo.FavouriteCustomers WHERE Person_ID = @Person_ID)
    BEGIN
        UPDATE dbo.FavouriteCustomers SET DisCount = @DC WHERE Person_ID = @Person_ID;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.FavouriteCustomers (Person_ID, DisCount)
        VALUES (@Person_ID, @DC);
    END
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_RestockProduct]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_RestockProduct]
    @Product_ID int,
    @Quantity int,
    @Reason nvarchar(200),
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @RowsAffected = 0;
    IF @Quantity <= 0 THROW 52001, 'Restock quantity must be positive.', 1;
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @OldQuantity int;
        SELECT @OldQuantity = RemainingQuantity FROM dbo.Products WITH (UPDLOCK, HOLDLOCK) WHERE Product_ID = @Product_ID AND IsActive = 1;
        IF @OldQuantity IS NULL THROW 52002, 'Product does not exist or is inactive.', 1;
        DECLARE @Operation_ID int = (SELECT Operation_ID FROM dbo.OperationTypes WHERE OperationName = N'Restock');
        IF @Operation_ID IS NULL THROW 52003, 'Restock operation type is not configured.', 1;
        UPDATE dbo.Products SET RemainingQuantity = RemainingQuantity + @Quantity WHERE Product_ID = @Product_ID;
        INSERT INTO dbo.InventoryLog (Operation_ID, Product_ID, QuantityChange, OldQuantity, NewQuantity, OldStatus, NewStatus, CreatedAt, Reason)
        VALUES (@Operation_ID, @Product_ID, @Quantity, @OldQuantity, @OldQuantity + @Quantity, N'Available', N'Restocked', SYSUTCDATETIME(), @Reason);
        SET @RowsAffected = 1;
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_RevokeAllRefreshTokensForPerson]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/****** Object:  StoredProcedure [dbo].[SP_RevokeRefreshToken]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_RevokeRefreshToken]
    @TokenHash CHAR(64),
    @ReplacedByTokenHash CHAR(64) = NULL,
    @RowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.RefreshTokens
    SET
        RevokedAt = SYSUTCDATETIME(),
        ReplacedByTokenHash = @ReplacedByTokenHash
    WHERE TokenHash = @TokenHash
      AND RevokedAt IS NULL;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_RotateRefreshToken]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_RotateRefreshToken]
    @OldTokenHash CHAR(64),
    @NewTokenHash CHAR(64),
    @NewExpiresAt DATETIME2,
    @NewToken_ID INT OUTPUT,
    @Person_ID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @NewToken_ID = 0;
    SET @Person_ID = NULL;

    DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();

    BEGIN TRY
        BEGIN TRANSACTION;

        SELECT @Person_ID = Person_ID
        FROM dbo.RefreshTokens WITH (UPDLOCK, HOLDLOCK)
        WHERE TokenHash = @OldTokenHash
          AND RevokedAt IS NULL
          AND ExpiresAt > @Now;

        IF @Person_ID IS NULL
        BEGIN
            ROLLBACK TRANSACTION;
            RETURN;
        END;

        UPDATE dbo.RefreshTokens
        SET
            RevokedAt = @Now,
            ReplacedByTokenHash = @NewTokenHash
        WHERE TokenHash = @OldTokenHash
          AND RevokedAt IS NULL
          AND ExpiresAt > @Now;

        IF @@ROWCOUNT <> 1
        BEGIN
            ROLLBACK TRANSACTION;
            SET @Person_ID = NULL;
            RETURN;
        END;

        INSERT INTO dbo.RefreshTokens
        (
            TokenHash,
            Person_ID,
            ExpiresAt,
            CreatedAt
        )
        VALUES
        (
            @NewTokenHash,
            @Person_ID,
            @NewExpiresAt,
            @Now
        );

        SET @NewToken_ID = CONVERT(INT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_UpdateOrders]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_UpdateOrders]
    @Order_Id int,
    @D_At datetime2(0),
    @Status nvarchar(20),
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @RowsAffected = 0;
    DECLARE @CurrentStatus nvarchar(20), @CreatedAt datetime2(0);
    SELECT @CurrentStatus = Status, @CreatedAt = CreatedAt FROM dbo.Orders WHERE Order_ID = @Order_Id;
    IF @CurrentStatus IS NULL RETURN;
    IF @Status = N'Cancelled' THROW 52007, 'Use the cancellation operation to cancel an order.', 1;
    IF NOT ((@CurrentStatus = N'Pending' AND @Status = N'Confirmed') OR (@CurrentStatus = N'Confirmed' AND @Status = N'Shipped') OR (@CurrentStatus = N'Shipped' AND @Status = N'Delivered'))
        THROW 52008, 'Invalid order status transition.', 1;
    IF @Status = N'Delivered' SET @D_At = COALESCE(@D_At, CONVERT(datetime2(0), SYSUTCDATETIME()));
    ELSE SET @D_At = NULL;
    IF @D_At IS NOT NULL AND @D_At < @CreatedAt THROW 52009, 'DeliveredAt cannot be earlier than CreatedAt.', 1;
    UPDATE dbo.Orders SET Status = @Status, DeliveredAt = @D_At WHERE Order_ID = @Order_Id;
    SET @RowsAffected = @@ROWCOUNT;
END;
GO
/****** Object:  StoredProcedure [dbo].[SP_UpdatePerson]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[SP_UpdatePerson]
    @Person_ID int,
    @PhoneNumber nvarchar(20),
    @Address nvarchar(250),
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Persons
    SET PhoneNumber = @PhoneNumber,
        Address = @Address
    WHERE Person_ID = @Person_ID;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO
/****** Object:  StoredProcedure [dbo].[UpdateProduct]    Script Date: 09/10/2026 10:53:21 م ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE   PROCEDURE [dbo].[UpdateProduct]
    @Description nvarchar(1000),
    @UnitPrice decimal(18,2),
    @Product_ID int,
    @RowsAffected int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Products
    SET Description = @Description,
        UnitPrice = @UnitPrice
    WHERE Product_ID = @Product_ID;

    SET @RowsAffected = @@ROWCOUNT;
END;
GO
USE [master]
GO
ALTER DATABASE [ShopDB] SET  READ_WRITE 
GO
