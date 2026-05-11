using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using InclusiON.DTOs.Common;

namespace InclusiON.Infrastructure.Extensions
{
    public static class QueryablePagedExtensions
    {
        public static async Task<PagedResponse<T>> ToPagedAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var totalRecords = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }

        public static async Task<PagedResponse<T>> ToPagedAsync<T>(
            this IQueryable<T> query,
            int page,
            int pageSize,
            SortField? sortBy,
            string sortDirection,
            Dictionary<SortField, Expression<Func<T, object>>> sortMappings,
            CancellationToken cancellationToken = default)
        {
            var totalRecords = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            var isAscending = string.Equals(sortDirection, "ASC", StringComparison.OrdinalIgnoreCase);

            var sortExpression = sortBy.HasValue && sortMappings.TryGetValue(sortBy.Value, out var mapping)
                ? mapping
                : sortMappings[SortField.Id];

            var ordered = isAscending
                ? query.OrderBy(sortExpression)
                : query.OrderByDescending(sortExpression);

            var data = await ordered
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return new PagedResponse<T>
            {
                Data = data,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1
            };
        }
    }
}
