using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class CpuCoolerRepository : ICpuCoolerRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               cc.fan_rpm, cc.noise_level_db, cc.height_mm, cc.tdp_rating_watts,
               COALESCE((SELECT socket_id FROM cpu_cooler_sockets ccs WHERE ccs.cooler_id = c.id LIMIT 1), 0) AS socket_id
        FROM components c
        JOIN cpu_coolers cc ON c.id = cc.component_id";

    public CpuCoolerRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<CpuCooler?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCpuCooler(reader);
        }

        return null;
    }

    public async Task<IEnumerable<CpuCooler>> GetByFilterAsync(
        int? socketId = null,
        int? minTdpRatingWatts = null,
        int? maxHeightMm = null,
        decimal? maxNoiseLevelDb = null,
        CoolingType? coolingType = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var coolers = new List<CpuCooler>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (socketId.HasValue)
        {
            sql.Append(" AND EXISTS (SELECT 1 FROM cpu_cooler_sockets ccs WHERE ccs.cooler_id = c.id AND ccs.socket_id = @socketId)");
            command.Parameters.AddWithValue("socketId", socketId.Value);
        }

        if (minTdpRatingWatts.HasValue)
        {
            sql.Append(" AND cc.tdp_rating_watts >= @minTdpRatingWatts");
            command.Parameters.AddWithValue("minTdpRatingWatts", minTdpRatingWatts.Value);
        }

        if (maxHeightMm.HasValue)
        {
            sql.Append(" AND cc.height_mm <= @maxHeightMm");
            command.Parameters.AddWithValue("maxHeightMm", maxHeightMm.Value);
        }

        if (maxNoiseLevelDb.HasValue)
        {
            sql.Append(" AND cc.noise_level_db <= @maxNoiseLevelDb");
            command.Parameters.AddWithValue("maxNoiseLevelDb", maxNoiseLevelDb.Value);
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            coolers.Add(MapCpuCooler(reader, socketId));
        }

        return coolers;
    }

    private static CpuCooler MapCpuCooler(NpgsqlDataReader reader, int? preferredSocketId = null)
    {
        var cooler = new CpuCooler();
        RepositoryQueryHelper.PopulateComponent(cooler, reader);
        cooler.FanRpm = reader.IsDBNull(7) ? null : reader.GetInt32(7);
        cooler.NoiseLevelDb = reader.IsDBNull(8) ? null : reader.GetDecimal(8);
        cooler.HeightMm = reader.GetInt32(9);
        cooler.TdpRatingWatts = reader.GetInt32(10);
        var dbSocketId = reader.IsDBNull(11) ? 0 : reader.GetInt32(11);
        cooler.SocketId = preferredSocketId ?? dbSocketId;
        return cooler;
    }
}

