using ServiceStack.OrmLite;
using ServiceStack.Text;
using System.Collections;
using System.Linq.Expressions;

namespace Helpers.OrmLite;

public static partial class SqlExpressionExtensions
{
    public static SqlExpression<T> Join<T, TSubquery>(
        this SqlExpression<T> expression,
        SqlExpression<TSubquery> subquery,
        Expression<Func<T, TSubquery, bool>> joinExpr,
        string subqueryAlias)
    {
        return expression.CreateJoin("INNER JOIN", subquery, joinExpr, subqueryAlias);
    }

    public static SqlExpression<T> LeftJoin<T, TSubquery>(
        this SqlExpression<T> expression,
        SqlExpression<TSubquery> subquery,
        Expression<Func<T, TSubquery, bool>> joinExpr,
        string subqueryAlias)
    {
        return expression.CreateJoin("LEFT JOIN", subquery, joinExpr, subqueryAlias);
    }

    private static SqlExpression<T> CreateJoin<T, TSubquery>(
        this SqlExpression<T> expression,
        string joinType,
        SqlExpression<TSubquery> subquery,
        Expression<Func<T, TSubquery, bool>> joinExpr,
        string subqueryAlias)
    {
        // This is to "move" parameters from the subquery
        // to the parent query while keeping the right
        // parameter count and order.
        //
        // Otherwise, we could have a parameter named '@0'
        // on the parent and subquery that refer to
        // different columns and values.
        var subqueryParams = subquery.Params.Select(t => t.Value!).ToArray();
        var subquerySql = FormatFilter(expression, subquery.ToSelectStatement(), filterParams: subqueryParams);

        // This is a hacky way of replacing the original
        // table name from the join condition with the
        // subquery alias
        // From:
        //      "table1"."Id" = "table2"."Table1Id"
        // To:
        //      "table1"."Id" = "mySubqueryAlias"."Table1Id"
        var originalCondition = expression.Visit(joinExpr).ToString();

        var definition = ModelDefinition<TSubquery>.Definition;
        var aliasCondition = definition.Alias == null
                                ? originalCondition
                                : originalCondition!.Replace(definition.Alias, subqueryAlias);

        // For example,
        // LEFT JOIN (SELECT Column1 FROM ...) cte ON parent.Id = cte.parentId
        expression = expression.CustomJoin<TSubquery>($"{joinType} ({subquerySql}) {subqueryAlias} ON {aliasCondition}");

        return expression;
    }

    // Taken from:
    // https://github.com/ServiceStack/ServiceStack/blob/main/ServiceStack.OrmLite/src/ServiceStack.OrmLite/Expressions/SqlExpression.cs#L557
    private static string FormatFilter<T>(SqlExpression<T> query, string sqlFilter, params object[] filterParams)
    {
        if (string.IsNullOrEmpty(sqlFilter))
        {
            return string.Empty;
        }

        for (var i = 0; i < filterParams.Length; i++)
        {
            var pLiteral = "{" + i + "}";
            var filterParam = filterParams[i];

            if (filterParam is SqlInValues sqlParams)
            {
                if (sqlParams.Count > 0)
                {
                    var sqlIn = CreateInParamSql(query, sqlParams.GetValues());
                    sqlFilter = sqlFilter.Replace(pLiteral, sqlIn);
                }
                else
                {
                    sqlFilter = sqlFilter.Replace(pLiteral, SqlInValues.EmptyIn);
                }
            }
            else
            {
                var p = query.AddParam(filterParam);
                sqlFilter = sqlFilter.Replace(pLiteral, p.ParameterName);
            }
        }
        return sqlFilter;
    }

    // Taken from:
    // https://github.com/ServiceStack/ServiceStack/blob/main/ServiceStack.OrmLite/src/ServiceStack.OrmLite/Expressions/SqlExpression.cs#L588
    private static string CreateInParamSql<T>(SqlExpression<T> query, IEnumerable values)
    {
        var sbParams = StringBuilderCache.Allocate();
        foreach (var item in values)
        {
            var p = query.AddParam(item);

            if (sbParams.Length > 0)
                sbParams.Append(",");

            sbParams.Append(p.ParameterName);
        }
        var sqlIn = StringBuilderCache.ReturnAndFree(sbParams);
        return sqlIn;
    }
}