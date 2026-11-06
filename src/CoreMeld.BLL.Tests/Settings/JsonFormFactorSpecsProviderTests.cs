using System.Text.Json;
using CoreMeld.BLL.Settings;

namespace CoreMeld.BLL.Tests.Settings;

public class JsonFormFactorSpecsProviderTests: IDisposable
{
    private static readonly string RealFilePath = Path.Combine(
        AppContext.BaseDirectory,
        "Settings",
        "form-factors.json");

    private readonly List<string> _tempFiles = new();

    public void Dispose()
    {
        foreach (var path in _tempFiles)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private string WriteRaw(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"form-factors-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }

    private static object Entry(int id, string name, int length, int width)
        => new { Id = id, Name = name, LengthMm = length, WidthMm = width };

    private static List<object> BuildValid() => new()
    {
        Entry(1, "ATX", 305, 244),
        Entry(2, "Micro-ATX", 244, 244),
        Entry(3, "Mini-ITX", 170, 170)
    };

    private string WriteSettings(List<object> entries)
        => WriteRaw(JsonSerializer.Serialize(entries));

    private JsonFormFactorSpecsProvider CreateProvider(List<object> entries)
        => new(WriteSettings(entries));

    [Fact]
    public void GetBoardSize_ValidFile_ReturnsWrittenSize()
    {
        var provider = CreateProvider(BuildValid());

        var size = provider.GetBoardSize(2);

        Assert.Equal(new BoardSize(244, 244), size);
    }

    [Theory]
    [InlineData(1, 305, 244)]
    [InlineData(2, 244, 244)]
    [InlineData(3, 170, 170)]
    public void RealFile_Loads_AndHasKnownSizes(int id, int length, int width)
    {
        Assert.True(File.Exists(RealFilePath), $"Файл не скопійовано у вихідну папку: {RealFilePath}");

        var provider = new JsonFormFactorSpecsProvider(RealFilePath);

        Assert.Equal(new BoardSize(length, width), provider.GetBoardSize(id));
    }

    [Fact]
    public void Constructor_FileMissing_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        Assert.Throws<InvalidOperationException>(() => new JsonFormFactorSpecsProvider(path));
    }

    [Fact]
    public void Constructor_NotJson_Throws()
    {
        var path = WriteRaw("це не json");

        Assert.Throws<InvalidOperationException>(() => new JsonFormFactorSpecsProvider(path));
    }

    [Fact]
    public void Constructor_EmptyFile_Throws()
    {
        var path = WriteRaw("");

        Assert.Throws<InvalidOperationException>(() => new JsonFormFactorSpecsProvider(path));
    }

    [Fact]
    public void Constructor_JsonNull_Throws()
    {
        var path = WriteRaw("null");

        Assert.Throws<InvalidOperationException>(() => new JsonFormFactorSpecsProvider(path));
    }

    [Fact]
    public void Constructor_EmptyArray_Throws()
    {
        var path = WriteRaw("[]");

        Assert.Throws<InvalidOperationException>(() => new JsonFormFactorSpecsProvider(path));
    }

    [Fact]
    public void Constructor_NullEntry_Throws()
    {
        var path = WriteRaw("[null]");

        Assert.Throws<InvalidOperationException>(() => new JsonFormFactorSpecsProvider(path));
    }

    [Fact]
    public void Constructor_DuplicateId_Throws()
    {
        var entries = new List<object>
        {
            Entry(1, "ATX", 305, 244),
            Entry(1, "Other", 244, 244)
        };

        Assert.Throws<InvalidOperationException>(() => CreateProvider(entries));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveLength_Throws(int length)
    {
        var entries = new List<object> { Entry(1, "ATX", length, length) };

        Assert.Throws<InvalidOperationException>(() => CreateProvider(entries));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveWidth_Throws(int width)
    {
        var entries = new List<object> { Entry(1, "ATX", 305, width) };

        Assert.Throws<InvalidOperationException>(() => CreateProvider(entries));
    }

    [Fact]
    public void Constructor_WidthGreaterThanLength_Throws()
    {
        var entries = new List<object> { Entry(1, "ATX", 170, 244) };

        Assert.Throws<InvalidOperationException>(() => CreateProvider(entries));
    }

    [Fact]
    public void GetBoardSize_UnknownId_Throws()
    {
        var provider = CreateProvider(BuildValid());

        Assert.Throws<InvalidOperationException>(() => provider.GetBoardSize(99));
    }
}