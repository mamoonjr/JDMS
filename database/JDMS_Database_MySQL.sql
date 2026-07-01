CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `AspNetRoles` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoles` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUsers` (
    `Id` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `FullName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedUserName` varchar(256) CHARACTER SET utf8mb4 NULL,
    `Email` varchar(256) CHARACTER SET utf8mb4 NULL,
    `NormalizedEmail` varchar(256) CHARACTER SET utf8mb4 NULL,
    `EmailConfirmed` tinyint(1) NOT NULL,
    `PasswordHash` longtext CHARACTER SET utf8mb4 NULL,
    `SecurityStamp` longtext CHARACTER SET utf8mb4 NULL,
    `ConcurrencyStamp` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NULL,
    `PhoneNumberConfirmed` tinyint(1) NOT NULL,
    `TwoFactorEnabled` tinyint(1) NOT NULL,
    `LockoutEnd` datetime(6) NULL,
    `LockoutEnabled` tinyint(1) NOT NULL,
    `AccessFailedCount` int NOT NULL,
    CONSTRAINT `PK_AspNetUsers` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `AuditLogs` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NULL,
    `UserName` longtext CHARACTER SET utf8mb4 NULL,
    `ActionType` int NOT NULL,
    `EntityName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `EntityId` longtext CHARACTER SET utf8mb4 NULL,
    `OldValues` longtext CHARACTER SET utf8mb4 NULL,
    `NewValues` longtext CHARACTER SET utf8mb4 NULL,
    `IpAddress` longtext CHARACTER SET utf8mb4 NULL,
    `ActionDate` datetime(6) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_AuditLogs` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `CompanySettings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CompanyName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `LogoPath` longtext CHARACTER SET utf8mb4 NULL,
    `Address` longtext CHARACTER SET utf8mb4 NULL,
    `Phone` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `TaxRate` decimal(5,4) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_CompanySettings` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Customers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CustomerCode` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `FullName` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
    `MobileNumber` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
    `SecondaryMobile` longtext CHARACTER SET utf8mb4 NULL,
    `Email` longtext CHARACTER SET utf8mb4 NULL,
    `Notes` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Customers` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Governorates` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `NameAr` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `NameEn` longtext CHARACTER SET utf8mb4 NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Governorates` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Products` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `ProductName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `SKU` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Description` longtext CHARACTER SET utf8mb4 NULL,
    `UnitPrice` decimal(18,2) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Products` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetRoleClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetRoleClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetRoleClaims_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserClaims` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ClaimType` longtext CHARACTER SET utf8mb4 NULL,
    `ClaimValue` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserClaims` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_AspNetUserClaims_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserLogins` (
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderKey` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `ProviderDisplayName` longtext CHARACTER SET utf8mb4 NULL,
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AspNetUserLogins` PRIMARY KEY (`LoginProvider`, `ProviderKey`),
    CONSTRAINT `FK_AspNetUserLogins_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserRoles` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `RoleId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK_AspNetUserRoles` PRIMARY KEY (`UserId`, `RoleId`),
    CONSTRAINT `FK_AspNetUserRoles_AspNetRoles_RoleId` FOREIGN KEY (`RoleId`) REFERENCES `AspNetRoles` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_AspNetUserRoles_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `AspNetUserTokens` (
    `UserId` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `LoginProvider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `Value` longtext CHARACTER SET utf8mb4 NULL,
    CONSTRAINT `PK_AspNetUserTokens` PRIMARY KEY (`UserId`, `LoginProvider`, `Name`),
    CONSTRAINT `FK_AspNetUserTokens_AspNetUsers_UserId` FOREIGN KEY (`UserId`) REFERENCES `AspNetUsers` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Areas` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `GovernorateId` int NOT NULL,
    `NameAr` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `NameEn` longtext CHARACTER SET utf8mb4 NOT NULL,
    `DeliveryFee` decimal(18,2) NOT NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Areas` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Areas_Governorates_GovernorateId` FOREIGN KEY (`GovernorateId`) REFERENCES `Governorates` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Addresses` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `CustomerId` int NOT NULL,
    `GovernorateId` int NOT NULL,
    `AreaId` int NOT NULL,
    `Street` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Building` longtext CHARACTER SET utf8mb4 NULL,
    `Apartment` longtext CHARACTER SET utf8mb4 NULL,
    `Latitude` double NULL,
    `Longitude` double NULL,
    `GoogleMapsLink` longtext CHARACTER SET utf8mb4 NULL,
    `IsDefault` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Addresses` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Addresses_Areas_AreaId` FOREIGN KEY (`AreaId`) REFERENCES `Areas` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Addresses_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_Addresses_Governorates_GovernorateId` FOREIGN KEY (`GovernorateId`) REFERENCES `Governorates` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `Drivers` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `DriverName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PhoneNumber` longtext CHARACTER SET utf8mb4 NOT NULL,
    `VehicleType` int NOT NULL,
    `AssignedAreaId` int NULL,
    `IsActive` tinyint(1) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Drivers` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Drivers_Areas_AssignedAreaId` FOREIGN KEY (`AssignedAreaId`) REFERENCES `Areas` (`Id`) ON DELETE SET NULL
) CHARACTER SET=utf8mb4;

CREATE TABLE `Orders` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrderNumber` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `CustomerId` int NOT NULL,
    `AddressId` int NOT NULL,
    `OrderDate` datetime(6) NOT NULL,
    `DeliveryDate` datetime(6) NULL,
    `Status` int NOT NULL,
    `Notes` longtext CHARACTER SET utf8mb4 NULL,
    `Subtotal` decimal(18,2) NOT NULL,
    `DeliveryFee` decimal(18,2) NOT NULL,
    `Discount` decimal(18,2) NOT NULL,
    `Tax` decimal(18,2) NOT NULL,
    `GrandTotal` decimal(18,2) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Orders` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Orders_Addresses_AddressId` FOREIGN KEY (`AddressId`) REFERENCES `Addresses` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_Orders_Customers_CustomerId` FOREIGN KEY (`CustomerId`) REFERENCES `Customers` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE TABLE `DeliveryTrackings` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrderId` int NOT NULL,
    `DriverId` int NOT NULL,
    `AssignDate` datetime(6) NOT NULL,
    `DeliveryDate` datetime(6) NULL,
    `Status` int NOT NULL,
    `DeliveryNotes` longtext CHARACTER SET utf8mb4 NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_DeliveryTrackings` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_DeliveryTrackings_Drivers_DriverId` FOREIGN KEY (`DriverId`) REFERENCES `Drivers` (`Id`) ON DELETE RESTRICT,
    CONSTRAINT `FK_DeliveryTrackings_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `Invoices` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `InvoiceNumber` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
    `OrderId` int NOT NULL,
    `InvoiceDate` datetime(6) NOT NULL,
    `Subtotal` decimal(18,2) NOT NULL,
    `DeliveryFee` decimal(18,2) NOT NULL,
    `Discount` decimal(18,2) NOT NULL,
    `Tax` decimal(18,2) NOT NULL,
    `GrandTotal` decimal(18,2) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_Invoices` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_Invoices_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

CREATE TABLE `OrderDetails` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `OrderId` int NOT NULL,
    `ProductId` int NOT NULL,
    `Quantity` int NOT NULL,
    `UnitPrice` decimal(18,2) NOT NULL,
    `Discount` decimal(18,2) NOT NULL,
    `LineTotal` decimal(18,2) NOT NULL,
    `CreatedAt` datetime(6) NOT NULL,
    `UpdatedAt` datetime(6) NULL,
    CONSTRAINT `PK_OrderDetails` PRIMARY KEY (`Id`),
    CONSTRAINT `FK_OrderDetails_Orders_OrderId` FOREIGN KEY (`OrderId`) REFERENCES `Orders` (`Id`) ON DELETE CASCADE,
    CONSTRAINT `FK_OrderDetails_Products_ProductId` FOREIGN KEY (`ProductId`) REFERENCES `Products` (`Id`) ON DELETE RESTRICT
) CHARACTER SET=utf8mb4;

CREATE INDEX `IX_Addresses_AreaId` ON `Addresses` (`AreaId`);

CREATE INDEX `IX_Addresses_CustomerId` ON `Addresses` (`CustomerId`);

CREATE INDEX `IX_Addresses_GovernorateId` ON `Addresses` (`GovernorateId`);

CREATE INDEX `IX_Areas_GovernorateId_NameAr` ON `Areas` (`GovernorateId`, `NameAr`);

CREATE INDEX `IX_AspNetRoleClaims_RoleId` ON `AspNetRoleClaims` (`RoleId`);

CREATE UNIQUE INDEX `RoleNameIndex` ON `AspNetRoles` (`NormalizedName`);

CREATE INDEX `IX_AspNetUserClaims_UserId` ON `AspNetUserClaims` (`UserId`);

CREATE INDEX `IX_AspNetUserLogins_UserId` ON `AspNetUserLogins` (`UserId`);

CREATE INDEX `IX_AspNetUserRoles_RoleId` ON `AspNetUserRoles` (`RoleId`);

CREATE INDEX `EmailIndex` ON `AspNetUsers` (`NormalizedEmail`);

CREATE UNIQUE INDEX `UserNameIndex` ON `AspNetUsers` (`NormalizedUserName`);

CREATE INDEX `IX_AuditLogs_ActionDate` ON `AuditLogs` (`ActionDate`);

CREATE INDEX `IX_AuditLogs_UserId` ON `AuditLogs` (`UserId`);

CREATE UNIQUE INDEX `IX_Customers_CustomerCode` ON `Customers` (`CustomerCode`);

CREATE INDEX `IX_Customers_MobileNumber` ON `Customers` (`MobileNumber`);

CREATE INDEX `IX_DeliveryTrackings_DriverId` ON `DeliveryTrackings` (`DriverId`);

CREATE UNIQUE INDEX `IX_DeliveryTrackings_OrderId` ON `DeliveryTrackings` (`OrderId`);

CREATE INDEX `IX_Drivers_AssignedAreaId` ON `Drivers` (`AssignedAreaId`);

CREATE UNIQUE INDEX `IX_Governorates_NameAr` ON `Governorates` (`NameAr`);

CREATE UNIQUE INDEX `IX_Invoices_InvoiceNumber` ON `Invoices` (`InvoiceNumber`);

CREATE UNIQUE INDEX `IX_Invoices_OrderId` ON `Invoices` (`OrderId`);

CREATE INDEX `IX_OrderDetails_OrderId` ON `OrderDetails` (`OrderId`);

CREATE INDEX `IX_OrderDetails_ProductId` ON `OrderDetails` (`ProductId`);

CREATE INDEX `IX_Orders_AddressId` ON `Orders` (`AddressId`);

CREATE INDEX `IX_Orders_CustomerId` ON `Orders` (`CustomerId`);

CREATE INDEX `IX_Orders_OrderDate` ON `Orders` (`OrderDate`);

CREATE UNIQUE INDEX `IX_Orders_OrderNumber` ON `Orders` (`OrderNumber`);

CREATE INDEX `IX_Orders_Status` ON `Orders` (`Status`);

CREATE UNIQUE INDEX `IX_Products_SKU` ON `Products` (`SKU`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260604095310_InitialCreate', '8.0.11');

COMMIT;

