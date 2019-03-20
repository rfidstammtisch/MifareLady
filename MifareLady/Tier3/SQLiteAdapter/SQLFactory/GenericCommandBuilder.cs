using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using SQLiteAdapter.Statements;
using SQLiteAdapter.Statements.Enums;
using NLog;
using SQLiteAdapter.Models;

namespace SQLiteAdapter.SQLFactory
{
    public class GenericCommandBuilder : ICommandBuilder
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        #region Typemap
        protected Dictionary<DbType, string> _TYPEMAP
        {
            get
            {
                return new Dictionary<DbType, string>
                {
                    {DbType.Byte, "number"},
                    {DbType.Boolean, "number"},
                    {DbType.Int16, "number"},
                    {DbType.Int32, "number"},
                    {DbType.Int64, "number"},
                    {DbType.UInt16, "number"},
                    {DbType.UInt32, "number"},
                    {DbType.UInt64, "number"},
                    {DbType.VarNumeric, "number"},
                    {DbType.Binary, "blob"},
                    {DbType.Object, "blob"},
                    {DbType.SByte, "blob"},
                    {DbType.Single, "blob"},
                    {DbType.Guid, "nvarchar (255)"},
                    {DbType.AnsiString, "nvarchar (255)"},
                    {DbType.String, "nvarchar (255)"},
                    {DbType.AnsiStringFixedLength, "nvarchar (255)"},
                    {DbType.StringFixedLength, "nvarchar (255)"},
                    {DbType.Xml, "nvarchar (255)"},
                    {DbType.Time, "datetime"},
                    {DbType.Date, "date"},
                    {DbType.DateTimeOffset, "datetime"},
                    {DbType.DateTime, "datetime"},
                    {DbType.DateTime2, "datetime"},
                    {DbType.Decimal, "float"},
                    {DbType.Double, "float"},
                    {DbType.Currency, "float"},
                };

            }
        }
        #endregion

        #region Properties
        public string Prefix { get; set; }
        #endregion

        #region Ctor

        public GenericCommandBuilder()
        {
            Prefix = "@";
        }
        #endregion


        #region Interface members
        public DbCommand SelectCommand(DbConnection connection, IList<string> selectedTables, IList<string> selectedFields = null,
            WhereStatement whereStatement = null, WhereStatement havingStatement = null, IList<string> groupBy = null,
            IList<OrderByClause> orderByClause = null, bool distinct = false)
        {
            return BuildSelectCommand(connection, selectedTables, selectedFields, whereStatement, havingStatement, groupBy, orderByClause, distinct);
        }

        public DbCommand InsertCommand(DbConnection connection, string table, params Dictionary<string, object>[] values)
        {
            return BuildInsertCommand(connection, table, values);
        }

        public DbCommand UpdateCommand(DbConnection connection, string table, Dictionary<string, object> values, WhereStatement whereStatement = null)
        {
            return BuildUpdateCommand(connection, table, values, whereStatement);
        }

        public DbCommand DeleteCommand(DbConnection connection, string table, WhereStatement whereStatement = null)
        {
            return BuildDeleteCommand(connection, table, whereStatement);
        }

        public DbCommand CreateCommand(DbConnection connection, string table, IList<DataField> fields)
        {
            return BuildCreateCommand(connection, table, fields);
        }

        public DbCommand AddColumnsCommand(DbConnection connection, string table, IList<DataField> fields)
        {
            return BuildAddColumnsCommand(connection, table, fields);
        }
        #endregion

