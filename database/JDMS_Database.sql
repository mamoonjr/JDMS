IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AuditLogs] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NULL,
    [UserName] nvarchar(max) NULL,
    [ActionType] int NOT NULL,
    [EntityName] nvarchar(max) NOT NULL,
    [EntityId] nvarchar(max) NULL,
    [OldValues] nvarchar(max) NULL,
    [NewValues] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [ActionDate] datetime2 NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [CompanySettings] (
    [Id] int NOT NULL IDENTITY,
    [CompanyName] nvarchar(max) NOT NULL,
    [LogoPath] nvarchar(max) NULL,
    [Address] nvarchar(max) NULL,
    [Phone] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [TaxRate] decimal(5,4) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_CompanySettings] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [CustomerCode] nvarchar(20) NOT NULL,
    [FullName] nvarchar(200) NOT NULL,
    [MobileNumber] nvarchar(20) NOT NULL,
    [SecondaryMobile] nvarchar(max) NULL,
    [Email] nvarchar(max) NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Governorates] (
    [Id] int NOT NULL IDENTITY,
    [NameAr] nvarchar(100) NOT NULL,
    [NameEn] nvarchar(max) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Governorates] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [ProductName] nvarchar(max) NOT NULL,
    [SKU] nvarchar(450) NOT NULL,
    [Description] nvarchar(max) NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Areas] (
    [Id] int NOT NULL IDENTITY,
    [GovernorateId] int NOT NULL,
    [NameAr] nvarchar(450) NOT NULL,
    [NameEn] nvarchar(max) NOT NULL,
    [DeliveryFee] decimal(18,2) NOT NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Areas] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Areas_Governorates_GovernorateId] FOREIGN KEY ([GovernorateId]) REFERENCES [Governorates] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Addresses] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [GovernorateId] int NOT NULL,
    [AreaId] int NOT NULL,
    [Street] nvarchar(max) NOT NULL,
    [Building] nvarchar(max) NULL,
    [Apartment] nvarchar(max) NULL,
    [Latitude] float NULL,
    [Longitude] float NULL,
    [GoogleMapsLink] nvarchar(max) NULL,
    [IsDefault] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Addresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Addresses_Areas_AreaId] FOREIGN KEY ([AreaId]) REFERENCES [Areas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Addresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Addresses_Governorates_GovernorateId] FOREIGN KEY ([GovernorateId]) REFERENCES [Governorates] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Drivers] (
    [Id] int NOT NULL IDENTITY,
    [DriverName] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [VehicleType] int NOT NULL,
    [AssignedAreaId] int NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Drivers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Drivers_Areas_AssignedAreaId] FOREIGN KEY ([AssignedAreaId]) REFERENCES [Areas] ([Id]) ON DELETE SET NULL
);
GO

CREATE TABLE [Orders] (
    [Id] int NOT NULL IDENTITY,
    [OrderNumber] nvarchar(450) NOT NULL,
    [CustomerId] int NOT NULL,
    [AddressId] int NOT NULL,
    [OrderDate] datetime2 NOT NULL,
    [DeliveryDate] datetime2 NULL,
    [Status] int NOT NULL,
    [Notes] nvarchar(max) NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DeliveryFee] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [Tax] decimal(18,2) NOT NULL,
    [GrandTotal] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Addresses_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Addresses] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Orders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);
GO

CREATE TABLE [DeliveryTrackings] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [DriverId] int NOT NULL,
    [AssignDate] datetime2 NOT NULL,
    [DeliveryDate] datetime2 NULL,
    [Status] int NOT NULL,
    [DeliveryNotes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_DeliveryTrackings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeliveryTrackings_Drivers_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [Drivers] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DeliveryTrackings_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Invoices] (
    [Id] int NOT NULL IDENTITY,
    [InvoiceNumber] nvarchar(450) NOT NULL,
    [OrderId] int NOT NULL,
    [InvoiceDate] datetime2 NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    [DeliveryFee] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [Tax] decimal(18,2) NOT NULL,
    [GrandTotal] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_Invoices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Invoices_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [OrderDetails] (
    [Id] int NOT NULL IDENTITY,
    [OrderId] int NOT NULL,
    [ProductId] int NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Discount] decimal(18,2) NOT NULL,
    [LineTotal] decimal(18,2) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    CONSTRAINT [PK_OrderDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_OrderDetails_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [Orders] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_OrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION
);
GO

CREATE INDEX [IX_Addresses_AreaId] ON [Addresses] ([AreaId]);
GO

CREATE INDEX [IX_Addresses_CustomerId] ON [Addresses] ([CustomerId]);
GO

CREATE INDEX [IX_Addresses_GovernorateId] ON [Addresses] ([GovernorateId]);
GO

CREATE INDEX [IX_Areas_GovernorateId_NameAr] ON [Areas] ([GovernorateId], [NameAr]);
GO

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO

CREATE INDEX [IX_AuditLogs_ActionDate] ON [AuditLogs] ([ActionDate]);
GO

CREATE INDEX [IX_AuditLogs_UserId] ON [AuditLogs] ([UserId]);
GO

CREATE UNIQUE INDEX [IX_Customers_CustomerCode] ON [Customers] ([CustomerCode]);
GO

CREATE INDEX [IX_Customers_MobileNumber] ON [Customers] ([MobileNumber]);
GO

CREATE INDEX [IX_DeliveryTrackings_DriverId] ON [DeliveryTrackings] ([DriverId]);
GO

CREATE UNIQUE INDEX [IX_DeliveryTrackings_OrderId] ON [DeliveryTrackings] ([OrderId]);
GO

CREATE INDEX [IX_Drivers_AssignedAreaId] ON [Drivers] ([AssignedAreaId]);
GO

CREATE UNIQUE INDEX [IX_Governorates_NameAr] ON [Governorates] ([NameAr]);
GO

CREATE UNIQUE INDEX [IX_Invoices_InvoiceNumber] ON [Invoices] ([InvoiceNumber]);
GO

CREATE UNIQUE INDEX [IX_Invoices_OrderId] ON [Invoices] ([OrderId]);
GO

CREATE INDEX [IX_OrderDetails_OrderId] ON [OrderDetails] ([OrderId]);
GO

CREATE INDEX [IX_OrderDetails_ProductId] ON [OrderDetails] ([ProductId]);
GO

CREATE INDEX [IX_Orders_AddressId] ON [Orders] ([AddressId]);
GO

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);
GO

CREATE INDEX [IX_Orders_OrderDate] ON [Orders] ([OrderDate]);
GO

CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [Orders] ([OrderNumber]);
GO

CREATE INDEX [IX_Orders_Status] ON [Orders] ([Status]);
GO

CREATE UNIQUE INDEX [IX_Products_SKU] ON [Products] ([SKU]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260604094544_InitialCreate', N'8.0.11');
GO

COMMIT;
GO

