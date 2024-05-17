using ServiceStack.OrmLite;
using System.Data;
using System.Linq.Expressions;

namespace Helpers.OrmLite;

public static partial class SqlExpressionExtensions
{
    /// <summary>
    /// Join with a DataTable of identifiers. It requires
    /// the <see cref="Converters.DataTableParameterConverter"/> 
    /// to be registered into the DialectProvider first.
    ///
    /// It requires the DataTable to have an "Id" column.
    /// </summary>
    public static SqlExpression<T> JoinToDataTable<T>(
        this SqlExpression<T> self,
        Expression<Func<T, int>> expression,
        DataTable table)
    {
        var sourceDefinition = ModelDefinition<T>.Definition;

        var property = self.Visit(expression);
        var parameter = self.ConvertToParam(table);

        // Expected Sql:
        // INNER JOIN @0 ON ("Parent"."EvaluatedExpression"= "@0"."Id")
        var onExpression = @$"ON ({self.SqlTable(sourceDefinition)}.{self.SqlColumn(property.ToString())} = ""{parameter}"".""Id"")";
        var customSql = $"INNER JOIN {parameter} {onExpression}";
        self.CustomJoin(customSql);

        return self;
    }
}
