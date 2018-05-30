using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Nest;

namespace ElasticSearchExtensions
{
    /*
     * https://github.com/tmsmith/Dapper-Extensions/wiki/Predicates
     */
    public static class Predicates
    {
        /// <summary>
        /// Factory method that creates a new IFieldPredicate predicate: [FieldName] [Operator] [Value]. 
        /// Example: WHERE FirstName = 'Foo'
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="expression">An expression that returns the left operand [FieldName].</param>
        /// <param name="op">The comparison operator.</param>
        /// <param name="value">The value for the predicate.</param>
        /// <param name="not">Effectively inverts the comparison operator. Example: WHERE FirstName &lt;&gt; 'Foo'.</param>
        /// <returns>An instance of IFieldPredicate.</returns>
        public static IFieldPredicate Field<T>(Expression<Func<T, object>> expression, Operator op, object value,
            bool not = false) where T : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetPropertyInfo(expression);
            return new FieldPredicate<T>
            {
                PropertyInfo = propertyInfo,
                PropertyName = propertyInfo.Name,
                Operator = op,
                Value = value,
                Not = not
            };
        }

        /// <summary>
        /// Factory method that creates a new Sort which controls how the results will be sorted.
        /// </summary>
        public static SortField Sort<T>(Expression<Func<T, object>> expression,
            SortOrder sortOrder = SortOrder.Ascending) where T : class
        {
            PropertyInfo propertyInfo = ReflectionHelper.GetPropertyInfo(expression);

            return new SortField
            {
                Field = Infer.Field(propertyInfo),
                Order = sortOrder
            };
        }
    }

    public interface IPredicate
    {
        QueryContainer GetQuery();
    }

    public interface IFieldPredicate : IPredicate
    {
        PropertyInfo PropertyInfo { get; set; }
        string PropertyName { get; set; }

        Operator Operator { get; set; }

        bool Not { get; set; }

        object Value { get; set; }
    }

    public class FieldPredicate<T> : IFieldPredicate where T : class
    {
        public PropertyInfo PropertyInfo { get; set; }

        public string PropertyName { get; set; }

        public Operator Operator { get; set; }

        public bool Not { get; set; }

        public object Value { get; set; }

        public QueryContainer GetQuery()
        {
            QueryContainer query;
            switch (Operator)
            {
                case Operator.Eq:
                    query = new TermQuery
                    {
                        Field = Infer.Field(PropertyInfo),
                        Value = Value
                    };
                    break;
                case Operator.Like:
                    query = new MatchPhraseQuery
                    {
                        Field = Infer.Field(PropertyInfo),
                        Query = Value.ToString()
                    };
                    break;
                case Operator.Gt:
                    query = new TermRangeQuery
                    {
                        Field = Infer.Field(PropertyInfo),
                        GreaterThan = Value.ToString()
                    };
                    break;
                case Operator.Gte:
                    query = new TermRangeQuery
                    {
                        Field = Infer.Field(PropertyInfo),
                        GreaterThanOrEqualTo = Value.ToString()
                    };
                    break;
                case Operator.Lt:
                    query = new TermRangeQuery
                    {
                        Field = Infer.Field(PropertyInfo),
                        LessThan = Value.ToString()
                    };
                    break;
                case Operator.Lte:
                    query = new TermRangeQuery
                    {
                        Field = Infer.Field(PropertyInfo),
                        LessThanOrEqualTo = Value.ToString()
                    };
                    break;
                case Operator.In:
                    query = new TermsQuery
                    {
                        Field = PropertyName,
                        Terms = (List<object>) Value
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return query;
        }
    }

    public interface IPredicateGroup : IPredicate
    {
        GroupOperator Operator { get; set; }

        IList<IPredicate> Predicates { get; set; }
    }

    /// <summary>
    /// Groups IPredicates together using the specified group operator.
    /// </summary>
    public class PredicateGroup : IPredicateGroup
    {
        public GroupOperator Operator { get; set; }

        public IList<IPredicate> Predicates { get; set; } = new List<IPredicate>();

        public QueryContainer GetQuery()
        {
            var boolQuery = new BoolQuery();

            if (!Predicates.Any())
                return boolQuery;

            var mustList = new List<QueryContainer>();
            var mustNotList = new List<QueryContainer>();
            var shouldList = new List<QueryContainer>();

            foreach (var predicate in Predicates)
            {
                switch (Operator)
                {
                    case GroupOperator.And:
                        if (predicate is IFieldPredicate fieldPredicate && fieldPredicate.Not)
                            mustNotList.Add(fieldPredicate.GetQuery());
                        else
                            mustList.Add(predicate.GetQuery());
                        break;
                    case GroupOperator.Or:
                        shouldList.Add(predicate.GetQuery());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            boolQuery.Must = mustList;
            boolQuery.MustNot = mustNotList;
            boolQuery.Should = shouldList;

            return boolQuery;
        }
    }

    /// <summary>
    /// Comparison operator for predicates.
    /// </summary>
    public enum Operator
    {
        /// <summary>
        /// Equal to
        /// </summary>
        Eq,

        /// <summary>
        /// Greater than
        /// </summary>
        Gt,

        /// <summary>
        /// Greater than or equal to
        /// </summary>
        Gte,

        /// <summary>
        /// Less than
        /// </summary>
        Lt,

        /// <summary>
        /// Less than or equal to
        /// </summary>
        Lte,

        /// <summary>
        /// Like (You can use % in the value to do wilcard searching)
        /// </summary>
        Like,

        In
    }

    /// <summary>
    /// Operator to use when joining predicates in a PredicateGroup.
    /// </summary>
    public enum GroupOperator
    {
        And,
        Or
    }

    public static class PredicateExtensions
    {
        public static IPredicate GetPredicate(this List<IPredicate> list)
        {
            if (list == null || !list.Any())
                throw new ArgumentNullException(nameof(list));

            if (list.Count == 1)
                return list[0];

            return new PredicateGroup
            {
                Operator = GroupOperator.And,
                Predicates = list
            };
        }
    }
}