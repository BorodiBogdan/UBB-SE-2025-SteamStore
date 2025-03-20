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
	isInCart BIT,
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
go
DROP PROCEDURE IF EXISTS getAllTags;
go
CREATE PROCEDURE getAllTags as
begin
	Select * from tags;
end
go
DROP PROCEDURE IF EXISTS getGameTags;
go
CREATE PROCEDURE getGameTags
    @gid int
AS
BEGIN
    SELECT t.tag_name 
    FROM Games g
    INNER JOIN game_tags gt ON gt.game_id = g.game_id
    INNER JOIN tags t ON t.tag_id = gt.tag_id
    WHERE g.game_id=@gid; 
END;
go
DROP PROCEDURE IF EXISTS getGameRating;
go
CREATE PROCEDURE getGameRating
	@gid int
AS
BEGIN
    SELECT AVG(gr.rating) 
    FROM Games g
    INNER JOIN game_reviews gr ON gr.game_id=g.game_id
    WHERE g.game_id = @gid; 
END;
go
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
    SELECT g.game_id, g.name, g.price, g.publisher_id, g.description, g.image_url, gu.is_purchased, gu.isInCart
    FROM games g
    LEFT JOIN games_users gu ON g.game_id = gu.game_id AND gu.user_id = @user_id
    WHERE gu.isInCart = 1 AND g.status = 'Available';
END;
GO

DROP PROCEDURE IF EXISTS AddGameToCart;
GO

