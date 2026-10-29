using System.Text.Json;

namespace CoreMeld.BLL.Services;

public static class LocalBuildStorage
{
    private static readonly string StorageFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "CoreMeld",
        "saved_build_ids.json");

    public static IReadOnlyList<int> GetSavedBuildIds()
    {
        try
        {
            if (!File.Exists(StorageFilePath))
                return Array.Empty<int>();

            var json = File.ReadAllText(StorageFilePath);
            var list = JsonSerializer.Deserialize<List<int>>(json);
            return list ?? new List<int>();
        }
        catch
        {
            return Array.Empty<int>();
        }
    }

    public static void AddSavedBuildId(int id)
    {
        try
        {
            var dir = Path.GetDirectoryName(StorageFilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var current = GetSavedBuildIds().ToList();
            if (!current.Contains(id))
            {
                current.Insert(0, id);
            }

            var json = JsonSerializer.Serialize(current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(StorageFilePath, json);
        }
        catch
        {
            // Non-critical if local file write fails
        }
    }
}

