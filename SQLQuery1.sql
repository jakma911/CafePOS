-- =====================================================
--  Café POS Management System - SQL Server script
--  Run this script in SSMS first, before running the app
-- =====================================================
IF DB_ID('CafePOS') IS NULL CREATE DATABASE CafePOS;
GO
USE CafePOS;
GO

IF OBJECT_ID('AuditLog')   IS NOT NULL DROP TABLE AuditLog;
IF OBJECT_ID('Payments')   IS NOT NULL DROP TABLE Payments;
IF OBJECT_ID('OrderItems') IS NOT NULL DROP TABLE OrderItems;
IF OBJECT_ID('Orders')     IS NOT NULL DROP TABLE Orders;
IF OBJECT_ID('Products')   IS NOT NULL DROP TABLE Products;
IF OBJECT_ID('CafeTables') IS NOT NULL DROP TABLE CafeTables;
IF OBJECT_ID('Users')      IS NOT NULL DROP TABLE Users;
GO

CREATE TABLE Users (
    UserId       INT IDENTITY(1,1) PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash NVARCHAR(64)  NOT NULL,          -- SHA256 hex
    FullName     NVARCHAR(100) NOT NULL,
    Role         NVARCHAR(20)  NOT NULL CHECK (Role IN ('Admin','Manager','Cashier')),
    IsActive     BIT NOT NULL DEFAULT 1
);

CREATE TABLE Products (
    ProductId INT IDENTITY(1,1) PRIMARY KEY,
    Name      NVARCHAR(100) NOT NULL,
    Category  NVARCHAR(50)  NOT NULL,
    Price     DECIMAL(10,2) NOT NULL CHECK (Price >= 0),
    IsActive  BIT NOT NULL DEFAULT 1
);

CREATE TABLE CafeTables (
    TableId   INT IDENTITY(1,1) PRIMARY KEY,
    TableName NVARCHAR(30) NOT NULL UNIQUE,
    Status    NVARCHAR(20) NOT NULL DEFAULT 'Available'   -- Available / Occupied
);

CREATE TABLE Orders (
    OrderId   INT IDENTITY(1,1) PRIMARY KEY,
    OrderNo   NVARCHAR(30) NOT NULL UNIQUE,
    TableId   INT NOT NULL REFERENCES CafeTables(TableId),
    UserId    INT NOT NULL REFERENCES Users(UserId),
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status    NVARCHAR(20) NOT NULL DEFAULT 'Open',        -- Open / Paid / Cancelled
    Total     DECIMAL(12,2) NOT NULL DEFAULT 0
);

CREATE TABLE OrderItems (
    ItemId    INT IDENTITY(1,1) PRIMARY KEY,
    OrderId   INT NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
    ProductId INT NOT NULL REFERENCES Products(ProductId),
    Qty       INT NOT NULL CHECK (Qty > 0),
    UnitPrice DECIMAL(10,2) NOT NULL
);

CREATE TABLE Payments (
    PaymentId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId   INT NOT NULL REFERENCES Orders(OrderId),
    Method    NVARCHAR(30)  NOT NULL,
    Amount    DECIMAL(12,2) NOT NULL,
    Received  DECIMAL(12,2) NOT NULL,
    ChangeAmt DECIMAL(12,2) NOT NULL,
    PaidDate  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE AuditLog (
    LogId    INT IDENTITY(1,1) PRIMARY KEY,
    UserId   INT NULL,
    Username NVARCHAR(50) NULL,
    Action   NVARCHAR(50) NOT NULL,
    Details  NVARCHAR(400) NULL,
    LogDate  DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- Sample data (users are seeded automatically by the app on first run:
--   admin/admin123, manager/manager123, cashier/cashier123)
INSERT INTO CafeTables(TableName) VALUES ('T01'),('T02'),('T03'),('T04'),('T05'),('T06'),('T07'),('T08');

INSERT INTO Products(Name,Category,Price) VALUES
 ('Espresso','Coffee',1.50),
 ('Americano','Coffee',1.75),
 ('Cappuccino','Coffee',2.25),
 ('Latte','Coffee',2.50),
 ('Iced Coffee','Coffee',2.00),
 ('Green Tea','Tea',1.50),
 ('Milk Tea','Tea',2.00),
 ('Orange Juice','Juice',2.25),
 ('Mango Smoothie','Juice',2.75),
 ('Croissant','Bakery',1.80),
 ('Chocolate Cake','Bakery',3.00),
 ('Cheese Sandwich','Food',3.50);
GO
