using System;
using Nest;
using Shouldly;
using Xunit;

namespace ElasticSearchExtensions.Tests
{
    public class FieldPredicateTests : TestBase
    {
        [Theory]
        [InlineData(Operator.Gt)]
        [InlineData(Operator.Gte)]
        [InlineData(Operator.Lt)]
        [InlineData(Operator.Lte)]
        public void GetQuery_DateTime_Gt_Test(Operator @operator)
        {
            var predicate = Predicates.Field<AuditInfo>(x => x.ExecutionTime, @operator, new DateTime(2018, 5, 1));

            var result = GetSearchResponse<AuditInfo>(predicate);

            result.ShouldNotBeNull();
            result.IsValid.ShouldBeTrue();
        }

        [Theory]
        [InlineData(Operator.Gt)]
        [InlineData(Operator.Gte)]
        [InlineData(Operator.Lt)]
        [InlineData(Operator.Lte)]
        public void GetQuery_Number_Gt_Test(Operator @operator)
        {
            var predicate = Predicates.Field<AuditInfo>(x => x.ExecutionDuration, @operator, 50);

            var result = GetSearchResponse<AuditInfo>(predicate);

            result.ShouldNotBeNull();
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void GetQuery_String_Eq_Test()
        {
            var predicate = Predicates.Field<AuditInfo>(x => x.MethodName, Operator.Eq, "hello");

            var result = GetSearchResponse<AuditInfo>(predicate);

            result.ShouldNotBeNull();
            result.IsValid.ShouldBeTrue();
        }

        [Fact]
        public void GetQuery_String_Like_Test()
        {
            var predicate = Predicates.Field<AuditInfo>(x => x.MethodName, Operator.Like, "hello");

            var result = GetSearchResponse<AuditInfo>(predicate);

            result.ShouldNotBeNull();
            result.IsValid.ShouldBeTrue();
        }

        private ISearchResponse<T> GetSearchResponse<T>(IPredicate predicate) where T : class
        {
            var query = predicate.GetQuery();

            var searchRequest = new SearchRequest<T> {Size = 1000, Query = query};
            
            return Client.Search<T>(searchRequest);
        }
    }
}