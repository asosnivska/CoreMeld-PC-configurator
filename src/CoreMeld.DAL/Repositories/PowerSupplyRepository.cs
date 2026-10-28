using System.Text;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;
using Npgsql;

namespace CoreMeld.DAL.Repositories;

public class PowerSupplyRepository : IPowerSupplyRepository
{
    private readonly DbConnectionManager _dbManager;

    private const string BaseSelect = @"
        SELECT c.id, c.name, c.price, c.brand, c.color, c.image_url, c.category_id,
               ps.form_factor_id, ps.wattage, ps.efficiency_rating
        FROM components c
        JOIN power_supplies ps ON c.id = ps.component_id";

    public PowerSupplyRepository(DbConnectionManager dbManager)
    {
        _dbManager = dbManager;
    }

    public async Task<PowerSupply?> GetByIdAsync(int id)
    {
        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand($"{BaseSelect} WHERE c.id = @id", connection);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapPowerSupply(reader);
        }

        return null;
    }

    public async Task<IEnumerable<PowerSupply>> GetByFilterAsync(
        int? formFactorId = null,
        int? wattage = null,
        string? efficiencyRating = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        IEnumerable<string>? brands = null,
        string? nameContains = null,
        int? limit = null)
    {
        var powerSupplies = new List<PowerSupply>();

        await using var connection = _dbManager.CreateConnection();
        await connection.OpenAsync();

        var sql = new StringBuilder(BaseSelect);
        sql.Append(" WHERE 1=1");

        await using var command = new NpgsqlCommand();
        command.Connection = connection;

        if (formFactorId.HasValue)
        {
            sql.Append(" AND ps.form_factor_id = @formFactorId");
            command.Parameters.AddWithValue("formFactorId", formFactorId.Value);
        }

        if (wattage.HasValue)
        {
            sql.Append(" AND ps.wattage >= @wattage");
            command.Parameters.AddWithValue("wattage", wattage.Value);
        }

        if (!string.IsNullOrWhiteSpace(efficiencyRating))
        {
            sql.Append(" AND LOWER(ps.efficiency_rating) = LOWER(@efficiencyRating)");
            command.Parameters.AddWithValue("efficiencyRating", efficiencyRating.Trim());
        }

        RepositoryQueryHelper.ApplyCommonFilters(sql, command, minPrice, maxPrice, brands, nameContains, limit);

        command.CommandText = sql.ToString();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            powerSupplies.Add(MapPowerSupply(reader));
        }

        return powerSupplies;
    }

    private static PowerSupply MapPowerSupply(NpgsqlDataReader reader)
    {
        var ps = new PowerSupply();
        RepositoryQueryHelper.PopulateComponent(ps, reader);
        ps.FormFactorId = reader.GetInt32(7);
        ps.Wattage = reader.GetInt32(8);
        ps.EfficiencyRating = reader.GetString(9);
        return ps;
    }
}

