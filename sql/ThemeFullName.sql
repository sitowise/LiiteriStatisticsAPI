USE [LiiteriDataIndex]
GO
CREATE FUNCTION ThemeFullName
(
	@ThemeId varchar(max)
)
/*RETURNS @t table (name varchar(max))*/
RETURNS varchar(max)
AS
BEGIN

DECLARE @ThemeString varchar(max)

;WITH
	ReverseThemeHierarchy
AS (
	SELECT
		T.id,
		T.name,
		T.parent_id,
		0 AS hlevel
	FROM
		[Themes] T
	WHERE
		T.id = @ThemeId

	UNION ALL

	SELECT
		T.id,
		T.name,
		T.parent_id,
		hlevel + 1 AS LEVEL
	FROM
		[Themes] T
	INNER JOIN ReverseThemeHierarchy TH ON
		TH.parent_id = T.id
)

SELECT
	@ThemeString = COALESCE(@ThemeString + ' / ', '') + TH.name
FROM
	(SELECT TOP 20 * FROM ReverseThemeHierarchy ORDER BY hlevel DESC) TH

/*INSERT INTO @t (name) VALUES (@ThemeString)*/

RETURN(SELECT @ThemeString)
END

GO
