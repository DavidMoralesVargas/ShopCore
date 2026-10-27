using Tienda_Online.Shared.DTOs;

namespace Tienda_Online.Backend.Helpers
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, PaginacionDTO pagination)
        {
            return queryable.Skip((pagination.Pagina - 1) * pagination.NumeroRegistros)
                            .Take(pagination.NumeroRegistros);
        }
    }
}
