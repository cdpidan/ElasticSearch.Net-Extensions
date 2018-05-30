using System;
using System.Collections.Generic;
using Nest;
using Shouldly;
using Xunit;

namespace ElasticSearchExtensions.Tests
{
    public class UnitTest1
    {
        #region Init

        private readonly IElasticClient _client;
        private readonly List<IPredicate> _list;
        private readonly IPredicateGroup _pg1;
        private readonly IPredicateGroup _pg2;
        private readonly IPredicateGroup _pg3;

        public UnitTest1()
        {
            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .BasicAuthentication("elastic", "elasticpw")
                .DefaultIndex("abp-audit-log");

            _client = new ElasticClient(settings);

            _list = new List<IPredicate>
            {
                Predicates.Field<AuditInfo>(x => x.UserId, Operator.Eq, 28),
                Predicates.Field<AuditInfo>(x => x.MethodName, Operator.Like, "GetAll"),
                Predicates.Field<AuditInfo>(x => x.UserId, Operator.Eq, 35, true)
            };

            _pg1 = new PredicateGroup {Operator = GroupOperator.And};
            _pg1.Predicates.Add(Predicates.Field<AuditInfo>(x => x.UserId, Operator.Eq, 28));
            _pg1.Predicates.Add(Predicates.Field<AuditInfo>(x => x.MethodName, Operator.Like, "GetAll"));

            _pg2 = new PredicateGroup {Operator = GroupOperator.Or};
            _pg2.Predicates.Add(Predicates.Field<AuditInfo>(x => x.UserId, Operator.Eq, 28));
            _pg2.Predicates.Add(Predicates.Field<AuditInfo>(x => x.MethodName, Operator.Like, "GetAll"));


            var pg4 = new PredicateGroup {Operator = GroupOperator.And};
            pg4.Predicates.Add(Predicates.Field<AuditInfo>(x => x.UserId, Operator.Eq, 35));
            pg4.Predicates.Add(Predicates.Field<AuditInfo>(x => x.MethodName, Operator.Like,
                "GetRealtyProjectForEdit"));

            _pg3 = new PredicateGroup {Operator = GroupOperator.Or};
            _pg3.Predicates.Add(_pg1);
            _pg3.Predicates.Add(pg4);
        }

        #endregion

        [Fact]
        public void Test1()
        {
            var query = _list.GetPredicate().GetQuery();

            var searchRequest = new SearchRequest<AuditInfo> {Size = 1000, Query = query};

            var result = _client.Search<AuditInfo>(searchRequest);

            result.ShouldNotBeNull();
            result.Documents.Count.ShouldBe(92);
        }

        [Fact]
        public void Test2()
        {
            var query = _pg1.GetQuery();

            query.ShouldNotBeNull();
        }

        [Fact]
        public void Test3()
        {
            var query = _pg2.GetQuery();

            var searchRequest = new SearchRequest<AuditInfo> {Size = 1000, Query = query};

            var result = _client.Search<AuditInfo>(searchRequest);

            result.ShouldNotBeNull();
            result.Documents.Count.ShouldBe(130);
        }

        [Fact]
        public void Test4()
        {
            var query = _pg3.GetQuery();

            var searchRequest = new SearchRequest<AuditInfo> {Size = 1000, Query = query};

            var result = _client.Search<AuditInfo>(searchRequest);

            result.ShouldNotBeNull();
            result.Documents.Count.ShouldBe(127);
        }
    }
}