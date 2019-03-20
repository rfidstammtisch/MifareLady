using SQLiteAdapter.Statements.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SQLiteAdapter.Statements
{
    public class WhereStatement : List<List<WhereClause>>
    {
        // The list in this container will contain lists of clauses, and 
        // forms a where statement alltogether!

        public WhereStatement()
        { }

        public WhereStatement(WhereClause clause) : this()
        {
            Add(clause);
        }


        public int ClauseLevels
        {
            get { return Count; }
        }

        private void AssertLevelExistance(int level)
        {
            if (Count < (level - 1))
            {
                throw new Exception("Level " + level + " not allowed because level " + (level - 1) + " does not exist.");
            }
            // Check if new level must be created
            if (Count < level)
            {
                Add(new List<WhereClause>());
            }
        }

        public WhereStatement(string primaryKey, object[] ids)
        {
            for (int i = 0; i < ids.Length; i++)
            {
                // combine all statements with OR 
                // by every id in ids for Value
                // and primaryKey as FieldName
                var idWhere = new WhereClause { FieldName = primaryKey, ComparisonOperator = Comparison.Equals, Value = ids[i], SubClauses = new List<SubClause>() };
                Add(idWhere, i + 1);
            }
        }

        public WhereStatement(List<Dictionary<string, object>> list, string primaryKey)
        {
            // where[] primaryKey Equals object.primaryKey
            var whereClauses = list.Select(empl => new WhereClause { FieldName = primaryKey, ComparisonOperator = Comparison.Equals, Value = empl[primaryKey] }).ToList();
            
            // Combine statement with OR
            for (int i = 0; i < whereClauses.Count; i++)
                Add(whereClauses[i], i + 1);
        }

        public void Add(WhereClause clause) { Add(clause, 1); }
        public void Add(WhereClause clause, int level)
        {
            AddWhereClauseToLevel(clause, level);
        }
        public WhereClause Add(string field, Comparison @operator, object compareValue) { return Add(field, @operator, compareValue, 1); }
        public WhereClause Add(Enum field, Comparison @operator, object compareValue) { return Add(field.ToString(), @operator, compareValue, 1); }
        public WhereClause Add(string field, Comparison @operator, object compareValue, int level)
        {
            var newWhereClause = new WhereClause(field, @operator, compareValue);
            AddWhereClauseToLevel(newWhereClause, level);
            return newWhereClause;
        }

        private void AddWhereClauseToLevel(WhereClause clause, int level)
        {
            // Add the new clause to the array at the right level
            AssertLevelExistance(level);
            this[level - 1].Add(clause);
        }

        /// <summary>
        /// This static method combines 2 where statements with eachother to form a new statement
        /// </summary>
        /// <param name="statement1"></param>
        /// <param name="statement2"></param>
        /// <returns></returns>
        public static WhereStatement CombineStatements(WhereStatement statement1, WhereStatement statement2)
        {
            // statement1: {Level1}((Age<15 OR Age>=20) AND (strEmail LIKE 'e%') OR {Level2}(Age BETWEEN 15 AND 20))
            // Statement2: {Level1}((Name = 'Peter'))
            // Return statement: {Level1}((Age<15 or Age>=20) AND (strEmail like 'e%') AND (Name = 'Peter'))

            // Make a copy of statement1
            WhereStatement result = Copy(statement1);

            // Add all clauses of statement2 to result
            for (int i = 0; i < statement2.ClauseLevels; i++) // for each clause level in statement2
            {
                List<WhereClause> level = statement2[i];
                foreach (WhereClause clause in level) // for each clause in level i
                {
                    for (int j = 0; j < result.ClauseLevels; j++)  // for each level in result, add the clause
                    {
                        result.AddWhereClauseToLevel(clause, j);
                    }
                }
            }

            return result;
        }

        public static WhereStatement Copy(WhereStatement statement)
        {
            var result = new WhereStatement();
            int currentLevel = 0;
            foreach (var level in statement)
            {
                currentLevel++;
                result.Add(new List<WhereClause>());
                foreach (WhereClause clause in statement[currentLevel - 1])
                {
                    var clauseCopy = new WhereClause(clause.FieldName, clause.ComparisonOperator, clause.Value);
                    foreach (SubClause subClause in clause.SubClauses)
                    {
                        var subClauseCopy = new SubClause(subClause.LogicOperator, subClause.ComparisonOperator, subClause.Value);
                        clauseCopy.SubClauses.Add(subClauseCopy);
                    }
                    result[currentLevel - 1].Add(clauseCopy);
                }
            }
            return result;
        }
    }

}
