using System;

namespace ElasticSearchExtensions.Exceptions
{
    public class ElasticsearchException : Exception
    {
        public ElasticsearchException(string message) : base(message)
        {
        }

        public ElasticsearchException(string message, Exception exception) : base(message, exception)
        {
        }
    }
}