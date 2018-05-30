using System;
using Elasticsearch.Net;
using Nest;

namespace ElasticSearchExtensions.Tests
{
    public abstract class TestBase
    {
        protected IElasticClient Client;

        protected TestBase()
        {
            Client = CreatElasticClient();
        }

        private IElasticClient CreatElasticClient()
        {
            var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            var settings = new ConnectionSettings(connectionPool, new InMemoryConnection()).DefaultIndex("es-test");
           return new ElasticClient(settings);
        }
    }
}