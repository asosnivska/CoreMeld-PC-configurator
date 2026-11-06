using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class RamRepository : IRamRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               r.memory_type, r.capacity_gb, r.module_count, r.frequency_mhz
        FROM components c
        JOIN ram_modules r ON c.id = r.component_id";

    public RamRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<Ram?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapRam(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Ram>> GetByFilterAsync(
        IEnumerable<RamMemoryType>? memoryType = null,
        int? minCapacityGb = null,
        int? maxModuleCount = null,
        int? minFrequencyMhz = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var rams = new List<Ram>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (memoryType != null)
        {
            var types = memoryType.Select(t => t.ToString()).ToArray();
            if (types.Length > 0)
            {
                sql.Append(" AND r.memory_type = ANY(@ramMemoryTypes)");
                command.Parameters.AddWithValue("ramMemoryTypes", types);
            }
        }

        if (minCapacityGb.HasValue)
        {
            sql.Append(" AND r.capacity_gb >= @minCapacityGb");
            command.Parameters.AddWithValue("minCapacityGb", minCapacityGb.Value);
        }

        if (maxModuleCount.HasValue)
        {
            sql.Append(" AND r.module_count <= @maxModuleCount");
            command.Parameters.AddWithValue("maxModuleCount", maxModuleCount.Value);
        }

        if (minFrequencyMhz.HasValue)
        {
            sql.Append(" AND r.frequency_mhz >= @minFrequencyMhz");
            command.Parameters.AddWithValue("minFrequencyMhz", minFrequencyMhz.Value);
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rams.Add(MapRam(reader));
        }

        return rams;
    }

    private static Ram MapRam(NpgsqlDataReader reader)
    {
        var ram = new Ram();
        RepositoryQueryHelper.PopulateComponent(ram, reader);
        ram.RamMemoryType = reader.GetString(7);
        ram.CapacityGb = reader.GetInt32(8);
        ram.ModuleCount = reader.GetInt32(9);
        ram.FrequencyMhz = reader.GetInt32(10);
        return ram;
    }
}