        #region protected methods
        /// <summary>
        /// Builds the select query
        /// </summary>
        /// <returns>Returns a string containing the query, or a DbCommand containing a command with parameters</returns>
        protected virtual DbCommand BuildSelectCommand(DbConnection connection, IList<string> selectedTables, IList<string> selectedFields,
            WhereStatement whereStatement, WhereStatement havingStatement, IList<string> groupBy,
            IList<OrderByClause> orderByClause, bool distinct)
        {
            if (connection == null)
                throw new Exception("No connection available");

            DbCommand command;
            try
            {
                command = connection.CreateCommand();
            }
            catch (Exception ex)
            {
                Log.Error("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                    Log.Debug("InnerException: {0}", ex.InnerException.Message);
                Log.Trace("Trace: {0}", ex.StackTrace);
                return null;
            }


            var query = "SELECT ";

            // Output Distinct
            if (distinct)
            {
                query += "DISTINCT ";
            }

            // Output column names
            if (selectedFields == null || selectedFields.Count == 0)
            {
                if (selectedTables != null && selectedTables.Count == 1)
                    query += selectedTables[0] + "."; // By default only select * from the table that was selected. If there are any joins, it is the responsibility of the user to select the needed columns.

                query += "*";
            }
            else
            {
                query = selectedFields.Aggregate(query, (current, columnName) => current + (columnName + ','));
                query = query.TrimEnd(','); // Trim de last comma inserted by foreach loop
                query += ' ';
            }

            // Output table names
            if (selectedTables != null && selectedTables.Count > 0)
            {
                query += " FROM ";
                query = selectedTables.Aggregate(query, (current, tableName) => current + (tableName + ','));
                query = query.TrimEnd(','); // Trim de last comma inserted by foreach loop
                query += ' ';
            }

            // Output where statement
            if (whereStatement != null && whereStatement.ClauseLevels > 0)
                query += " WHERE " + BuildWhereStatement(ref command, whereStatement);

            // Output GroupBy statement
            if (groupBy != null && groupBy.Count > 0)
            {
                query += " GROUP BY ";
                query = groupBy.Aggregate(query, (current, column) => current + (column + ','));
                query = query.TrimEnd(',');
                query += ' ';
            }

            // Output having statement
            if (havingStatement != null && havingStatement.ClauseLevels > 0)
            {
                // Check if a Group By Clause was set
                if (groupBy == null || groupBy.Count == 0)
                    throw new SyntaxErrorException("Having statement was set without Group By");

                query += " HAVING " + BuildWhereStatement(ref command, whereStatement);

            }

            // Output OrderBy statement
            if (orderByClause != null && orderByClause.Count > 0)
            {
                query += " ORDER BY ";
                foreach (var clause in orderByClause)
                {
                    var obc = " ";
                    switch (clause.SortOrder)
                    {
                        case Sorting.Ascending:
                            obc = clause.FieldName + " ASC"; 
                            break;
                        case Sorting.Descending:
                            obc = clause.FieldName + " DESC"; 
                            break;
                    }
                    query += obc + ',';
                }
                query = query.TrimEnd(','); // Trim de last AND inserted by foreach loop
                query += ' ';
            }

            // Return the build command
            command.CommandText = query;
            return command;
        }

        protected virtual DbCommand BuildUpdateCommand(DbConnection connection, string table, Dictionary<string, object> values,
            WhereStatement whereStatement)
        {
            if (connection == null)
                throw new Exception("No connection available");

            if (string.IsNullOrWhiteSpace(table))
                throw new Exception("No table available");

            if (values == null || values.Count == 0)
                return null;

            DbCommand command;
            try
            {
                command = connection.CreateCommand();
            }
            catch (Exception ex)
            {
                Log.Error("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                    Log.Debug("InnerException: {0}", ex.InnerException.Message);
                Log.Trace("Trace: {0}", ex.StackTrace);
                return null;
            }

            var query = string.Format("UPDATE {0} SET ", table);

            // output set statement
            query += BuildSetStatement(ref command, values);

            // Output where statement
            if (whereStatement != null && whereStatement.ClauseLevels > 0)
                query += " WHERE " + BuildWhereStatement(ref command, whereStatement);

            // Return the build command
            command.CommandText = query;
            return command;
        }

        protected virtual DbCommand BuildInsertCommand(DbConnection connection, string table, params Dictionary<string, object>[] values)
        {
            if (connection == null)
                throw new Exception("No connection available");

            if (string.IsNullOrWhiteSpace(table))
                throw new Exception("No table available");

            if (values == null || values.Length == 0)
                return null;

            DbCommand command;
            try
            {
                command = connection.CreateCommand();
            }
            catch (Exception ex)
            {
                Log.Error("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                    Log.Debug("InnerException: {0}", ex.InnerException.Message);
                Log.Trace("Trace: {0}", ex.StackTrace);
                return null;
            }

            var query = string.Format("INSERT INTO {0} ", table);

            // output set statement
            query += BuildInsertStatement(ref command, values);

            // Return the build command
            command.CommandText = query;

            return command;
        }


        public DbCommand BuildAddColumnsCommand(DbConnection connection, string table, IList<DataField> fields)
        {
            if (connection == null)
                throw new Exception("No connection available");

            DbCommand command;
            try
            {
                command = connection.CreateCommand();
            }
            catch (Exception ex)
            {
                Log.Error("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                    Log.Debug("InnerException: {0}", ex.InnerException.Message);
                Log.Trace("Trace: {0}", ex.StackTrace);
                return null;
            }

            // ALTER TABLE
            var query = $"ALTER TABLE {table} ADD {{0}}";
            var fieldList = new List<string>();
            foreach (var field in fields)
            {
                // trim for whitspaces
                if (field.ColumnName.Trim().Length == 0) continue;

                // get string for type
                var type = _TYPEMAP[field.DataType];
                if (field.IsPrimary)
                    type += " primary key";

                var nullValue = field.AllowDBNull ? "null" : "not null";

                // example: [name] [string] not null DEFAULT test
                fieldList.Add(string.Format($"[{field.ColumnName}] [{type}] {nullValue} {{0}}", !field.AllowDBNull ? $"DEFAULT {field.DefaultValue}" : string.Empty));
            }

            // concat all field lines with ', ' to build one query
            command.CommandText = string.Format(query, string.Join(",", fieldList));
            return command;
        }

        protected virtual DbCommand BuildDeleteCommand(DbConnection connection, string table, WhereStatement whereStatement)
        {
            if (connection == null)
                throw new Exception("No connection available");

            if (string.IsNullOrWhiteSpace(table))
                throw new Exception("No table available");

            DbCommand command;
            try
            {
                command = connection.CreateCommand();
            }
            catch (Exception ex)
            {
                Log.Error("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                    Log.Debug("InnerException: {0}", ex.InnerException.Message);
                Log.Trace("Trace: {0}", ex.StackTrace);
                return null;
            }

            var query = string.Format("DELETE FROM {0}", table);

            // Output where statement
            if (whereStatement != null && whereStatement.ClauseLevels > 0)
                query += " WHERE " + BuildWhereStatement(ref command, whereStatement);

            // Return the build command
            command.CommandText = query;
            return command;
        }

        protected virtual DbCommand BuildCreateCommand(DbConnection connection, string table, IList<DataField> fields)
        {
            if (connection == null)
                throw new Exception("No connection available");

            if (string.IsNullOrWhiteSpace(table))
                throw new Exception("No table available");

            DbCommand command;
            try
            {
                command = connection.CreateCommand();
            }
            catch (Exception ex)
            {
                Log.Error("Exception: {0}", ex.Message);
                if (ex.InnerException != null)
                    Log.Debug("InnerException: {0}", ex.InnerException.Message);
                Log.Trace("Trace: {0}", ex.StackTrace);
                return null;
            }

            var query = string.Format("CREATE TABLE IF NOT EXISTS {0} ", table);

            // output create statement
            query += BuildCreateStatement(fields);

            // Return the build command
            command.CommandText = query;
            return command;
        }

        protected virtual string BuildCreateStatement(IList<DataField> fields)
        {
            var fieldList = new List<string>();

            if (fields == null || !fields.Any()) return string.Empty;

            foreach (var field in fields)
            {
                if (field.ColumnName.Trim().Length == 0) continue;

                // get string for type
                var type = _TYPEMAP[field.DataType];

                if (field.AutoIncrement)
                {
                    fieldList.Add(string.Format("[{0}]  INTEGER PRIMARY KEY AUTOINCREMENT", field.ColumnName));
                    continue;
                }

                if (field.IsPrimary)
                    type += " primary key";

                var nullValue = field.AllowDBNull ? "null" : "not null";

                fieldList.Add(string.Format("[{0}] [{1}] {2}", field.ColumnName, type, nullValue));
            }

            return string.Format("({0})", string.Join(",", fieldList));
        }

        protected virtual string BuildInsertStatement(ref DbCommand usedDbCommand, params Dictionary<string, object>[] valuesCollection)
        {
            var fieldList = new List<string>();
            var parameterList = new List<string>();

            var valueQuery = string.Empty;

            // Loop through all values
            foreach (var collection in valuesCollection)
            {
                parameterList.Clear();
                fieldList.Clear();

                // Loop through all value pairs
                foreach (var value in collection)
                {
                    // Create a parameter
                    string parameterName = string.Format(
                        "{0}p{1}_{2}",
                        Prefix,
                        usedDbCommand.Parameters.Count + 1,
                        value.Key.Replace('.', '_')
                        );

                    DbParameter parameter = usedDbCommand.CreateParameter();
                    parameter.ParameterName = parameterName;
                    // empty strings should result in null-value
                    var forceNull = value.Value == null || string.IsNullOrEmpty(value.Value.ToString());
                    parameter.Value = forceNull ? null : value.Value;
                    usedDbCommand.Parameters.Add(parameter);

                    // save field name and parameter name
                    //fieldList.Add(string.Format("'{0}'", value.Key));
                    fieldList.Add(string.Format("{0}", value.Key));
                    parameterList.Add(parameterName);
                }

                valueQuery += string.Format("({0}), ", string.Join(",", parameterList));
            }
            valueQuery = valueQuery.Trim().Trim(',');

            return string.Format("({0}) VALUES {1}", string.Join(",", fieldList), valueQuery);
        }
        
        protected virtual string BuildSetStatement(ref DbCommand usedDbCommand, Dictionary<string, object> values)
        {
            var valueClauseList = new List<string>();

            // Loop through all value pairs
            foreach (var value in values)
            {
                // Create a parameter
                string parameterName = string.Format(
                    "{0}p{1}_{2}",
                    Prefix,
                    usedDbCommand.Parameters.Count + 1,
                    value.Key.Replace('.', '_')
                    );

                DbParameter parameter = usedDbCommand.CreateParameter();
                parameter.ParameterName = parameterName;
                var forceNull = value.Value == null || string.IsNullOrEmpty(value.Value.ToString());
                parameter.Value = forceNull ? null : value.Value;
                usedDbCommand.Parameters.Add(parameter);

                // Create a where clause using the parameter, instead of its value
                valueClauseList.Add(CreateComparisonClause(value.Key, Comparison.Equals, new SqlLiteral(parameterName)));
            }

            var query = string.Join(",", valueClauseList);

            return query;
        }

        protected virtual string BuildWhereStatement(ref DbCommand usedDbCommand, WhereStatement @where)
        {
            string query = string.Empty;

            // Loop through all statement levels, OR them together
            foreach (var whereStatement in where)
            {
                var levelWhere = string.Empty;

                // Loop through all conditions, AND them together
                foreach (var clause in whereStatement)
                {
                    var whereClause = string.Empty;

                    // Create a parameter
                    string parameterName = string.Format(
                        "{0}p{1}_{2}",
                        Prefix,
                        usedDbCommand.Parameters.Count + 1,
                        clause.FieldName.Replace('.', '_')
                        );

                    DbParameter parameter = usedDbCommand.CreateParameter();
                    parameter.ParameterName = parameterName;

                    // for using "like" operator surround value with '%'
                    switch (clause.ComparisonOperator)
                    {
                        case Comparison.Begins:
                            parameter.Value = string.Format("{0}%", clause.Value);
                            break;
                        case Comparison.Ends:
                            parameter.Value = string.Format("%{0}", clause.Value);
                            break;
                        case Comparison.Like:
                            parameter.Value = string.Format("%{0}%", clause.Value);
                            break;
                        case Comparison.NotLike:
                            parameter.Value = string.Format("%{0}%", clause.Value);
                            break;
                        default:
                            parameter.Value = clause.Value;
                            break;
                    }
                    usedDbCommand.Parameters.Add(parameter);

                    // Create a where clause using the parameter, instead of its value
                    whereClause += CreateComparisonClause(clause.FieldName, clause.ComparisonOperator, clause.Value != null ? new SqlLiteral(parameterName) : null);

                    // Loop through all subclauses, append them together with the specified logic operator
                    if(clause.SubClauses != null)
                        foreach (var subWhereClause in clause.SubClauses)
                        {
                            switch (subWhereClause.LogicOperator)
                            {
                                case LogicOperator.And:
                                    whereClause += " AND "; 
                                    break;
                                case LogicOperator.Or:
                                    whereClause += " OR "; 
                                    break;
                            }

                            // Create a parameter
                            parameterName = string.Format(
                                "{0}p{1}_{2}",
                                Prefix,
                                usedDbCommand.Parameters.Count + 1,
                                clause.FieldName.Replace('.', '_')
                                );

                            parameter = usedDbCommand.CreateParameter();
                            parameter.ParameterName = parameterName;
                            parameter.Value = subWhereClause.Value;
                            usedDbCommand.Parameters.Add(parameter);

                            // Create a where clause using the parameter, instead of its value
                            whereClause += CreateComparisonClause(clause.FieldName, subWhereClause.ComparisonOperator, new SqlLiteral(parameterName));
                        }
                        levelWhere += "(" + whereClause + ") AND ";
                    }

                levelWhere = levelWhere.Substring(0, levelWhere.Length - 5); // Trim de last AND inserted by foreach loop
                if (whereStatement.Count > 1)
                {
                    query += " (" + levelWhere + ") ";
                }
                else
                {
                    query += " " + levelWhere + " ";
                }
                query += " OR";
            }
            query = query.Substring(0, query.Length - 2); // Trim de last OR inserted by foreach loop

            return query;
        }

        protected virtual string CreateComparisonClause(string fieldName, Comparison comparisonOperator, object value)
        {
            string output = string.Empty;
            if (value != null && value != DBNull.Value)
            {
                switch (comparisonOperator)
                {
                    case Comparison.Equals:
                        output = fieldName + " = " + FormatSQLValue(value); 
                        break;
                    case Comparison.NotEquals:
                        output = fieldName + " <> " + FormatSQLValue(value); 
                        break;
                    case Comparison.GreaterThan:
                        output = fieldName + " > " + FormatSQLValue(value); 
                        break;
                    case Comparison.GreaterOrEquals:
                        output = fieldName + " >= " + FormatSQLValue(value); 
                        break;
                    case Comparison.LessThan:
                        output = fieldName + " < " + FormatSQLValue(value); 
                        break;
                    case Comparison.LessOrEquals:
                        output = fieldName + " <= " + FormatSQLValue(value); 
                        break;
                    //case Comparison.Like:
                    //    Output = fieldName + " LIKE " + FormatSQLValue(string.Format("%{0}%", value)); break;
                    //case Comparison.NotLike:
                    //    Output = "NOT " + fieldName + " LIKE " + FormatSQLValue(string.Format("%{0}%", value)) ; break;
                    //case Comparison.Begins:
                    //    Output = fieldName + " LIKE " + FormatSQLValue(string.Format("{0}%", value)); break;
                    //case Comparison.Ends:
                    //    Output = fieldName + " LIKE " + FormatSQLValue(string.Format("%{0}", value)); break;
                    case Comparison.Like:
                        output = fieldName + " LIKE " + FormatSQLValue(value); 
                        break;
                    case Comparison.NotLike:
                        output = "NOT " + fieldName + " LIKE " + FormatSQLValue(value); 
                        break;
                    case Comparison.Begins:
                        output = fieldName + " LIKE " + FormatSQLValue(value); 
                        break;
                    case Comparison.Ends:
                        output = fieldName + " LIKE " + FormatSQLValue(value); 
                        break;
                    case Comparison.In:
                        output = fieldName + " IN (" + FormatSQLValue(value) + ")"; 
                        break;
                }
            }
            else 
            {
                if ((comparisonOperator != Comparison.Equals) && (comparisonOperator != Comparison.NotEquals))
                {
                    throw new Exception("Cannot use comparison operator " + comparisonOperator.ToString() + " for NULL values.");
                }
                else
                {
                    switch (comparisonOperator)
                    {
                        case Comparison.Equals:
                            output = fieldName + " IS NULL"; 
                            break;
                        case Comparison.NotEquals:
                            output = "NOT " + fieldName + " IS NULL"; 
                            break;
                    }
                }
            }
            return output;
        }

        protected virtual string FormatSQLValue(object someValue, string prefix = "", string postfix = "")
        {
            string formattedValue;

            if (someValue == null)
            {
                formattedValue = "NULL";
            }
            else
            {
                switch (someValue.GetType().Name)
                {
                    case "String": formattedValue = "'" + ((string)someValue).Replace("'", "''") + "'"; 
                        break;
                    case "DateTime": formattedValue = "'" + ((DateTime)someValue).ToString("yyyy/MM/dd hh:mm:ss") + "'"; 
                        break;
                    case "DBNull": formattedValue = "NULL"; 
                        break;
                    case "Boolean": formattedValue = (bool)someValue ? "1" : "0"; 
                        break;
                    case "SqlLiteral": formattedValue = ((SqlLiteral)someValue).Value; 
                        break;
                    default: formattedValue = someValue.ToString(); 
                        break;
                }
            }

            return prefix + formattedValue + postfix;
        }

        #endregion
    }

}
