
-- This example tests that the scripts are sorted by natural sort (9, 10), not lexicographically (10, 9),
-- since view B depends on view A.

IF OBJECT_ID('dbo.B') IS NULL
    EXEC ('CREATE VIEW B AS SELECT N = 0');

GO

/*DatabaseGenerator:NoTransaction*/

-- This example also tests that the transaction which created ScriptA was committed before this ScriptB is executed without transaction.

DECLARE @t NVARCHAR(100) = CONVERT(NVARCHAR(100), @@TRANCOUNT);
DECLARE @sql NVARCHAR(max) = 'ALTER VIEW B AS SELECT TRANCOUNT = ' + @t + ', Y = X+1 FROM A';
EXEC (@sql)
