using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Application.Extensions
{
    public static class QueryableAuditableExtensions
    {
        public static IQueryable<T> WhereActive<T>(this IQueryable<T> query)
            where T : AuditableBaseEntity
            => query.Where(e => e.IsActive);

        public static IQueryable<T> WhereInactive<T>(this IQueryable<T> query)
            where T : AuditableBaseEntity
            => query.Where(e => !e.IsActive);

        public static IQueryable<T> WhereIsActive<T>(this IQueryable<T> query, bool? isActive)
            where T : AuditableBaseEntity
            => isActive.HasValue ? query.Where(e => e.IsActive == isActive.Value) : query;

        public static IQueryable<T> WhereCreatedBetween<T>(this IQueryable<T> query, DateTime? from, DateTime? to)
            where T : AuditableBaseEntity
        {
            if (from.HasValue) query = query.Where(e => e.CreatedAt >= from.Value);
            if (to.HasValue) query = query.Where(e => e.CreatedAt <= to.Value);
            return query;
        }

        public static IQueryable<T> WhereCreatedBy<T>(this IQueryable<T> query, Guid createdBy)
            where T : AuditableBaseEntity
            => query.Where(e => e.CreatedBy == createdBy);
    }
}
