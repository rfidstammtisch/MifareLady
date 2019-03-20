using SQLiteAdapter.Statements.Enums;
using System.Collections.Generic;

namespace SQLiteAdapter.Statements
{
    /// <summary>
    /// Represents a WHERE clause on 1 database column, containing 1 or more comparisons on 
    /// that column, chained together by logic operators: eg (UserID=1 or UserID=2 or UserID>100)
    /// This can be achieved by doing this:
    /// WhereClause myWhereClause = new WhereClause("UserID", Comparison.Equals, 1);
    /// myWhereClause.AddClause(LogicOperator.Or, Comparison.Equals, 2);
    /// myWhereClause.AddClause(LogicOperator.Or, Comparison.GreaterThan, 100);
    /// </summary>
    public struct WhereClause
    {
        private string m_FieldName;
        private Comparison m_ComparisonOperator;
        private object m_Value;
        public List<SubClause> SubClauses;	// Array of SubClause

        /// <summary>
        /// Gets/sets the name of the database column this WHERE clause should operate on
        /// </summary>
        public string FieldName
        {
            get { return m_FieldName; }
            set { m_FieldName = value; }
        }

        /// <summary>
        /// Gets/sets the comparison method
        /// </summary>
        public Comparison ComparisonOperator
        {
            get { return m_ComparisonOperator; }
            set { m_ComparisonOperator = value; }
        }

        /// <summary>
        /// Gets/sets the value that was set for comparison
        /// </summary>
        public object Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public WhereClause(string field, Comparison firstCompareOperator, object firstCompareValue)
        {
            m_FieldName = field;
            m_ComparisonOperator = firstCompareOperator;
            m_Value = firstCompareValue;
            SubClauses = new List<SubClause>();
        }
        public void AddClause(LogicOperator logic, Comparison compareOperator, object compareValue)
        {
            SubClause newSubClause = new SubClause(logic, compareOperator, compareValue);
            SubClauses.Add(newSubClause);
        }


        public override string ToString()
        {
            return string.Format("{0} ({1}) {2}", FieldName, ComparisonOperator, Value);
        }
    }

    public struct SubClause
    {
        public LogicOperator LogicOperator;
        public Comparison ComparisonOperator;
        public object Value;
        public SubClause(LogicOperator logic, Comparison compareOperator, object compareValue)
        {
            LogicOperator = logic;
            ComparisonOperator = compareOperator;
            Value = compareValue;
        }
    }

}
