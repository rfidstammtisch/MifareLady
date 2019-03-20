using SQLiteAdapter.Statements;
using SQLiteAdapter.Statements.Enums;
using SQLiteAdapter.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Data;
using System.Data.SQLite;
using SQLiteAdapter.SQLFactory;
using SQLiteAdapter.Configuration;

namespace SQLiteAdapter.SQLFactory
{
    public class SQLiteProvider
    {
        public static readonly string SQLiteDatabase = "Data.sqlite";
        private readonly GenericCommandBuilder commandBuilder;
        private readonly SQLiteHelper sqliteCommand;

        private string location;

        private static SQLiteProvider instance;
        public static SQLiteProvider Provider
        {
            get
            {
                return instance = instance ?? new SQLiteProvider(Path.Combine(SQLiteConfiguration.Configuration.DatabasePath, SQLiteConfiguration.Configuration.DatabaseName));
            }
        }

        public SQLiteProvider(string databaseName)
        {
            location = System.IO.Path.GetDirectoryName(databaseName);

            commandBuilder = new GenericCommandBuilder();
            sqliteCommand = new SQLiteHelper(databaseName);
        }

        public void CopyDatabase(string destinationPath)
        {
            using (var from = File.Open(sqliteCommand.DatabaseName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (File.Exists(destinationPath))
                    return;

                using (var to = File.OpenWrite(destinationPath))
                {
                    from.CopyTo(to);
                }
            }
        }

        public void DeleteDatabase(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public void CreateStore(string store, IList<DataField> fields)
        {
            sqliteCommand.RunNonQuery(() => commandBuilder.CreateCommand(sqliteCommand.Connection, store, fields));
        }

        public void Insert(string store, params Dictionary<string, object>[] records)
        {
            sqliteCommand.RunNonQuery(() => commandBuilder.InsertCommand(sqliteCommand.Connection, store, records));
        }

        public void Delete(string store, WhereStatement where)
        {
            sqliteCommand.RunNonQuery(() => commandBuilder.DeleteCommand(sqliteCommand.Connection, store, where));
        }

        public void Update(string store, WhereStatement where, Dictionary<string, object> record)
        {
            sqliteCommand.RunNonQuery(() => commandBuilder.UpdateCommand(sqliteCommand.Connection, store, record, where));
        }

        public void AddColumns(string store, IList<DataField> fields)
        {
            sqliteCommand.RunNonQuery(() => commandBuilder.AddColumnsCommand(sqliteCommand.Connection, store, fields));
        }

        public IList<Dictionary<string, object>> Select(string stores, WhereStatement where = null, OrderByClause order = null, bool distinct = false, params string[] fields)
        {
            return sqliteCommand.RunQuery(() => commandBuilder.SelectCommand(sqliteCommand.Connection, new[] { stores }, fields, where, null, null, null, distinct));
        }

        public Dictionary<string, object> Single(string store, WhereStatement where, params string[] fields)
        {
            var selectResult = Select(store, where, null, fields: fields);
            if (selectResult == null || selectResult.Count == 0) return null;

            return selectResult[0];
        }

        public List<string> GetStoreNames()
        {
            var list = new List<string>();

            // executes query that select names of all tables in master table of the database
            var query = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1";

            var dataTable = new DataTable();
            using (var connection = sqliteCommand.Connect())
            {
                connection.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, connection))
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    dataTable.Load(rdr);
                }
            }

            // Return all table names in the ArrayList
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(row.ItemArray[0].ToString());
            }

            return list;
        }

        public IList<DataField> GetStoreSchema(string store)
        {
            using (var connection = sqliteCommand.Connect())
            // query schema (with false query)
            using (var command = commandBuilder.SelectCommand(connection, new[] { store }, null, new WhereStatement(new WhereClause("1", Comparison.Equals, 0))))
            {
                // open SQLite DB
                connection.Open();

                var reader = command.ExecuteReader();
                var schema = reader.GetSchemaTable();
                if (schema == null) return null;

                // check schema TODO add type
                return (from DataRow row in schema.Rows
                        select new DataField
                        {
                            ColumnName = row.Field<string>("ColumnName"),
                            IsPrimary = row.Field<bool>("IsKey"),
                            AutoIncrement = row.Field<bool>("IsAutoIncrement"),
                            AllowDBNull = row.Field<bool>("AllowDBNull")
                        }).ToList();
            }
        }
    }
}
