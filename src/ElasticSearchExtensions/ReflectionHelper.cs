using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace ElasticSearchExtensions
{
    public static class ReflectionHelper
    {
        private static readonly List<Type> SimpleTypes = new List<Type>
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(bool),
            typeof(string),
            typeof(char),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(byte[])
        };

        public static MemberInfo GetProperty(LambdaExpression lambda)
        {
            Expression expr = lambda;
            for (;;)
            {
                switch (expr.NodeType)
                {
                    case ExpressionType.Lambda:
                        expr = ((LambdaExpression) expr).Body;
                        break;
                    case ExpressionType.Convert:
                        expr = ((UnaryExpression) expr).Operand;
                        break;
                    case ExpressionType.MemberAccess:
                        MemberExpression memberExpression = (MemberExpression) expr;
                        MemberInfo mi = memberExpression.Member;
                        return mi;
                    default:
                        return null;
                }
            }
        }

        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> expression) where T : class
        {
            PropertyInfo propertyInfo = GetProperty(expression) as PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentNullException(typeof(T).Name);

            return propertyInfo;
        }
    }
}