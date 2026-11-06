using System.Text.Json;
using CoreMeld.BLL.Settings;
using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Tests.Settings;

public class JsonAllocationSettingsProviderTests: IDisposable
{
    
    private static readonly string RealFilePath = Path.Combine(
        AppContext.BaseDirectory, 
        "Settings", 
        "allocation-weights.json"
        ); 
    
    private readonly List<string> _tempFiles = new();
    public void Dispose()
    {
        foreach(var path in _tempFiles)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    public string WriteRaw(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"allocation-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, content);
        _tempFiles.Add(path);
        return path;
    }


    public string  WriteSettings(Dictionary<Category, Dictionary<ComponentCategory, decimal>> settings)=>
        WriteRaw(JsonSerializer.Serialize(settings));
    
    
    private JsonAllocationSettingsProvider CreateProvider(
        Dictionary<Category, Dictionary<ComponentCategory, decimal>> settings)
        => new(WriteSettings(settings));


    private static Dictionary<Category, Dictionary<ComponentCategory, decimal>> BuildValid()
    {
        var settings = new Dictionary<Category, Dictionary<ComponentCategory, decimal>>();

        foreach (var category in Enum.GetValues<Category>())
        {
            var weights = new Dictionary<ComponentCategory, decimal>();

            foreach (var component in Enum.GetValues<ComponentCategory>())
            {
                if (component == ComponentCategory.Cpu)
                {
                    weights[component] = 40m;
                }
                else if (component == ComponentCategory.Gpu)
                {
                    weights[component] = 30m;
                }
                else if (component == ComponentCategory.Ram)
                {
                    weights[component] = 30m;
                }
                else
                {
                    weights[component] = 0m;
                }
            }

            settings[category] = weights;
        }

        return settings;
    }


    [Fact]
    public void GetWeights_ValidFile_ReturnsWrittenWeights()
    {
        var settings = BuildValid();

        settings[Category.Office][ComponentCategory.Cpu] = 60m;
        settings[Category.Office][ComponentCategory.Ram] = 40m;
        settings[Category.Office][ComponentCategory.Gpu] = 0m;

        var provider = CreateProvider(settings);

        var weightsOffice = provider.GetWeights(Category.Office);
        var weightsGamingFullHd = provider.GetWeights(Category.GamingFullHd);
        
        Assert.Equal(60m, weightsOffice[ComponentCategory.Cpu]);
        Assert.Equal(0m, weightsOffice[ComponentCategory.Gpu]);
        Assert.Equal(40m, weightsOffice[ComponentCategory.Ram]);
        Assert.Equal(40m, weightsGamingFullHd[ComponentCategory.Cpu]);
    }


    [Fact]
    public void GetWeights_ValidFile_ReturnsAllEightComponents()
    {
        var settings = BuildValid();
        var provider = CreateProvider(settings);

        var chekRendering = provider.GetWeights(Category.Rendering);
        var chekOffice = provider.GetWeights(Category.Office);

        
        Assert.Equal(8, chekRendering.Count);
        Assert.Equal(8, chekOffice.Count);
    }

    [Fact]
    public void GetWeights_Result_IsReadOnly()
    {
        var provider = CreateProvider(BuildValid());

        var weights = (IDictionary<ComponentCategory, decimal>)provider.GetWeights(Category.Office);

        Assert.Throws<NotSupportedException>(() => weights[ComponentCategory.Cpu] = 10m);
    }
    
    [Fact]
    public void RealFile_Loads_AndEveryProfileSumsTo100()
    {
        Assert.True(File.Exists(RealFilePath), $"Файл не скопійовано у вихідну папку: {RealFilePath}");

        var provider = new JsonAllocationSettingsProvider(RealFilePath);

        foreach (var category in Enum.GetValues<Category>())
        {
            Assert.Equal(100m, provider.GetWeights(category).Values.Sum());
        }
    }

    [Theory]
    [InlineData(Category.Office, ComponentCategory.Cpu, 35)]
    [InlineData(Category.Office, ComponentCategory.Gpu, 0)]
    [InlineData(Category.GamingFullHd, ComponentCategory.Gpu, 35)]
    [InlineData(Category.Gaming4K, ComponentCategory.Gpu, 45)]
    public void RealFile_KnownWeights_MatchTable(Category category, ComponentCategory component, decimal expected)
    {
        var provider = new JsonAllocationSettingsProvider(RealFilePath);

        Assert.Equal(expected, provider.GetWeights(category)[component]);
    }

    [Fact]
    public void Constructor_FileMissing_Throws()
    {
        var path = Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.json");

        Assert.Throws<InvalidOperationException>(() => new JsonAllocationSettingsProvider(path));
    }

    [Fact]
    public void Constructor_NotJson_Throws()
    {
        var path = WriteRaw("це не json");

        Assert.Throws<InvalidOperationException>(() => new JsonAllocationSettingsProvider(path));
    }

    [Fact]
    public void Constructor_EmptyFile_Throws()
    {
        var path = WriteRaw("");

        Assert.Throws<InvalidOperationException>(() => new JsonAllocationSettingsProvider(path));
    }

    [Fact]
    public void Constructor_JsonNull_Throws()
    {
        var path = WriteRaw("null");

        Assert.Throws<InvalidOperationException>(() => new JsonAllocationSettingsProvider(path));
    }

    [Fact]
    public void Constructor_UnknownProfileName_Throws()
    {
        var path = WriteRaw("""{ "NoSuchProfile": { "Cpu": 100 } }""");

        Assert.Throws<InvalidOperationException>(() => new JsonAllocationSettingsProvider(path));
    }

    [Fact]
    public void Constructor_MissingProfile_Throws()
    {
        var settings = BuildValid();
        settings.Remove(Category.Office);

        Assert.Throws<InvalidOperationException>(() => CreateProvider(settings));
    }

    [Fact]
    public void Constructor_MissingComponent_Throws()
    {
        var settings = BuildValid();
        
        settings[Category.Office].Remove(ComponentCategory.Case);

        Assert.Throws<InvalidOperationException>(() => CreateProvider(settings));
    }

    [Fact]
    public void Constructor_NegativeWeight_Throws()
    {
        var settings = BuildValid();
        settings[Category.Office][ComponentCategory.Cpu] = 45m;
        settings[Category.Office][ComponentCategory.Storage] = -5m;

        Assert.Throws<InvalidOperationException>(() => CreateProvider(settings));
    }

    [Theory]
    [InlineData(99)]
    [InlineData(101)]
    public void Constructor_SumIsNot100_Throws(decimal total)
    {
        var settings = BuildValid();
        settings[Category.Office][ComponentCategory.Ram] = total - 70m;

        Assert.Throws<InvalidOperationException>(() => CreateProvider(settings));
    }
}
