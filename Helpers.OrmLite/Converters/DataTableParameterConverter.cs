using ServiceStack.OrmLite;
using System.Data.SqlClient;
using System.Data;

namespace Helpers.OrmLite.Converters;

/// <summary>
/// This converter only works when passing a DataTable
/// as a parameter to OrmLite's SelectAsync and
/// LoadSelectAsync methods.
/// 
/// It expects a table type on the database.
/// For example:
/// 
/// "CREATE TYPE dbo.IntList AS TABLE(Id INT NULL)"
///
/// Use it with the <see cref="JoinToDataTable"/>
/// extension method.
/// </summary>
public class DataTableParameterConverter : OrmLiteConverter
{
    public override string ColumnDefinition
        => throw new NotImplementedException("Only use to pass DataTable as parameter.");

    public override void InitDbParam(IDbDataParameter p, Type fieldType)
    {
        if (p is SqlParameter sqlParameter)
        {
            sqlParameter.SqlDbType = SqlDbType.Structured;
            sqlParameter.TypeName = "dbo.IntList";
        }
    }

    // It works with a DataTable parameter with an
    // "Id" column.
    //
    // OrmLite 6.8.0 LoadSelect and LoadSelectAsync
    // methods don't use parameterized queries to load
    // references. It turns the DataTable into a
    // comma-separated list of int.
    public override string ToQuotedString(Type fieldType, object value)
    {
        var dataTable = (DataTable)value;
        var ids = dataTable.AsEnumerable().Select(row => row.Field<int>("Id"));
        return string.Join(",", ids);
    }
}