using Microsoft.EntityFrameworkCore;
using SinovadMediaServer.Transversal.Collection;
using System.Linq.Expressions;

namespace SinovadMediaServer.Transversal.Pagging
{
    public static class PaginExtension
    {
        public static async Task<DataCollection<T>> GetPagedAsync<T>(this IQueryable<T> query, int pageIndex, int pageSize, string sortBy, string sortDirection, string searchText=null, string searchBy=null, CancellationToken cancellationToken = default)
        {
            var searchQuery = query.SearchBy(searchText, searchBy);
            var result = new DataCollection<T>
            {
                Items = await searchQuery.OrderBy(sortBy, sortDirection).Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken),
                Total = await searchQuery.CountAsync(cancellationToken),
                Page = pageIndex
            };
            if (result.Total > 0)
            {
                result.Pages = Convert.ToInt32(Math.Ceiling(Convert.ToDecimal(result.Total) / pageSize));
            }
            return result;
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string sortBy, string sortDirection)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return source;
            }

            ParameterExpression parameter = Expression.Parameter(source.ElementType, "");

            MemberExpression property = Expression.Property(parameter, sortBy);
            LambdaExpression lambda = Expression.Lambda(property, parameter);

            string methodName = sortDirection == "asc" ? "OrderBy" : "OrderByDescending";

            Expression methodCallExpression = Expression.Call(typeof(Queryable), methodName,
                                  new Type[] { source.ElementType, property.Type },
                                  source.Expression, Expression.Quote(lambda));

            return source.Provider.CreateQuery<T>(methodCallExpression);
        }

        public static IQueryable<T> SearchBy<T>(this IQueryable<T> source, string searchText, string searchBy)
        {
            if (string.IsNullOrEmpty(searchText) || string.IsNullOrEmpty(searchBy))
            {
                return source;
            }
            ParameterExpression parameter = Expression.Parameter(source.ElementType, "");
            List<string> listColumnNames = new List<string>();
            if (!string.IsNullOrEmpty(searchBy))
            {
                listColumnNames = searchBy.Split("|").ToList();
            }
            Expression fullExpression = null;
            foreach (var columnName in listColumnNames)
            {
                try
                {
                    var propertyExp = Expression.Property(parameter, columnName);//Column name in the table
                    var someValue = Expression.Constant(searchText, typeof(string));//Value to compare
                    var ex = Expression.Call(propertyExp, "Contains", Type.EmptyTypes, someValue);
                    if (fullExpression == null)
                    {
                        fullExpression = ex;
                    }
                    else
                    {
                        fullExpression = Expression.Or(fullExpression, ex);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            if (fullExpression != null)
            {
                var lambda = Expression.Lambda<Func<T, bool>>(fullExpression, parameter);
                source = source.Where(lambda);
            }
            return source;
        }

    }
}
