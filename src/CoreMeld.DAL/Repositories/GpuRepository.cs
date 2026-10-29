using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class GpuRepository : IGpuRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               g.vram_gb, g.memory_type, g.tdp_watts, g.length_mm, g.recommended_psu_watts
        FROM components c
        JOIN gpus g ON c.id = g.component_id";

    public GpuRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<Gpu?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapGpu(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Gpu>> GetByFilterAsync(
        int? minVramGb = null,
        int? maxRecommendedPsuWatts = null,
        IEnumerable<VideoMemoryType>? memoryType = null,
        int? maxTdpWatts = null,
        int? maxLengthMm = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var gpus = new List<Gpu>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (minVramGb.HasValue)
        {
            sql.Append(" AND g.vram_gb >= @minVramGb");
            command.Parameters.AddWithValue("minVramGb", minVramGb.Value);
        }

        if (maxRecommendedPsuWatts.HasValue)
        {
            sql.Append(" AND g.recommended_psu_watts <= @maxRecommendedPsuWatts");
            command.Parameters.AddWithValue("maxRecommendedPsuWatts", maxRecommendedPsuWatts.Value);
        }

        if (memoryType != null)
        {
            var types = memoryType.Select(t => t.ToString()).ToArray();
            if (types.Length > 0)
            {
                sql.Append(" AND g.memory_type = ANY(@gpuMemoryTypes)");
                command.Parameters.AddWithValue("gpuMemoryTypes", types);
            }
        }

        if (maxTdpWatts.HasValue)
        {
            sql.Append(" AND g.tdp_watts <= @maxTdpWatts");
            command.Parameters.AddWithValue("maxTdpWatts", maxTdpWatts.Value);
        }

        if (maxLengthMm.HasValue)
        {
            sql.Append(" AND g.length_mm <= @maxLengthMm");
            command.Parameters.AddWithValue("maxLengthMm", maxLengthMm.Value);
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            gpus.Add(MapGpu(reader));
        }

        return gpus;
    }

    private static Gpu MapGpu(NpgsqlDataReader reader)
    {
        var gpu = new Gpu();
        RepositoryQueryHelper.PopulateComponent(gpu, reader);
        gpu.VramGb = reader.GetInt32(7);
        var memStr = reader.GetString(8);
        gpu.MemoryType = Enum.TryParse<VideoMemoryType>(memStr, true, out var vt) ? vt : VideoMemoryType.GDDR6;
        gpu.TdpWatts = reader.GetInt32(9);
        gpu.LengthMm = reader.GetInt32(10);
        gpu.RecommendedPsuWatts = reader.GetInt32(11);
        return gpu;
    }
}

