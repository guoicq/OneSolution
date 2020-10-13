using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Azure.Cosmos.Table;

namespace OneSolution.Storage.Table
{
    public static class TableQueryHelper
    {
        public static string GenerateFilterStartWith(string field, string startsWithPattern)
        {
            var length = startsWithPattern.Length - 1;
            var lastChar = startsWithPattern[length];

            var nextLastChar = (char)(lastChar + 1);

            var startsWithEndPattern = startsWithPattern.Substring(0, length) + nextLastChar;

            var prefixCondition = GenerateFilterBetween(field, startsWithEndPattern, startsWithEndPattern);

            return prefixCondition;
        }

        public static string GenerateFilterBetween(string field, string beginValue, string endValue)
        {
            if (endValue == null && beginValue == null)
                throw new ArgumentNullException();

            if (beginValue == null)
                return TableQuery.GenerateFilterCondition(field,
                    QueryComparisons.LessThan,
                    endValue);

            if (endValue == null)
                return TableQuery.GenerateFilterCondition(field,
                    QueryComparisons.GreaterThanOrEqual,
                    beginValue);

            if (string.Compare(beginValue, endValue, StringComparison.InvariantCulture) > 0)
            {
                var v = beginValue;
                beginValue = endValue;
                endValue = v;
            }
            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterCondition(field,
                    QueryComparisons.GreaterThanOrEqual,
                    beginValue),
                TableOperators.And,
                TableQuery.GenerateFilterCondition(field,
                    QueryComparisons.LessThan,
                    endValue)
                );

            return prefixCondition;
        }
        public static string GenerateFilterBetween(string field, DateTime beginValue, DateTime endValue)
        {
            if (beginValue > endValue)
            {
                var v = beginValue;
                beginValue = endValue;
                endValue = v;
            }
            var prefixCondition = TableQuery.CombineFilters(
                TableQuery.GenerateFilterConditionForDate(field,
                    QueryComparisons.GreaterThanOrEqual,
                    beginValue),
                TableOperators.And,
                TableQuery.GenerateFilterConditionForDate(field,
                    QueryComparisons.LessThan,
                    endValue)
                );

            return prefixCondition;
        }
    }

}
