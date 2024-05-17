using ServiceStack.OrmLite;
using System.Linq.Expressions;

namespace Helpers.OrmLite;

public static partial class SqlExpressionExtensions
{
    public static SqlExpression<T> Paginate<T>(
        this SqlExpression<T> expression,
        int pageIndex,
        int pageSize)
    {
        return expression.Limit(pageSize * pageIndex, pageSize);
    }

    public static SqlExpression<T> Where<T>(
        this SqlExpression<T> expression,
        IEnumerable<Expression<Func<T, bool>>> predicates)
    {
        return predicates.Aggregate(expression, (current, predicate) => current.Where(predicate));
    }

    public static SqlExpression<T> WhereIf<T>(
        this SqlExpression<T> expression,
        bool condition,
        Expression<Func<T, bool>> onTrue)
    {
        return condition ? expression.Where(onTrue) : expression;
    }

    public static SqlExpression<T> WhereIf<T>(
        this SqlExpression<T> expression,
        bool condition,
        Expression<Func<T, bool>> onTrue,
        Expression<Func<T, bool>> onFalse)
    {
        return condition ? expression.Where(onTrue) : expression.Where(onFalse);
    }

    public static SqlExpression<T> WhereIf<T, R>(
        this SqlExpression<T> expression,
        bool condition,
        Expression<Func<T, R, bool>> onTrue,
        Expression<Func<T, R, bool>> onFalse)
    {
        return condition ? expression.Where(onTrue) : expression.Where(onFalse);
    }
}