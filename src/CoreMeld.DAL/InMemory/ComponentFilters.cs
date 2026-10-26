using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

internal static class ComponentFilters
{
    public static IEnumerable<T> ApplyCommon<T>(
        this IEnumerable<T> source,
        decimal? minPrice,
        decimal? maxPrice,
        IEnumerable<string>? brands,
        string? nameContains,
        int? limit) where T : Component
    {
        var query = source;

        if (minPrice.HasValue)
            query = query.Where(c => c.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(c => c.Price <= maxPrice.Value);

        if (brands is not null)
        {
            var set = brands.ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (set.Count > 0)
                query = query.Where(c => set.Contains(c.Brand));
        }

        if (!string.IsNullOrWhiteSpace(nameContains))
            query = query.Where(c => c.Name.Contains(nameContains, StringComparison.OrdinalIgnoreCase));

        if (limit.HasValue)
            query = query.Take(limit.Value);

        return query.ToList();
    }
}