using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class BuildRepository : IBuildRepository
{
    private readonly DbConnectionManager _dbManager;

    public BuildRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<int> CreateBuildAsync(string name, decimal totalPrice, int totalTdp, IEnumerable<int> componentIds)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            await using var cmd = new NpgsqlCommand(
                "INSERT INTO builds (name, total_price, total_tdp_watts) VALUES (@name, @price, @tdp) RETURNING id;",
                connection,
                transaction);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("price", totalPrice);
            cmd.Parameters.AddWithValue("tdp", totalTdp);

            var buildId = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            foreach (var cid in componentIds)
            {
                await using var itemCmd = new NpgsqlCommand(
                    "INSERT INTO build_components (build_id, component_id, quantity) VALUES (@buildId, @componentId, 1) ON CONFLICT DO NOTHING;",
                    connection,
                    transaction);
                itemCmd.Parameters.AddWithValue("buildId", buildId);
                itemCmd.Parameters.AddWithValue("componentId", cid);
                await itemCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return buildId;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateBuildAsync(int buildId, string name, decimal totalPrice, int totalTdp, IEnumerable<int> componentIds)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            await using var cmd = new NpgsqlCommand(
                "UPDATE builds SET name = @name, total_price = @price, total_tdp_watts = @tdp WHERE id = @id;",
                connection,
                transaction);
            cmd.Parameters.AddWithValue("id", buildId);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("price", totalPrice);
            cmd.Parameters.AddWithValue("tdp", totalTdp);
            await cmd.ExecuteNonQueryAsync();

            await using var delCmd = new NpgsqlCommand(
                "DELETE FROM build_components WHERE build_id = @buildId;",
                connection,
                transaction);
            delCmd.Parameters.AddWithValue("buildId", buildId);
            await delCmd.ExecuteNonQueryAsync();

            foreach (var cid in componentIds)
            {
                await using var itemCmd = new NpgsqlCommand(
                    "INSERT INTO build_components (build_id, component_id, quantity) VALUES (@buildId, @componentId, 1) ON CONFLICT DO NOTHING;",
                    connection,
                    transaction);
                itemCmd.Parameters.AddWithValue("buildId", buildId);
                itemCmd.Parameters.AddWithValue("componentId", cid);
                await itemCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<BuildSummary>> GetAllBuildsAsync(int? limit = null)
    {
        var list = new List<BuildSummary>();
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, name, total_price, total_tdp_watts, created_at FROM builds ORDER BY id DESC";
        if (limit.HasValue)
            sql += " LIMIT @limit";

        await using var cmd = new NpgsqlCommand(sql, connection);
        if (limit.HasValue)
            cmd.Parameters.AddWithValue("limit", limit.Value);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapBuildSummary(reader));
        }

        return list;
    }

    public async Task<IEnumerable<BuildSummary>> GetBuildsByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
            return Enumerable.Empty<BuildSummary>();

        var list = new List<BuildSummary>();
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT id, name, total_price, total_tdp_watts, created_at FROM builds WHERE id = ANY(@ids) ORDER BY id DESC";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("ids", idList.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(MapBuildSummary(reader));
        }

        return list;
    }

    public async Task<IEnumerable<int>> GetComponentIdsForBuildAsync(int buildId)
    {
        var list = new List<int>();
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = "SELECT component_id FROM build_components WHERE build_id = @buildId";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("buildId", buildId);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(reader.GetInt32(0));
        }

        return list;
    }

    private static BuildSummary MapBuildSummary(NpgsqlDataReader reader)
    {
        return new BuildSummary
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            TotalPrice = reader.GetDecimal(2),
            TotalTdpWatts = reader.GetInt32(3),
            CreatedAt = reader.GetDateTime(4)
        };
    }
}
