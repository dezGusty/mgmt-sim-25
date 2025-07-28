using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Database.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string sortBy, bool sortDescending = false)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            var entityType = typeof(T);
            var property = entityType.GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return query;

            var parameter = Expression.Parameter(entityType, "x");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExpression = Expression.Lambda(propertyAccess, parameter);

            string methodName = sortDescending ? "OrderByDescending" : "OrderBy";

            var resultExpression = Expression.Call(
                typeof(Queryable),
                methodName,
                new Type[] { entityType, property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExpression));

            return query.Provider.CreateQuery<T>(resultExpression);
        }
    }
}
