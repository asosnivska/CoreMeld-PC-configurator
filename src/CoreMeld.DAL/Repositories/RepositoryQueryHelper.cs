using System.Text;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

internal static class RepositoryQueryHelper
{
    public static void PopulateComponent(Component component, NpgsqlDataReader reader)
    {
        component.Id = reader.GetInt32(0);
        component.Name = reader.GetString(1);
        component.Price = reader.GetDecimal(2);
        component.Brand = reader.GetString(3);
        component.Color = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
        component.ImageUrl = reader.IsDBNull(5) ? string.Empty : reader.GetString(5);
        component.CategoryId = reader.GetInt32(6);
    }

    public static void ApplyCommonFilters(
        StringBuilder sql,
        NpgsqlCommand command,
        decimal? minPrice,
        decimal? maxPrice,
        IEnumerable<string>? brands,
        string? nameContains,
        int? limit)
    {
        if (minPrice.HasValue)
        {
            sql.Append(" AND c.price >= @minPrice");
            command.Parameters.AddWithValue("minPrice", minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            sql.Append(" AND c.price <= @maxPrice");
            command.Parameters.AddWithValue("maxPrice", maxPrice.Value);
        }

        if (brands != null)
        {
            var brandList = brands.Where(b => !string.IsNullOrWhiteSpace(b))
                                  .Select(b => b.ToLowerInvariant())
                                  .ToArray();
            if (brandList.Length > 0)
            {
                sql.Append(" AND LOWER(c.brand) = ANY(@brands)");
                command.Parameters.AddWithValue("brands", brandList);
            }
        }

        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            sql.Append(" AND c.name ILIKE @nameContains");
            command.Parameters.AddWithValue("nameContains", $"%{nameContains.Trim()}%");
        }

        if (limit.HasValue)
        {
            sql.Append(" LIMIT @limit");
            command.Parameters.AddWithValue("limit", limit.Value);
        }
    }
}

