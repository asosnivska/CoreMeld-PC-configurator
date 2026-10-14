using CoreMeld.DAL.Interfaces;
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

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, category_id FROM components", connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            components.Add(new Component
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                Brand = reader.GetString(3),
                Color = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                ImageUrl = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                CategoryId = reader.GetInt32(6)
            });
        }

        return components;
    }

    public async Task<Component?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, category_id FROM components WHERE id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Component
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Price = reader.GetDecimal(2),
                Brand = reader.GetString(3),
                Color = reader.GetString(4),
                CategoryId = reader.GetInt32(5)
            };
        }

        return null;
    }
}
