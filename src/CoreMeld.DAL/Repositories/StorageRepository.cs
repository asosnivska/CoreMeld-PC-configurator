using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class StorageRepository : IStorageRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               s.drive_type, s.capacity_gb, s.interface
        FROM components c
        JOIN storage_drives s ON c.id = s.component_id";

    public StorageRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<Storage?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapStorage(reader);
        }

        return null;
    }

    public async Task<IEnumerable<Storage>> GetByFilterAsync(
        IEnumerable<StorageType>? driveType = null,
        int? capacityGb = null,
        IEnumerable<string>? interfaces = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var storages = new List<Storage>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (driveType != null)
        {
            var types = driveType.Select(t => t.ToString()).ToArray();
            if (types.Length > 0)
            {
                sql.Append(" AND s.drive_type = ANY(@driveTypes)");
                command.Parameters.AddWithValue("driveTypes", types);
            }
        }

        if (capacityGb.HasValue)
        {
            sql.Append(" AND s.capacity_gb >= @capacityGb");
            command.Parameters.AddWithValue("capacityGb", capacityGb.Value);
        }

        if (interfaces != null)
        {
            var ifaces = interfaces.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray();
            if (ifaces.Length > 0)
            {
                sql.Append(" AND s.interface = ANY(@interfaces)");
                command.Parameters.AddWithValue("interfaces", ifaces);
            }
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            storages.Add(MapStorage(reader));
        }

        return storages;
    }

    private static Storage MapStorage(NpgsqlDataReader reader)
    {
        var storage = new Storage();
        RepositoryQueryHelper.PopulateComponent(storage, reader);
        storage.DriveType = reader.GetString(7);
        storage.CapacityGb = reader.GetInt32(8);
        storage.InterfaceType = reader.GetString(9);
        return storage;
    }
}

