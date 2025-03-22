DROP PROCEDURE IF EXISTS GetGameTags;
GO

CREATE PROCEDURE GetGameTags
    @game_id int
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT t.tag_id, t.tag_name 
    FROM tags t
    INNER JOIN game_tags gt ON gt.tag_id = t.tag_id
    WHERE gt.game_id = @game_id;
END;
GO 