CREATE PROCEDURE AddGameToCart
    @user_id INT,
    @game_id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM games_users WHERE user_id = @user_id AND game_id = @game_id AND isInCart = 1)
    BEGIN
        RAISERROR ('The game is already in the cart.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM games_users WHERE user_id = @user_id AND game_id = @game_id AND is_purchased = 1)
    BEGIN
        RAISERROR ('The game is already purchased.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM games_users WHERE user_id = @user_id AND game_id = @game_id)
    BEGIN
        UPDATE games_users
        SET isInCart = 1
        WHERE user_id = @user_id AND game_id = @game_id;
    END
    ELSE
    BEGIN
        INSERT INTO games_users (user_id, game_id, isInCart)
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
			SET isInCart = 0
			WHERE user_id = @user_id AND game_id = @game_id;
		END
	ELSE
		BEGIN
			INSERT INTO games_users (user_id, game_id, is_purchased)
			VALUES (@user_id, @game_id, 0);
		END
END;
GO

DROP PROCEDURE IF EXISTS RemoveGameFromWishlist
GO

CREATE PROCEDURE RemoveGameFromWishlist
	@user_id INT,
	@game_id INT
	AS
	BEGIN
		IF EXISTS (SELECT * FROM games_users WHERE user_id = @user_id AND game_id = @game_id)
			BEGIN
				UPDATE games_users
				SET isInWishlist = 0
				WHERE user_id = @user_id AND game_id = @game_id;
			END
		ELSE
			BEGIN
				RAISERROR('Game is not in wishlist', 16, 1);
			END
	END;
GO

DROP PROCEDURE IF EXISTS addGameToPurchased
GO

CREATE PROCEDURE addGameToPurchased
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

DROP PROCEDURE IF EXISTS addGameToWishlist
GO

CREATE PROCEDURE addGameToWishlist 
    @user_id INT,
    @game_id INT
AS
BEGIN
    IF EXISTS (SELECT 1 FROM games_users WHERE user_id = @user_id AND game_id = @game_id AND isInWishlist = 1)
    BEGIN
        RAISERROR ('The game is already in the wishlist.', 16, 1);
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM games_users WHERE user_id = @user_id AND game_id = @game_id)
    BEGIN
        UPDATE games_users
        SET isInWishlist = 1
        WHERE user_id = @user_id AND game_id = @game_id;
    END
    ELSE
    BEGIN
        INSERT INTO games_users (user_id, game_id, isInWishlist)
        VALUES (@user_id, @game_id, 1);
    END
END;
GO

-- Insert mock users
INSERT INTO users (user_id, username, balance, point_balance, is_developer)
VALUES
(1, 'john_doe', 100.00, 500.00, 0),
(2, 'jane_smith', 150.00, 300.00, 1),
(3, 'alex_brown', 50.00, 150.00, 0),
(4, 'Behaviour Interactive', 200.00, 1000.00, 1),
(5, 'Valve Corporation', 150.00, 300.00, 1),
(6, 'Nintendo', 250.00, 800.00, 1),
(7, 'Hempuli Oy', 100.00, 500.00, 1),
(8, 'Mobius Digital', 120.00, 600.00, 1),
(9, 'Mojang Studios', 300.00, 900.00, 1),
(10, 'Unknown Worlds Entertainment', 180.00, 700.00, 1),
(11, 'mary_jones', 200.00, 1000.00, 1);

-- Insert mock games
delete from games
INSERT INTO games (game_id, name, price, publisher_id, description, image_url, minimum_requirements, recommended_requirements, status)
VALUES 
(1, 'Risk of Rain 2', 24.99, 3, 'A rogue-like third-person shooter where players fight through hordes of monsters to escape an alien planet.', 'https://upload.wikimedia.org/wikipedia/en/c/c1/Risk_of_Rain_2.jpg', '4GB RAM, 2.5GHz Processor, GTX 580', '8GB RAM, 3.0GHz Processor, GTX 680', 'Available'),
(2, 'Dead by Daylight', 19.99, 4, 'A multiplayer horror game where survivors must evade a killer.', 'https://images-wixmp-ed30a86b8c4ca887773594c2.wixmp.com/f/11986720-4999-4524-9809-1a25313ee2e5/dg8ii9d-d3f5eb42-9041-4ddc-954a-c3f9359e914e.png?token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzdWIiOiJ1cm46YXBwOjdlMGQxODg5ODIyNjQzNzNhNWYwZDQxNWVhMGQyNmUwIiwiaXNzIjoidXJuOmFwcDo3ZTBkMTg4OTgyMjY0MzczYTVmMGQ0MTVlYTBkMjZlMCIsIm9iaiI6W1t7InBhdGgiOiJcL2ZcLzExOTg2NzIwLTQ5OTktNDUyNC05ODA5LTFhMjUzMTNlZTJlNVwvZGc4aWk5ZC1kM2Y1ZWI0Mi05MDQxLTRkZGMtOTU0YS1jM2Y5MzU5ZTkxNGUucG5nIn1dXSwiYXVkIjpbInVybjpzZXJ2aWNlOmZpbGUuZG93bmxvYWQiXX0.XGf2I0nx7hyCw6EFGJ5lEdexo3Uj5emUoC6texzl3A4', '8GB RAM, i3-4170, GTX 760', '16GB RAM, i5-6500, GTX 1060', 'Available'),
(3, 'Counter-Strike 2', 0.00, 5, 'A tactical first-person shooter featuring team-based gameplay.', 'https://sm.ign.com/ign_nordic/cover/c/counter-st/counter-strike-2_jc2d.jpg', '8GB RAM, i5-2500K, GTX 660', '16GB RAM, i7-7700K, GTX 1060', 'Available'),
(4, 'Half-Life 2', 9.99, 5, 'A story-driven first-person shooter that revolutionized the genre.', 'https://media.moddb.com/images/mods/1/47/46951/d1jhx20-dc797b78-5feb-4005-b206-.1.jpg', '512MB RAM, 1.7GHz Processor, DirectX 8 GPU', '1GB RAM, 3.0GHz Processor, DirectX 9 GPU', 'Available'),
(5, 'Mario', 59.99, 6, 'A classic platformer adventure with iconic characters and worlds.', 'https://play-lh.googleusercontent.com/3ZKfMRp_QrdN-LzsZTbXdXBH-LS1iykSg9ikNq_8T2ppc92ltNbFxS-tORxw2-6kGA', 'N/A', 'N/A', 'Available'),
(6, 'The Legend of Zelda', 59.99, 6, 'An epic adventure game where heroes save the kingdom of Hyrule.', 'https://m.media-amazon.com/images/I/71oHNyzdN1L.jpg', 'N/A', 'N/A', 'Available'),
(7, 'Baba Is You', 14.99, 7, 'A puzzle game where you change the rules to solve challenges.', 'https://is5-ssl.mzstatic.com/image/thumb/Purple113/v4/9e/30/61/9e3061a5-b2f0-87ad-9e90-563f37729be5/source/256x256bb.jpg', '2GB RAM, 1.0GHz Processor', '4GB RAM, 2.0GHz Processor', 'Available'),
(8, 'Portal 2', 9.99, 5, 'A mind-bending puzzle-platformer with a dark sense of humor.', 'https://steamuserimages-a.akamaihd.net/ugc/789750822988419716/AB9DA751B588ADA1597DB3318BFED932F994683F/?imw=5000&imh=5000&ima=fit&impolicy=Letterbox&imcolor=%23000000&letterbox=false', '2GB RAM, 1.7GHz Processor, DirectX 9 GPU', '4GB RAM, 3.0GHz Processor, GTX 760', 'Available'),
(9, 'Outer Wilds', 24.99, 8, 'An exploration-based game where you unravel cosmic mysteries.', 'https://img-eshop.cdn.nintendo.net/i/bc850a322c0b2e2b410bf462993fffa602a32803eafd1805ef22774ac634c779.jpg', '6GB RAM, i5-2300, GTX 560', '8GB RAM, i7-6700, GTX 970', 'Available'),
(10, 'Minecraft', 29.99, 9, 'A sandbox game that lets you build and explore infinite worlds.', 'https://cdn2.steamgriddb.com/icon/f0b57183da91a7972b2b3c06b0db5542/32/512x512.png', '4GB RAM, Intel HD 4000', '8GB RAM, GTX 1060', 'Available'),
(11, 'Subnautica', 29.99, 10, 'An underwater survival adventure set on an alien ocean planet.', 'https://www.nintendo.com/eu/media/images/11_square_images/games_18/nintendo_switch_5/SQ_NSwitch_Subnautica_image500w.jpg', '8GB RAM, i5-4590, GTX 550', '16GB RAM, i7-7700K, GTX 1060', 'Available'),
(12, 'Space Invaders', 9.99, 1, 'A classic arcade shooter where you defend Earth from alien invaders.', 'https://static.wikia.nocookie.net/classics/images/a/a1/Space_Invaders_Logo.jpeg/revision/latest?cb=20210725054724', '2GB RAM, 1.2GHz Processor', '4GB RAM, 2.4GHz Processor', 'Available'),
(13, 'Fantasy Quest', 29.99, 2, 'An epic adventure RPG set in a magical world.', 'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTPRuOKdjy9y8-lChDfNMqrurbqEBzs0oGYvw&s', '4GB RAM, 2.0GHz Processor', '8GB RAM, 3.0GHz Processor', 'Available'),
(14, 'Racing Turbo', 19.99, 1, 'A fast-paced racing game with stunning graphics and intense action.', 'https://play-lh.googleusercontent.com/QUM3PMNJvBPb0J5ovrt1WYefhq4ik3LNhIhBDWCSZ_qthzm5F7ODHqfkBoLVhiS0Rdau', '2GB RAM, 1.5GHz Processor', '4GB RAM, 2.5GHz Processor', 'Unavailable');
GO

-- Insert tags into the tags table
INSERT INTO tags (tag_id, tag_name)
VALUES
    (1, 'Rogue-Like'),
    (2, 'Third-Person Shooter'),
    (3, 'Multiplayer'),
    (4, 'Horror'),
    (5, 'First-Person Shooter'),
    (6, 'Action'),
    (7, 'Platformer'),
    (8, 'Adventure'),
    (9, 'Puzzle'),
    (10, 'Exploration'),
    (11, 'Sandbox'),
    (12, 'Survival'),
    (13, 'Arcade'),
    (14, 'RPG'),
    (15, 'Racing');

-- Associate tags with games in the game_tags table
INSERT INTO game_tags (tag_id, game_id)
VALUES
    -- Risk of Rain 2
    (1, 1),
    (2, 1),
    (6, 1),
    -- Dead by Daylight
    (3, 2),
    (4, 2),
    (6, 2),
    -- Counter-Strike 2
    (3, 3),
    (5, 3),
    (6, 3),
    -- Half-Life 2
    (5, 4),
    (6, 4),
    (8, 4),
    -- Mario
    (7, 5),
    (6, 5),
    (8, 5),
    -- The Legend of Zelda
    (8, 6),
    (6, 6),
    (14, 6),
    -- Baba Is You
    (9, 7),
    (8, 7),
    -- Portal 2
    (9, 8),
    (5, 8),
    (6, 8),
    -- Outer Wilds
    (10, 9),
    (8, 9),
    -- Minecraft
    (11, 10),
    (12, 10),
    (8, 10),
    -- Subnautica
    (12, 11),
    (10, 11),
    (8, 11),
    -- Space Invaders
    (13, 12),
    (6, 12),
    -- Fantasy Quest
    (14, 13),
    (8, 13),
    -- Racing Turbo
    (15, 14),
    (6, 14);

INSERT INTO game_reviews (id, game_id, rating, comment, username) 
VALUES 
-- Risk of Rain 2
(1, 1, 4.5, 'Great rogue-like action!', 'gamer123'),
(2, 1, 4.0, 'Fun but tough learning curve.', 'roguelover'),
(3, 1, 4.7, 'Addictive gameplay!', 'space_runner'),
(4, 1, 4.6, 'Co-op is awesome!', 'teamplayer'),

-- Dead by Daylight
(5, 2, 4.8, 'Terrifying and thrilling!', 'horror_fan'),
(6, 2, 4.2, 'Best multiplayer horror game.', 'survivor_pro'),
(7, 2, 4.5, 'Killers are fun to play!', 'slasher_king'),
(8, 2, 4.3, 'Needs better matchmaking.', 'ghosted'),

-- Counter-Strike 2
(9, 3, 4.7, 'Tactical and competitive.', 'cs_master'),
(10, 3, 4.6, 'Improved graphics and gameplay.', 'fps_guy'),
(11, 3, 4.9, 'A must-play shooter!', 'headshot_ace'),

-- Half-Life 2
(12, 4, 5.0, 'One of the best FPS ever!', 'hl2_legend'),
(13, 4, 4.9, 'Story and gameplay are top-notch.', 'gamer_dude'),
(14, 4, 5.0, 'Gordon Freeman is iconic!', 'lambda_fan'),

-- Mario
(15, 5, 4.9, 'Mario never disappoints!', 'nintendo_fan'),
(16, 5, 4.7, 'Classic platformer fun.', 'retro_gamer'),
(17, 5, 5.0, 'A masterpiece!', 'jumpman'),
(18, 5, 4.6, 'Great for all ages.', 'family_gamer'),

-- The Legend of Zelda
(19, 6, 5.0, 'Epic adventure, must play!', 'zelda_fan'),
(20, 6, 4.8, 'Beautiful and engaging.', 'rpg_lover'),
(21, 6, 5.0, 'Breath of the Wild is GOAT.', 'hyrule_warrior'),
(22, 6, 4.7, 'Great puzzles and combat.', 'sword_master'),

-- Baba Is You
(23, 7, 4.5, 'Unique and clever puzzles.', 'puzzle_master'),
(24, 7, 4.3, 'Mind-bending gameplay.', 'indie_gamer'),
(25, 7, 4.6, 'Innovative mechanics.', 'logic_wiz'),

-- Portal 2
(26, 8, 4.9, 'Incredible puzzle design.', 'portal_lover'),
(27, 8, 4.8, 'Hilarious and challenging.', 'glados_fan'),
(28, 8, 5.0, 'Perfect co-op mode!', 'coop_champ'),
(29, 8, 4.7, 'Wish it was longer!', 'puzzle_freak'),

-- Outer Wilds
(30, 9, 4.7, 'Amazing exploration.', 'space_explorer'),
(31, 9, 4.6, 'A masterpiece of discovery.', 'curious_gamer'),
(32, 9, 4.8, 'Great music and story.', 'astro_wanderer'),

-- Minecraft
(33, 10, 5.0, 'Endless creativity!', 'block_builder'),
(34, 10, 4.8, 'Addictive and fun.', 'mine_crafter'),
(35, 10, 5.0, 'Survival mode is the best.', 'crafting_pro'),
(36, 10, 4.9, 'Best sandbox game ever.', 'pixel_adventurer'),

-- Subnautica
(37, 11, 4.7, 'Underwater survival at its best.', 'deep_diver'),
(38, 11, 4.6, 'Great atmosphere and exploration.', 'ocean_explorer'),
(39, 11, 4.9, 'Beautiful and immersive.', 'sea_survivor'),
(40, 11, 4.5, 'Scary deep-sea creatures!', 'aquatic_fear');


-- Ensure user 1 has purchased at least 5 games
INSERT INTO games_users (user_id, game_id, isInWishlist, is_purchased, isInCart)
VALUES 
(1, 1, 0, 1, 1),
(1, 2, 0, 1, 1),
(1, 3, 0, 1, 1),
(1, 4, 0, 1, 1),
(1, 5, 0, 1, 1);
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