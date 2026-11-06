using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class CpuRepository : ICpuRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               cpu.socket_id, cpu.cores, cpu.threads, cpu.base_clock, cpu.boost_clock, cpu.tdp_watts, cpu.has_integrated_gpu
        FROM components c
        JOIN cpus cpu ON c.id = cpu.component_id";

    public CpuRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<Cpu?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapCpu(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Cpu>> GetByFilterAsync(
        int? socketId = null,
        int? minCores = null,
        int? minThreads = null,
        bool? hasIntegratedGpu = null,
        int? maxTdpWatts = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var cpus = new List<Cpu>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (socketId.HasValue)
        {
            sql.Append(" AND cpu.socket_id = @socketId");
            command.Parameters.AddWithValue("socketId", socketId.Value);
        }

        if (minCores.HasValue)
        {
            sql.Append(" AND cpu.cores >= @minCores");
            command.Parameters.AddWithValue("minCores", minCores.Value);
        }

        if (minThreads.HasValue)
        {
            sql.Append(" AND cpu.threads >= @minThreads");
            command.Parameters.AddWithValue("minThreads", minThreads.Value);
        }

        if (hasIntegratedGpu.HasValue)
        {
            sql.Append(" AND cpu.has_integrated_gpu = @hasIntegratedGpu");
            command.Parameters.AddWithValue("hasIntegratedGpu", hasIntegratedGpu.Value);
        }

        if (maxTdpWatts.HasValue)
        {
            sql.Append(" AND cpu.tdp_watts <= @maxTdpWatts");
            command.Parameters.AddWithValue("maxTdpWatts", maxTdpWatts.Value);
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            cpus.Add(MapCpu(reader));
        }

        return cpus;
    }

    private static Cpu MapCpu(NpgsqlDataReader reader)
    {
        var cpu = new Cpu();
        RepositoryQueryHelper.PopulateComponent(cpu, reader);
        cpu.SocketId = reader.GetInt32(7);
        cpu.Cores = reader.GetInt32(8);
        cpu.Threads = reader.GetInt32(9);
        cpu.BaseClock = reader.GetDecimal(10);
        cpu.BoostClock = reader.GetDecimal(11);
        cpu.TdpWatts = reader.GetInt32(12);
        cpu.HasIntegratedGpu = reader.GetBoolean(13);
        return cpu;
    }
}

