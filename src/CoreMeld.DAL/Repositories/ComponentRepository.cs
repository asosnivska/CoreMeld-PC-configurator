using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class ComponentRepository : IComponentRepository
{
    private readonly DbConnectionManager _dbManager;

    public ComponentRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<IEnumerable<Component>> GetAllAsync()
    {
        var components = new List<Component>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, image_url, category_id FROM components ORDER BY id ASC", connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var component = new Component();
            RepositoryQueryHelper.PopulateComponent(component, reader);
            components.Add(component);
        }

        return components;
    }

    public async Task<Component?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, image_url, category_id FROM components WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            var component = new Component();
            RepositoryQueryHelper.PopulateComponent(component, reader);
            return component;
        }

        return null;
    }

    public async Task<IEnumerable<Component>> GetByCategoryAsync(ComponentCategory category)
        => await GetByCategoryIdAsync((int)category);

    public async Task<IEnumerable<Component>> GetByCategoryIdAsync(int categoryId)
    {
        var components = new List<Component>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, image_url, category_id FROM components WHERE category_id = @categoryId ORDER BY id ASC", connection);
        command.Parameters.AddWithValue("categoryId", categoryId);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var component = new Component();
            RepositoryQueryHelper.PopulateComponent(component, reader);
            components.Add(component);
        }

        return components;
    }

    /// <summary>
    /// Filtered query for components (price range, manufacturers, socket, chipset, sorting by price asc/desc).
    /// All parameters are parameterized safely (Ticket #43).
    /// </summary>
    public async Task<IEnumerable<Component>> GetFilteredAsync(
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? manufacturers = null,
        int? socketId = null,
        string? chipset = null,
        bool? sortByPriceAsc = null,
        int? categoryId = null,
        int? limit = null)
    {
        var components = new List<Component>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(@"
            SELECT DISTINCT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id
            FROM components c
            LEFT JOIN cpus cpu ON c.id = cpu.component_id
            LEFT JOIN motherboards mb ON c.id = mb.component_id
            LEFT JOIN cpu_coolers cc ON c.id = cc.component_id
            LEFT JOIN cpu_cooler_sockets ccs ON cc.component_id = ccs.cooler_id
            WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (categoryId.HasValue)
        {
            sql.Append(" AND c.category_id = @categoryId");
            command.Parameters.AddWithValue("categoryId", categoryId.Value);
        }

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

        if (manufacturers != null)
        {
            var brandList = manufacturers.Where(b => !string.IsNullOrWhiteSpace(b))
                                         .Select(b => b.ToLowerInvariant())
                                         .ToArray();
            if (brandList.Length > 0)
            {
                sql.Append(" AND LOWER(c.brand) = ANY(@manufacturers)");
                command.Parameters.AddWithValue("manufacturers", brandList);
            }
        }

        if (socketId.HasValue)
        {
            sql.Append(" AND (cpu.socket_id = @socketId OR mb.socket_id = @socketId OR ccs.socket_id = @socketId)");
            command.Parameters.AddWithValue("socketId", socketId.Value);
        }

        if (!string.IsNullOrWhiteSpace(chipset))
        {
            sql.Append(" AND LOWER(mb.chipset) = LOWER(@chipset)");
            command.Parameters.AddWithValue("chipset", chipset.Trim());
        }

        if (sortByPriceAsc.HasValue)
        {
            sql.Append(sortByPriceAsc.Value ? " ORDER BY c.price ASC, c.id ASC" : " ORDER BY c.price DESC, c.id ASC");
        }
        else
        {
            sql.Append(" ORDER BY c.id ASC");
        }

        if (limit.HasValue)
        {
            sql.Append(" LIMIT @limit");
            command.Parameters.AddWithValue("limit", limit.Value);
        }

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var component = new Component();
            RepositoryQueryHelper.PopulateComponent(component, reader);
            components.Add(component);
        }

        return components;
    }
}
