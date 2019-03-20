using SQLiteAdapter.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteAdapter.Configuration
{
    public class SQLiteConfiguration
    {
        private static SQLiteConfiguration configuration;
        public static SQLiteConfiguration Configuration { get => configuration = configuration ?? LoadFromJson(); }

        public string DatabasePath { get; set; }
        public string DatabaseName { get; set; }

        private static SQLiteConfiguration LoadFromJson()
        {
            var fileName = $"{typeof(SQLiteConfiguration).Name}.json";

            if (File.Exists(fileName))
                return File.ReadAllText(fileName).DeserializeJson<SQLiteConfiguration>();

            return new SQLiteConfiguration
            {
                DatabaseName = "data.sqlite",
                DatabasePath = ".",
            };
        }
    }
}
