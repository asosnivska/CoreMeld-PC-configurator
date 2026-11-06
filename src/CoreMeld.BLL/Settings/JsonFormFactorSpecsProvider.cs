using System.Text.Json;

namespace CoreMeld.BLL.Settings;

public class JsonFormFactorSpecsProvider: IFormFactorSpecsProvider
{
    private readonly Dictionary<int, BoardSize> _sizes = new();

    private sealed class FormFactorEntry
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int LengthMm { get; set; }
        public int WidthMm { get; set; }
    }
    
    public JsonFormFactorSpecsProvider(string filePath)
    {
        string json;
        try
        {
            json = File.ReadAllText(filePath);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            throw new InvalidOperationException($"Form factor settings file not found: {filePath}", ex);
        }

        List<FormFactorEntry?>? entries;
        try
        {
            entries = JsonSerializer.Deserialize<List<FormFactorEntry?>>(json);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Form factor settings file is not valid: {filePath}", ex);
        }

        if (entries is null || entries.Count == 0)
        {
            throw new InvalidOperationException($"Form factor settings file is empty: {filePath}");
        }

        foreach (var entry in entries)
        {
            Validate(entry);

            if (!_sizes.TryAdd(entry!.Id, new BoardSize(entry.LengthMm, entry.WidthMm)))
            {
                throw new InvalidOperationException($"Form factor id {entry.Id} is duplicated in settings.");
            }
        }
    }
    
    
    public BoardSize GetBoardSize(int formFactorId)
    {
        if (!_sizes.TryGetValue(formFactorId, out var size))
        {
            throw new InvalidOperationException($"Unknown form factor id: {formFactorId}.");
        }

        return size;
    }
    
    private static void Validate(FormFactorEntry? entry)
    {
        if (entry is null)
        {
            throw new InvalidOperationException("Form factor settings contain an empty entry.");
        }

        if (entry.LengthMm <= 0)
        {
            throw new InvalidOperationException($"Form factor '{entry.Name}' (id {entry.Id}): length must be greater than 0.");
        }

        if (entry.WidthMm <= 0)
        {
            throw new InvalidOperationException($"Form factor '{entry.Name}' (id {entry.Id}): width must be greater than 0.");
        }

        if (entry.WidthMm > entry.LengthMm)
        {
            throw new InvalidOperationException($"Form factor '{entry.Name}' (id {entry.Id}): width must not exceed length.");
        }
    }
}