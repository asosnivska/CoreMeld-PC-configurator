using System.Collections.ObjectModel;
using System.Text.Json;
using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Settings;

public class JsonAllocationSettingsProvider : IAllocationSettingsProvider
{
    private const decimal ExpectedTotal = 100m;

    private readonly IReadOnlyDictionary<Category, IReadOnlyDictionary<ComponentCategory, decimal>> _weights;

    public JsonAllocationSettingsProvider(string filePath)
    {
        string json;
        try
        {
            json = File.ReadAllText(filePath);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException($"Allocation settings file not found: {filePath}", ex);
        }

        Dictionary<Category, Dictionary<ComponentCategory, decimal>>? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<Dictionary<Category, Dictionary<ComponentCategory, decimal>>>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Allocation settings file is not valid: {filePath}", ex);
        }

        if (parsed is null)
        {
            throw new InvalidOperationException($"Allocation settings file is empty: {filePath}");
        }

        Validate(parsed);

        _weights = parsed.ToDictionary(
            pair => pair.Key,
            pair => (IReadOnlyDictionary<ComponentCategory, decimal>)
                new ReadOnlyDictionary<ComponentCategory, decimal>(pair.Value));
    }

    public IReadOnlyDictionary<ComponentCategory, decimal> GetWeights(Category category)
    {
        return _weights[category];
    }

    private static void Validate(Dictionary<Category, Dictionary<ComponentCategory, decimal>> parsed)
    {
        foreach (Category category in Enum.GetValues<Category>())
        {
            if (!parsed.TryGetValue(category, out var profile))
            {
                throw new InvalidOperationException($"Profile '{category}' is missing in allocation settings.");
            }

            foreach (ComponentCategory component in Enum.GetValues<ComponentCategory>())
            {
                if (!profile.TryGetValue(component, out decimal weight))
                {
                    throw new InvalidOperationException(
                        $"Profile '{category}': weight for '{component}' is missing.");
                }

                if (weight < 0)
                {
                    throw new InvalidOperationException(
                        $"Profile '{category}': weight for '{component}' is negative ({weight}).");
                }
            }

            decimal total = profile.Values.Sum();
            if (total != ExpectedTotal)
            {
                throw new InvalidOperationException(
                    $"Profile '{category}': weights sum to {total}, expected {ExpectedTotal}.");
            }
        }
    }
    
}