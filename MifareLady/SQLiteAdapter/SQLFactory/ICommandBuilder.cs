using SQLiteAdapter.Models;
using SQLiteAdapter.Statements;
using System.Collections.Generic;
using System.Data.Common;

namespace SQLiteAdapter.SQLFactory
{
    public interface ICommandBuilder
    {
        DbCommand SelectCommand(DbConnection connection, IList<string> selectedTables, IList<string> selectedFields = null, WhereStatement whereStatement=null, WhereStatement havingStatement=null, IList<string> groupBy=null, IList<OrderByClause> orderByClause=null, bool distinct=false);
        DbCommand InsertCommand(DbConnection connection, string table, params Dictionary<string, object>[] values);
        DbCommand UpdateCommand(DbConnection connection, string table, Dictionary<string, object> values, WhereStatement whereStatement = null);
        DbCommand DeleteCommand(DbConnection connection, string table, WhereStatement whereStatement = null);
        DbCommand CreateCommand(DbConnection connection, string table, IList<DataField> fields);
    }
}
