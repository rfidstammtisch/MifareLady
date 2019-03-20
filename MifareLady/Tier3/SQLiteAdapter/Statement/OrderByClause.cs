using SQLiteAdapter.Statements.Enums;

namespace SQLiteAdapter.Statements
{
    /// <summary>
    /// Represents a ORDER BY clause to be used with SELECT statements
    /// </summary>
    public class OrderByClause
    {
        public string FieldName;
        public Sorting SortOrder;
        public OrderByClause(string field)
        {
            FieldName = field;
            SortOrder = Sorting.Ascending;
        }
        public OrderByClause(string field, Sorting order)
        {
            FieldName = field;
            SortOrder = order;
        }
    }
}
