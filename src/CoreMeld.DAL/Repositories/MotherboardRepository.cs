using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class MotherboardRepository : IMotherboardRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               mb.socket_id, mb.form_factor_id, mb.chipset, mb.memory_type, mb.memory_slots, mb.max_memory_gb, mb.has_wifi
        FROM components c
        JOIN motherboards mb ON c.id = mb.component_id";

    public MotherboardRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<Motherboard?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapMotherboard(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Motherboard>> GetByFilterAsync(
        int? socketId = null,
        string? chipset = null,
        int? formFactorId = null,
        IEnumerable<RamMemoryType>? memoryType = null,
        int? minMemorySlots = null,
        int? minSupportedMemoryGb = null,
        bool? hasWifi = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var motherboards = new List<Motherboard>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (socketId.HasValue)
        {
            sql.Append(" AND mb.socket_id = @socketId");
            command.Parameters.AddWithValue("socketId", socketId.Value);
        }

        if (!string.IsNullOrWhiteSpace(chipset))
        {
            sql.Append(" AND LOWER(mb.chipset) = LOWER(@chipset)");
            command.Parameters.AddWithValue("chipset", chipset.Trim());
        }

        if (formFactorId.HasValue)
        {
            sql.Append(" AND mb.form_factor_id = @formFactorId");
            command.Parameters.AddWithValue("formFactorId", formFactorId.Value);
        }

        if (memoryType != null)
        {
            var types = memoryType.Select(t => t.ToString()).ToArray();
            if (types.Length > 0)
            {
                sql.Append(" AND mb.memory_type = ANY(@mbMemoryTypes)");
                command.Parameters.AddWithValue("mbMemoryTypes", types);
            }
        }

        if (minMemorySlots.HasValue)
        {
            sql.Append(" AND mb.memory_slots >= @minMemorySlots");
            command.Parameters.AddWithValue("minMemorySlots", minMemorySlots.Value);
        }

        if (minSupportedMemoryGb.HasValue)
        {
            sql.Append(" AND mb.max_memory_gb >= @minSupportedMemoryGb");
            command.Parameters.AddWithValue("minSupportedMemoryGb", minSupportedMemoryGb.Value);
        }

        if (hasWifi.HasValue)
        {
            sql.Append(" AND mb.has_wifi = @hasWifi");
            command.Parameters.AddWithValue("hasWifi", hasWifi.Value);
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            motherboards.Add(MapMotherboard(reader));
        }

        return motherboards;
    }

    private static Motherboard MapMotherboard(NpgsqlDataReader reader)
    {
        var mb = new Motherboard
        {
            HasWifi = reader.GetBoolean(13)
        };
        RepositoryQueryHelper.PopulateComponent(mb, reader);
        mb.SocketId = reader.GetInt32(7);
        mb.FormFactorId = reader.GetInt32(8);
        mb.Chipset = reader.GetString(9);
        var memStr = reader.GetString(10);
        mb.MemoryType = Enum.TryParse<RamMemoryType>(memStr, true, out var mt) ? mt : RamMemoryType.DDR4;
        mb.MemorySlots = reader.GetInt32(11);
        mb.MaxMemoryGb = reader.GetInt32(12);
        return mb;
    }
}

