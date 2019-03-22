using NLog;
using SQLiteAdapter.Models;
using SQLiteAdapter.SQLFactory;
using SQLiteAdapter.Statements;
using SQLiteAdapter.Statements.Enums;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteAdapter.Controller
{
    public class StoreProvider
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private static List<DataField> dataFields;

        public static void CreateStore(string name, Dictionary<string, DbType> fields)
        {
            dataFields = new List<DataField>();
            foreach (var key in fields.Keys)
                dataFields.Add(new DataField
                {
                    AllowDBNull = true,
                    AutoIncrement = false,
                    IsPrimary = false,
                    ColumnName = key,
                    DataType = fields[key],
                    DefaultValue = string.Empty,
                });

            if (dataFields.Count > 0)
            {
                dataFields[0].IsPrimary = true;

                SQLiteProvider.Provider.CreateStore(name, dataFields);
            }
        }

        public static void UpdateStore(string name, Dictionary<string, object> entry)
        {
            var dbTypes = new Dictionary<string, DbType>();
            foreach (var key in entry.Keys)
            {
                if (entry[key] == null)
                    continue;

                dbTypes[key] = GetDbType(entry[key].GetType());
            }

            CreateStore(name, dbTypes);

            // TODO: format prüfen ist es HEX oder INT --> vermutung is hex, in dem Fall einfach das primary feld ändern
            var primary = dataFields.First(field => field.IsPrimary).ColumnName;

            // Build primary-key-where by recordnumber
            var whereClause = new WhereClause { FieldName = primary, ComparisonOperator = Comparison.Equals, Value = entry[primary] };
            var statement = new WhereStatement(whereClause);

            // update all db´s for new fields
            var schema = SQLiteProvider.Provider.GetStoreSchema(name);
            if (entry.Keys.Any(e => !schema.Any(field => field.ColumnName == e)))
            {
                var newColumns = entry.Keys.Where(e => !schema.Any(field => field.ColumnName == e)).Select(e => new DataField
                {
                    AllowDBNull = true,
                    IsPrimary = false,
                    AutoIncrement = false,
                    ColumnName = e,
                    DataType = GetDbType(entry[e].GetType()),
                }).ToList();
                SQLiteProvider.Provider.AddColumns(name, newColumns);
            }

            var check = SQLiteProvider.Provider.Single(name, statement, "*");
            if (check != null)
                SQLiteProvider.Provider.Update(name, statement, entry);
            else
                SQLiteProvider.Provider.Insert(name, entry);
        }

        public static void DeleteEntry(string name, Dictionary<string, object> entry)
        {
            entry["LASTUPDATE"] = DateTime.Now;

            var dbTypes = new Dictionary<string, DbType>();
            foreach (var key in entry.Keys)
            {
                if (entry[key] == null)
                    continue;

                dbTypes[key] = GetDbType(entry[key].GetType());
            }

            // do not create store in deletion, because 
            // there are only two columns to share
            //CreateStore(name, dbTypes);

            // if there are no datafields there is also no table or db
            // in deletion is no need to create a store with 2 columns
            // entry is not complete because only primary key is known
            if (dataFields == null)
                return;

            var primary = dataFields.First(field => field.IsPrimary).ColumnName;

            // Build primary-key-where by recordnumber
            var whereClause = new WhereClause { FieldName = primary, ComparisonOperator = Comparison.Equals, Value = entry[primary] };
            var statement = new WhereStatement(whereClause);

            if (SQLiteProvider.Provider.Single(name, statement) != null)
                SQLiteProvider.Provider.Delete(name, statement);
        }

        public static List<Dictionary<string, object>> Search(string name, string columnName, object value)
        {
            var column = dataFields.FirstOrDefault(field => field.ColumnName == columnName);
            if (column == null)
            {
                Log.Error("[StoreProvider.GetSingle] No primary field found");
                return null;
            }

            var where = new WhereStatement();
            where.Add(new WhereClause { FieldName = column.ColumnName, ComparisonOperator = Comparison.Equals, Value = value });

            return SQLiteProvider.Provider.Select(name, where).ToList();
        }
        
        public static IList<Dictionary<string, object>> GetCompleteStore(string name, params string[] column)
        {
            var columns = column == null || column.Length == 0 ? "*" : string.Join(", ", column);

            // create default table with no columns
            var dbTypes = new Dictionary<string, DbType>();
            CreateStore(name, dbTypes);

            return SQLiteProvider.Provider.Select(name, null, null, false, columns);
        }

        public static List<string> GetStoreNames()
        {
            return SQLiteProvider.Provider.GetStoreNames();
        }

        public static bool Exists(string name, Dictionary<string, object> entry)
        {
            var statement = new WhereStatement();
            foreach (var key in entry.Keys)
            {
                var whereClause = new WhereClause { FieldName = key, ComparisonOperator = Comparison.Equals, Value = entry[key] };

                statement.Add(whereClause);
            }

            if (statement.Count == 0)
                return false;

            if (!SQLiteProvider.Provider.GetStoreNames().Any(table => table == name))
                return false;

            return SQLiteProvider.Provider.Single(name, statement, "*") != null;
        }

        private static DbType GetDbType(Type type)
        {
            if (type.Name.Contains("Int"))
                return DbType.Int64;
            if (type == typeof(byte))
                return DbType.Byte;
            if (type == typeof(DateTime))
                return DbType.DateTime;

            return DbType.AnsiString;
        }

        private static string GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type).ToString();
            }
            return null;
        }
    }
}
