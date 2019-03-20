using System.Data;

namespace SQLiteAdapter.Models
{
    public class DataField
    {
        public string ColumnName { get; set; }
        public DbType DataType { get; set; }
        public bool IsPrimary { get; set; }
        public bool AutoIncrement { get; set; }
        public bool AllowDBNull { get; set; }
        public string DefaultValue { get; set; }
    }
}
