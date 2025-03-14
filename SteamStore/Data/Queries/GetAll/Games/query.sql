CREATE PROCEDURE GetAllGames  
AS  
BEGIN  
    SELECT game_id, name, price, publisher_id, description, image_url  
    FROM Games;  
END;
