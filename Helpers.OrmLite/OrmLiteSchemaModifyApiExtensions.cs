using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using ServiceStack.OrmLite.SqlServer;
using System.Data;
using System.Linq.Expressions;

namespace Helpers.OrmLite;

public static class OrmLiteSchemaModifyApiExtensions
{
    public static void ChangeTableName<T>(
        this IDbConnection connection,
        string oldTableName)
    {
        var provider = GetSqlServerDialectProvider(connection);

        var modelDefinition = ModelDefinition<T>.Definition;

        var schemaName = modelDefinition.IsInSchema ? modelDefinition.Schema : "dbo";
        var oldObjectName = $"{schemaName}.{oldTableName}";
        var newObjectName = modelDefinition.ModelName;
        var sql = $"EXEC sp_rename {provider.GetQuotedValue(oldObjectName)}, {provider.GetQuotedValue(newObjectName)};";
        connection.ExecuteSql(sql);
    }

    public static void AddUniqueConstraint<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field)
    {
        var (_, tableName, columnName) = GetQualifiedColumnName(connection, field);

        var constraintName = GetUniqueConstraintName(tableName, columnName);
        var sql = $"ALTER TABLE {tableName} ADD CONSTRAINT {constraintName} UNIQUE({columnName})";
        connection.ExecuteSql(sql);
    }

    public static void DropUniqueConstraint<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field)
    {
        var (_, tableName, columnName) = GetQualifiedColumnName(connection, field);

        var constraintName = GetUniqueConstraintName(tableName, columnName);
        var sql = $"ALTER TABLE {tableName} DROP CONSTRAINT IF EXISTS {constraintName}";
        connection.ExecuteSql(sql);
    }

    public static void DropDefaultConstraint<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field)
    {
        var defaultConstraintName = connection.GetDefaultConstraintName(field);
        if (defaultConstraintName == null)
        {
            return;
        }

        var dialectProvider = GetSqlServerDialectProvider(connection);
        var modelDefinition = ModelDefinition<T>.Definition;

        var tableName = dialectProvider.NamingStrategy.GetTableName(modelDefinition);

        var sql = $"ALTER TABLE {tableName} DROP CONSTRAINT {defaultConstraintName};";
        connection.ExecuteSql(sql);
    }

    public static bool DoesIndexExist<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field)
    {
        var (_, tableName, columnName) = GetQualifiedColumnName(connection, field);
        var indexName = GetIndexName(tableName, columnName);

        var param = new
        {
            tableName,
            indexName
        };

        var sql = @"
            SELECT CASE WHEN EXISTS (
                SELECT * FROM sys.indexes
                WHERE name = @indexName
                AND object_id = OBJECT_ID(@tableName)
            ) THEN 1 ELSE 0 END";

        var result = connection.SqlScalar<int>(sql, param);
        return result > 0;
    }

    public static void CreateIndexIfNotExists<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field,
        bool unique = false)
    {
        if (connection.DoesIndexExist(field))
        {
            return;
        }

        var (_, tableName, columnName) = GetQualifiedColumnName(connection, field);
        var indexName = GetIndexName(tableName, columnName);
        connection.CreateIndex(field, indexName, unique);
    }

    public static void DropIndexIfExists<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field)
    {
        if (!connection.DoesIndexExist(field))
        {
            return;
        }

        var (schemaName, tableName, columnName) = GetQualifiedColumnName(connection, field);
        var indexName = GetIndexName(tableName, columnName);

        var sql = $"DROP INDEX {indexName} ON {schemaName}.{tableName}";
        connection.ExecuteSql(sql);
    }

    private static SqlServerOrmLiteDialectProvider GetSqlServerDialectProvider(IDbConnection connection)
    {
        var dialectProvider = connection.GetDialectProvider();
        if (dialectProvider.GetType() != typeof(SqlServerOrmLiteDialectProvider))
        {
            throw new InvalidOperationException("Only SQL Server Provider supported");
        }

        return (SqlServerOrmLiteDialectProvider)dialectProvider;
    }

    private static (string SchemaName, string TableName, string ColumnName)
        GetQualifiedColumnName<T>(this IDbConnection connection, Expression<Func<T, object>> field)
    {
        var dialectProvider = GetSqlServerDialectProvider(connection);

        var modelDefinition = ModelDefinition<T>.Definition;
        var schemaName = modelDefinition.IsInSchema ? modelDefinition.Schema : "dbo";
        var tableName = dialectProvider.NamingStrategy.GetTableName(modelDefinition);

        var fieldDefinition = modelDefinition.GetFieldDefinition(field);
        var columnName = dialectProvider.NamingStrategy.GetColumnName(fieldDefinition.FieldName);

        return (schemaName, tableName, columnName);
    }

    private static string GetUniqueConstraintName(string tableName, string columnName)
        => $"UQ_{tableName}_{columnName}";

    private static string GetIndexName(string tableName, string columnName)
        => $"IX_{tableName}_{columnName}";

    private static string GetDefaultConstraintName<T>(
        this IDbConnection connection,
        Expression<Func<T, object>> field)
    {
        var (schemaName, tableName, columnName) = GetQualifiedColumnName(connection, field);

        var param = new
        {
            schemaName,
            tableName,
            columnName
        };
        var query = @"SELECT d.name AS DefaultConstraint
                        FROM SYS.TABLES t
                        JOIN SYS.DEFAULT_CONSTRAINTS d
                          ON d.parent_object_id = t.object_id
                        JOIN SYS.COLUMNS c
                          ON c.object_id = t.object_id
                         AND c.column_id = d.parent_column_id
                       WHERE t.schema_id = schema_id(@schemaName)
                         AND t.name = @tableName
                         AND c.name = @columnName";
        return connection.ExecuteScalar<string>(query, param);
    }
}