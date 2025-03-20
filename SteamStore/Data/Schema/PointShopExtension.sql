-- Point Shop extension for mockDatabaseQuery.sql
-- Run this after the main mockDatabaseQuery.sql script

-- Drop existing Point Shop tables if they exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'UserInventoryItems')
    DROP TABLE UserInventoryItems;
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'PointShopItems')
    DROP TABLE PointShopItems;
GO

-- Create Point Shop Tables
CREATE TABLE PointShopItems (
    ItemId INT PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    ImagePath NVARCHAR(MAX),
    PointPrice DECIMAL(10, 2) NOT NULL,
    ItemType NVARCHAR(50) NOT NULL -- E.g., "ProfileBackground", "Avatar", "Emoticon", etc.
);

CREATE TABLE UserInventoryItems (
    UserId INT,
    ItemId INT,
    PurchaseDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 0,
    PRIMARY KEY (UserId, ItemId),
    FOREIGN KEY (ItemId) REFERENCES PointShopItems(ItemId)
);
GO

-- Drop existing Point Shop procedures if they exist
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetAllPointShopItems')
    DROP PROCEDURE GetAllPointShopItems;
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetUserPointShopItems')
    DROP PROCEDURE GetUserPointShopItems;
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'PurchasePointShopItem')
    DROP PROCEDURE PurchasePointShopItem;
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'ActivatePointShopItem')
    DROP PROCEDURE ActivatePointShopItem;
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'DeactivatePointShopItem')
    DROP PROCEDURE DeactivatePointShopItem;
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'UpdateUserPointBalance')
    DROP PROCEDURE UpdateUserPointBalance;
GO

-- Create Point Shop stored procedures
CREATE PROCEDURE GetAllPointShopItems
AS
BEGIN
    SELECT * FROM PointShopItems;
END
GO

CREATE PROCEDURE GetUserPointShopItems
    @UserId INT
AS
BEGIN
    SELECT i.ItemId, i.Name, i.Description, i.ImagePath, i.PointPrice, i.ItemType, ui.IsActive
    FROM PointShopItems i
    INNER JOIN UserInventoryItems ui ON i.ItemId = ui.ItemId
    WHERE ui.UserId = @UserId;
END
GO

CREATE PROCEDURE PurchasePointShopItem
    @UserId INT,
    @ItemId INT
AS
BEGIN
    -- Check if user already owns this item
    IF EXISTS (SELECT 1 FROM UserInventoryItems WHERE UserId = @UserId AND ItemId = @ItemId)
    BEGIN
        RAISERROR('User already owns this item', 16, 1);
        RETURN;
    END

    -- Insert the item into user's inventory
    INSERT INTO UserInventoryItems (UserId, ItemId, PurchaseDate, IsActive)
    VALUES (@UserId, @ItemId, GETDATE(), 0);
END
GO

CREATE PROCEDURE ActivatePointShopItem
    @UserId INT,
    @ItemId INT
AS
BEGIN
    DECLARE @ItemType NVARCHAR(50);
    
    -- Get the type of the item being activated
    SELECT @ItemType = ItemType 
    FROM PointShopItems 
    WHERE ItemId = @ItemId;
    
    -- Deactivate all other items of the same type for this user
    UPDATE UserInventoryItems
    SET IsActive = 0
    FROM UserInventoryItems ui
    INNER JOIN PointShopItems i ON ui.ItemId = i.ItemId
    WHERE ui.UserId = @UserId 
    AND i.ItemType = @ItemType;
    
    -- Activate the selected item
    UPDATE UserInventoryItems
    SET IsActive = 1
    WHERE UserId = @UserId AND ItemId = @ItemId;
END
GO

CREATE PROCEDURE DeactivatePointShopItem
    @UserId INT,
    @ItemId INT
AS
BEGIN
    UPDATE UserInventoryItems
    SET IsActive = 0
    WHERE UserId = @UserId AND ItemId = @ItemId;
END
GO

CREATE PROCEDURE UpdateUserPointBalance
    @UserId INT,
    @PointBalance DECIMAL(10, 2)
AS
BEGIN
    UPDATE users
    SET point_balance = @PointBalance
    WHERE user_id = @UserId;
END
GO

-- Insert sample data for Point Shop
INSERT INTO PointShopItems (ItemId, Name, Description, ImagePath, PointPrice, ItemType)
VALUES
(1, 'Blue Profile Background', 'A cool blue background for your profile', 'https://picsum.photos/id/1/200/200', 1000, 'ProfileBackground'),
(2, 'Red Profile Background', 'A vibrant red background for your profile', 'https://picsum.photos/id/20/200/200', 1000, 'ProfileBackground'),
(3, 'Golden Avatar Frame', 'A golden frame for your avatar image', 'https://picsum.photos/id/30/200/200', 2000, 'AvatarFrame'),
(4, 'Silver Avatar Frame', 'A silver frame for your avatar image', 'https://picsum.photos/id/40/200/200', 1500, 'AvatarFrame'),
(5, 'Happy Emoticon', 'Express yourself with this happy emoticon', 'https://picsum.photos/id/50/200/200', 500, 'Emoticon'),
(6, 'Sad Emoticon', 'Express yourself with this sad emoticon', 'https://picsum.photos/id/60/200/200', 500, 'Emoticon'),
(7, 'Gamer Avatar', 'Cool gamer avatar for your profile', 'https://picsum.photos/id/70/200/200', 1200, 'Avatar'),
(8, 'Ninja Avatar', 'Stealthy ninja avatar for your profile', 'https://picsum.photos/id/80/200/200', 1200, 'Avatar'),
(9, 'Space Mini-Profile', 'Space-themed mini profile', 'https://picsum.photos/id/90/200/200', 3000, 'MiniProfile'),
(10, 'Fantasy Mini-Profile', 'Fantasy-themed mini profile', 'https://picsum.photos/id/100/200/200', 3000, 'MiniProfile');

-- Add some items to test user's inventory
INSERT INTO UserInventoryItems (UserId, ItemId, PurchaseDate, IsActive)
VALUES
(1, 1, GETDATE(), 1),  -- Blue background (active)
(1, 3, GETDATE(), 0),  -- Golden frame (inactive)
(1, 5, GETDATE(), 1);  -- Happy emoticon (active)

PRINT 'Point Shop extension setup complete!' 