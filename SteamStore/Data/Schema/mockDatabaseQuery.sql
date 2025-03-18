USE SteamStore

---Run this to use the mock database of the SteamStore project

DROP TABLE IF EXISTS users_items;
DROP TABLE IF EXISTS point_items;
DROP TABLE IF EXISTS store_transaction;
DROP TABLE IF EXISTS games_users;
DROP TABLE IF EXISTS game_tags;
DROP TABLE IF EXISTS tags;
DROP TABLE IF EXISTS status;
DROP TABLE IF EXISTS Specifications;
DROP TABLE IF EXISTS specifications;
DROP TABLE IF EXISTS game_reviews;
DROP TABLE IF EXISTS games;
DROP TABLE IF EXISTS Games;
DROP TABLE IF EXISTS users;
GO

CREATE TABLE users (
    user_id INT PRIMARY KEY,
	username NVARCHAR(255),
    balance DECIMAL(10,2),
    point_balance DECIMAL(10,2),
    is_developer BIT
);

CREATE TABLE games (
    game_id INT PRIMARY KEY,
    name NVARCHAR(255),
    price DECIMAL(10,2),
    publisher_id INT FOREIGN KEY REFERENCES users(user_id),
    description NVARCHAR(MAX),
    image_url NVARCHAR(MAX),
	minimum_requirements NVARCHAR(MAX),
	recommended_requirements NVARCHAR(MAX),
	status NVARCHAR(MAX)
);

CREATE TABLE game_reviews (
    id INT PRIMARY KEY,
    game_id INT FOREIGN KEY REFERENCES games(game_id),
    rating DECIMAL(10, 2),
    comment NVARCHAR(MAX),
    username NVARCHAR(255)
);

CREATE TABLE tags (
    tag_id INT PRIMARY KEY,
    tag_name NVARCHAR(255)
);

CREATE TABLE game_tags (
    tag_id INT FOREIGN KEY REFERENCES tags(tag_id),
    game_id INT FOREIGN KEY REFERENCES games(game_id),
    PRIMARY KEY (tag_id, game_id)
);

CREATE TABLE games_users (
    user_id INT FOREIGN KEY REFERENCES users(user_id),
    game_id INT FOREIGN KEY REFERENCES games(game_id),
    isInWishlist BIT,
    is_purchased BIT,
    PRIMARY KEY (user_id, game_id)
);

CREATE TABLE store_transaction (
    game_id INT FOREIGN KEY REFERENCES games(game_id),
    user_id INT FOREIGN KEY REFERENCES users(user_id),
    date DATETIME,
    amount DECIMAL(10, 2),
    withMoney BIT
);

CREATE TABLE point_items (
    point_item_id INT PRIMARY KEY,
    name NVARCHAR(255),
    description NVARCHAR(MAX),
    price DECIMAL(10, 2),
    image_url NVARCHAR(MAX)
);

CREATE TABLE users_items (
    user_id INT FOREIGN KEY REFERENCES users(user_id),
    item_id INT FOREIGN KEY REFERENCES point_items(point_item_id)
);
GO

-- Stored Procedures
DROP PROCEDURE IF EXISTS GetAllGames;
GO

CREATE PROCEDURE GetAllGames  
AS  
BEGIN  
    SELECT game_id, name, price, publisher_id, description, image_url, minimum_requirements, recommended_requirements, status 
    FROM games;  
END;
GO

DROP PROCEDURE IF EXISTS GetAllCartGames;
GO

CREATE PROCEDURE GetAllCartGames
    @user_id INT
AS
BEGIN
    SELECT g.game_id, g.name, g.price, g.publisher_id, g.description, g.image_url 
    FROM games g
    LEFT JOIN games_users gu ON g.game_id = gu.game_id AND gu.user_id = @user_id
    WHERE gu.is_purchased = 1 AND g.status = 'Available';
END;
GO

DROP PROCEDURE IF EXISTS AddGameToCart;
GO

CREATE PROCEDURE AddGameToCart
	@user_id INT,
	@game_id INT
	AS
	BEGIN
		IF EXISTS (SELECT * FROM games_users WHERE user_id = @user_id AND game_id = @game_id)
			BEGIN
				UPDATE games_users
				SET is_purchased = 1
				WHERE user_id = @user_id AND game_id = @game_id;
			END
		ELSE
			BEGIN
				INSERT INTO games_users (user_id, game_id, is_purchased)
				VALUES (@user_id, @game_id, 1);
			END
	END;
GO

DROP PROCEDURE IF EXISTS RemoveGameFromCart;
GO

CREATE PROCEDURE RemoveGameFromCart
@user_id INT,
@game_id INT
AS
BEGIN
	IF EXISTS (SELECT * FROM games_users WHERE user_id = @user_id AND game_id = @game_id)
		BEGIN
			UPDATE games_users
			SET is_purchased = 0
			WHERE user_id = @user_id AND game_id = @game_id;
		END
	ELSE
		BEGIN
			INSERT INTO games_users (user_id, game_id, is_purchased)
			VALUES (@user_id, @game_id, 0);
		END
END;
GO

-- Insert mock users
INSERT INTO users (user_id, username, balance, point_balance, is_developer)
VALUES 
(1, 'john_doe', 100.00, 500.00, 0),
(2, 'jane_smith', 150.00, 300.00, 1),
(3, 'alex_brown', 50.00, 150.00, 0),
(4, 'mary_jones', 200.00, 1000.00, 1);
GO

-- Insert mock games
INSERT INTO games (game_id, name, price, publisher_id, description, image_url, minimum_requirements, recommended_requirements, status)
VALUES 
(1, 'Space Invaders', 9.99, 2, 'A classic arcade shooter.', 'https://example.com/space_invaders.jpg', '2GB RAM, 1.2GHz', '4GB RAM, 2.4GHz', 'Available'),
(2, 'Fantasy Quest', 29.99, 2, 'An epic RPG.', 'https://example.com/fantasy_quest.jpg', '4GB RAM, 2.0GHz', '8GB RAM, 3.0GHz', 'Available'),
(3, 'Racing Turbo', 19.99, 3, 'A high-speed racing game.', 'https://example.com/racing_turbo.jpg', '2GB RAM, 1.5GHz', '4GB RAM, 2.5GHz', 'Available'),
(4, 'Cyber Battle', 39.99, 1, 'A cyberpunk shooter.', 'https://example.com/cyber_battle.jpg', '6GB RAM, 2.5GHz', '12GB RAM, 3.5GHz', 'Available'),
(5, 'Zombie Apocalypse', 24.99, 3, 'A survival horror game.', 'https://example.com/zombie_apocalypse.jpg', '4GB RAM, 2.0GHz', '8GB RAM, 3.0GHz', 'Available');
GO

-- Ensure user 1 has purchased at least 5 games
INSERT INTO games_users (user_id, game_id, isInWishlist, is_purchased)
VALUES 
(1, 1, 0, 1),
(1, 2, 0, 1),
(1, 3, 0, 1),
(1, 4, 0, 1),
(1, 5, 0, 1);
GO

-- Add transactions for user 1
INSERT INTO store_transaction (game_id, user_id, date, amount, withMoney)
VALUES 
(1, 1, GETDATE(), 9.99, 1),
(2, 1, GETDATE(), 29.99, 1),
(3, 1, GETDATE(), 19.99, 1),
(4, 1, GETDATE(), 39.99, 1),
(5, 1, GETDATE(), 24.99, 1);
GO