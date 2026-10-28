using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class PCCaseRepository : IPCCaseRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               cs.max_gpu_length_mm, cs.max_cpu_cooler_height_mm
        FROM components c
        JOIN cases cs ON c.id = cs.component_id";

    public PCCaseRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<PCCase?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPCCase(reader);
        }

        return null;
    }

    public async Task<IEnumerable<PCCase>> GetByFilterAsync(
        int? supportedMotherboardFormFactorId = null,
        int? minSupportedGpuLengthMm = null,
        int? minSupportedCoolerHeightMm = null,
        string? color = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var cases = new List<PCCase>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (minSupportedGpuLengthMm.HasValue)
        {
            sql.Append(" AND cs.max_gpu_length_mm >= @minSupportedGpuLengthMm");
            command.Parameters.AddWithValue("minSupportedGpuLengthMm", minSupportedGpuLengthMm.Value);
        }

        if (minSupportedCoolerHeightMm.HasValue)
        {
            sql.Append(" AND cs.max_cpu_cooler_height_mm >= @minSupportedCoolerHeightMm");
            command.Parameters.AddWithValue("minSupportedCoolerHeightMm", minSupportedCoolerHeightMm.Value);
        }

        if (!string.IsNullOrWhiteSpace(color))
        {
            sql.Append(" AND LOWER(c.color) = LOWER(@color)");
            command.Parameters.AddWithValue("color", color.Trim());
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cases.Add(MapPCCase(reader));
        }

        return cases;
    }

    private static PCCase MapPCCase(NpgsqlDataReader reader)
    {
        var pcCase = new PCCase();
        RepositoryQueryHelper.PopulateComponent(pcCase, reader);
        pcCase.MaxGpuLengthMm = reader.GetInt32(7);
        pcCase.MaxCpuCoolerHeightMm = reader.GetInt32(8);
        return pcCase;
    }
}

