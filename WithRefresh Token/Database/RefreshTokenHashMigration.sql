/*
    Run against an existing ShopDB after deploying the Hash-based API.

    The legacy columns are retained for schema compatibility, but all raw
    token values are erased. New code never reads or writes those columns.
*/

USE ShopDB;
GO

IF COL_LENGTH(N'dbo.RefreshTokens', N'TokenHash') IS NULL
    ALTER TABLE dbo.RefreshTokens ADD TokenHash CHAR(64) NULL;
GO

IF COL_LENGTH(N'dbo.RefreshTokens', N'ReplacedByTokenHash') IS NULL
    ALTER TABLE dbo.RefreshTokens ADD ReplacedByTokenHash CHAR(64) NULL;
GO

UPDATE dbo.RefreshTokens
SET TokenHash = CONVERT(CHAR(64),
                        HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), Token)),
                        2)
WHERE TokenHash IS NULL
  AND Token IS NOT NULL;

UPDATE dbo.RefreshTokens
SET ReplacedByTokenHash = CONVERT(CHAR(64),
                                  HASHBYTES('SHA2_256', CONVERT(VARCHAR(200), ReplacedByToken)),
                                  2)
WHERE ReplacedByTokenHash IS NULL
  AND ReplacedByToken IS NOT NULL;
GO

IF EXISTS
(
    SELECT 1
    FROM dbo.RefreshTokens
    WHERE TokenHash IS NULL
)
BEGIN
    THROW 51000,
        'Migration stopped: one or more RefreshTokens have no TokenHash.',
        1;
END;
GO

/* Remove any unique constraint/index that still belongs to the raw Token column. */
DECLARE @sql NVARCHAR(MAX) = N'';

SELECT @sql += N'ALTER TABLE dbo.RefreshTokens DROP CONSTRAINT '
    + QUOTENAME(k.name) + N';' + CHAR(13) + CHAR(10)
FROM sys.key_constraints AS k
INNER JOIN sys.index_columns AS ic
    ON ic.object_id = k.parent_object_id
   AND ic.index_id = k.unique_index_id
INNER JOIN sys.columns AS c
    ON c.object_id = ic.object_id
   AND c.column_id = ic.column_id
WHERE k.parent_object_id = OBJECT_ID(N'dbo.RefreshTokens')
  AND c.name = N'Token';

IF @sql <> N''
    EXEC sys.sp_executesql @sql;
GO

/* Erase legacy raw values without changing the table shape. */
UPDATE dbo.RefreshTokens
SET Token = NULL,
    ReplacedByToken = NULL;
GO

/* Keep the invariant explicit after the cleanup. */
IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'UX_RefreshTokens_TokenHash'
      AND object_id = OBJECT_ID(N'dbo.RefreshTokens')
)
BEGIN
    CREATE UNIQUE INDEX UX_RefreshTokens_TokenHash
        ON dbo.RefreshTokens(TokenHash)
        WHERE TokenHash IS NOT NULL;
END;
GO
