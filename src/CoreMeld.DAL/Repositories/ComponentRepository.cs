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

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, image_url, category_id FROM components", connection);
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

        await using var command = new NpgsqlCommand("SELECT id, name, price, brand, color, image_url, category_id FROM components WHERE category_id = @categoryId", connection);
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
}
