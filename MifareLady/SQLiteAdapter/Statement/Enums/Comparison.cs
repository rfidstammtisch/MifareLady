namespace SQLiteAdapter.Statements.Enums
{
    /// <summary>
    /// Represents comparison operators for WHERE, HAVING and JOIN clauses
    /// </summary>
    public enum Comparison
    {
        Begins,
        Ends,
        Equals,
        NotEquals,
        Like,
        NotLike,
        GreaterThan,
        GreaterOrEquals,
        LessThan,
        LessOrEquals,
        In
    }
}
