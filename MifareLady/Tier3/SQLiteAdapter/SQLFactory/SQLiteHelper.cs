using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;

namespace SQLiteAdapter.SQLFactory
{
    public class SQLiteHelper
    {
        public delegate DbCommand DBCommand();
        private delegate T RunSql<out T>();

        public string DatabaseName { get; private set; }
        public DbConnection Connection { get; private set; }
        public DbCommand Command { get; private set; }

        public SQLiteHelper(string databaseName)
        {
            DatabaseName = databaseName;
                
            if(!File.Exists(databaseName))
                SQLiteConnection.CreateFile(databaseName);
        }

        public void RunNonQuery(DBCommand delegatedCommand)
        {
            Connect(() => RunResultNonQuery(delegatedCommand));
        }

        public List<Dictionary<string, object>> RunQuery(DBCommand delegatedCommand) 
        {
            return Connect(() => RunResultQuery(delegatedCommand));
        }

        public SQLiteConnection Connect()
        {
            // sort of hack to ensure \\\\ at start of network connection
            if (DatabaseName.StartsWith(@"\\") && !DatabaseName.StartsWith(@"\\\\"))
                DatabaseName = string.Format(@"\\{0}", DatabaseName);

            return new SQLiteConnection(string.Format("Data Source={0};", DatabaseName));
        }

        private T Connect<T>(RunSql<T> sql)
        {
            T result;

            using (Connection = Connect())
            {
                Connection.Open();

                result = sql();
            }

            return result;
        }

        private List<Dictionary<string, object>> RunResultQuery(DBCommand delegatedCommand) 
        {
            var result = new List<Dictionary<string, object>>();

            using (Command = delegatedCommand())
            using (var reader = Command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var entry = new Dictionary<string, object>();

                    for (var i = 0; i < reader.FieldCount; i++)
                        entry[reader.GetName(i)] = reader[reader.GetName(i)];

                    result.Add(entry);
                }
            }

            return result;
        }

        private bool? RunResultNonQuery(DBCommand delegatedCommand) 
        {
            //this could be the delegate
            using (Command = delegatedCommand())
            {
                Command.ExecuteNonQuery();
            }

            return null;
        }
    }
}
