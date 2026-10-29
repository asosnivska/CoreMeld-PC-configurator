using System.Diagnostics;
using CoreMeld.BLL.DTO;
using CoreMeld.DAL.InMemory;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class PCBuildService : IPCBuildService
{
    private readonly ICpuRepository _cpuRepository;
    private readonly IMotherboardRepository _motherboardRepository;
    private readonly IGpuRepository _gpuRepository;
    private readonly IRamRepository _ramRepository;
    private readonly IStorageRepository _storageRepository;
    private readonly ICpuCoolerRepository _cpuCoolerRepository;
    private readonly IPCCaseRepository _caseRepository;
    private readonly IPowerSupplyRepository _powerSupplyRepository;
    private readonly IBuildRepository _buildRepository;
    private readonly IComponentService _componentService;

    public PCBuildService(
        ICpuRepository cpuRepository,
        IMotherboardRepository motherboardRepository,
        IGpuRepository gpuRepository,
        IRamRepository ramRepository,
        IStorageRepository storageRepository,
        ICpuCoolerRepository cpuCoolerRepository,
        IPCCaseRepository caseRepository,
        IPowerSupplyRepository powerSupplyRepository,
        IBuildRepository buildRepository,
        IComponentService componentService)
    {
        _cpuRepository = cpuRepository;
        _motherboardRepository = motherboardRepository;
        _gpuRepository = gpuRepository;
        _ramRepository = ramRepository;
        _storageRepository = storageRepository;
        _cpuCoolerRepository = cpuCoolerRepository;
        _caseRepository = caseRepository;
        _powerSupplyRepository = powerSupplyRepository;
        _buildRepository = buildRepository;
        _componentService = componentService;
    }

    public async Task<PCBuildDto> GetDefaultBuildAsync()
    {
        try
        {
            return await AssembleBuildFromRepositoriesAsync(targetSocketId: 1); // AM4 default
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PCBuildService] Failed to load build from repositories: {ex.Message}. Using in-memory fallback.");
            return AssembleBuildFromInMemory(targetSocketId: 1);
        }
    }

    public async Task<IReadOnlyList<PCBuildDto>> GetPresetBuildsAsync()
    {
        var builds = new List<PCBuildDto>();

        try
        {
            builds.Add(await AssembleBuildFromRepositoriesAsync(targetSocketId: 1)); // AM4
        }
        catch
        {
            builds.Add(AssembleBuildFromInMemory(targetSocketId: 1));
        }

        try
        {
            builds.Add(await AssembleBuildFromRepositoriesAsync(targetSocketId: 2)); // AM5
        }
        catch
        {
            builds.Add(AssembleBuildFromInMemory(targetSocketId: 2));
        }

        try
        {
            builds.Add(await AssembleBuildFromRepositoriesAsync(targetSocketId: 3)); // LGA1700
        }
        catch
        {
            builds.Add(AssembleBuildFromInMemory(targetSocketId: 3));
        }

        return builds;
    }

    public PCBuildDto ReplaceComponent(PCBuildDto currentBuild, ComponentDto newComponent)
    {
        var updated = currentBuild.Components
            .Where(c => c.Category != newComponent.Category)
            .Append(newComponent)
            .OrderBy(c => c.Category)
            .ToList();

        decimal totalPrice = updated.Sum(c => c.Price);

        return currentBuild with
        {
            Components = updated,
            TotalPrice = totalPrice
        };
    }

    public async Task<int> SaveOrUpdateBuildAsync(PCBuildDto build, string name, int? existingBuildId)
    {
        var componentIds = build.Components.Select(c => c.Id).ToList();

        if (existingBuildId.HasValue)
        {
            // Update existing build in DB
            try
            {
                await _buildRepository.UpdateBuildAsync(existingBuildId.Value, name, build.TotalPrice, build.TotalTdp, componentIds);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PCBuildService] Failed to update build in DB: {ex.Message}. Using in-memory fallback.");
                var inMemRepo = new InMemoryBuildRepository();
                await inMemRepo.UpdateBuildAsync(existingBuildId.Value, name, build.TotalPrice, build.TotalTdp, componentIds);
            }

            return existingBuildId.Value;
        }
        else
        {
            // Create a brand new build
            int buildId;
            try
            {
                buildId = await _buildRepository.CreateBuildAsync(name, build.TotalPrice, build.TotalTdp, componentIds);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PCBuildService] Failed to save build to DB: {ex.Message}. Using in-memory fallback.");
                var inMemRepo = new InMemoryBuildRepository();
                buildId = await inMemRepo.CreateBuildAsync(name, build.TotalPrice, build.TotalTdp, componentIds);
            }

            // Store ID in local file
            LocalBuildStorage.AddSavedBuildId(buildId);
            return buildId;
        }
    }

    public async Task<IReadOnlyList<BuildSummary>> GetMySavedBuildsAsync()
    {
        var ids = LocalBuildStorage.GetSavedBuildIds();
        if (ids.Count == 0)
            return Array.Empty<BuildSummary>();

        try
        {
            var builds = await _buildRepository.GetBuildsByIdsAsync(ids);
            return builds.ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PCBuildService] DB error in GetMySavedBuildsAsync: {ex.Message}");
            var inMem = new InMemoryBuildRepository();
            return (await inMem.GetBuildsByIdsAsync(ids)).ToList();
        }
    }

    public async Task<IReadOnlyList<BuildSummary>> GetTopBuildsAsync()
    {
        try
        {
            var builds = (await _buildRepository.GetAllBuildsAsync(limit: 10)).ToList();
            if (builds.Count > 0)
                return builds;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PCBuildService] DB error in GetTopBuildsAsync: {ex.Message}");
        }

        var inMem = new InMemoryBuildRepository();
        return (await inMem.GetAllBuildsAsync()).ToList();
    }

    public async Task<PCBuildDto> LoadBuildByIdAsync(int buildId)
    {
        IEnumerable<int> componentIds;
        try
        {
            componentIds = await _buildRepository.GetComponentIdsForBuildAsync(buildId);
        }
        catch
        {
            var inMem = new InMemoryBuildRepository();
            componentIds = await inMem.GetComponentIdsForBuildAsync(buildId);
        }

        var ids = componentIds.ToList();
        if (ids.Count == 0)
        {
            return await GetDefaultBuildAsync();
        }

        var components = new List<ComponentDto>();
        foreach (ComponentCategory cat in Enum.GetValues<ComponentCategory>())
        {
            var categoryComponents = await _componentService.GetComponentsByCategoryAsync(cat);
            var matched = categoryComponents.FirstOrDefault(c => ids.Contains(c.Id));
            if (matched != null)
            {
                components.Add(matched);
            }
        }

        if (components.Count < 8)
        {
            var defaultBuild = await GetDefaultBuildAsync();
            foreach (var def in defaultBuild.Components)
            {
                if (!components.Any(c => c.Category == def.Category))
                    components.Add(def);
            }
        }

        var ordered = components.OrderBy(c => c.Category).ToList();
        return new PCBuildDto(
            TotalPrice: ordered.Sum(c => c.Price),
            TotalTdp: 250,
            IsCompatible: true,
            Components: ordered
        );
    }

    private async Task<PCBuildDto> AssembleBuildFromRepositoriesAsync(int targetSocketId)
    {
        var cpus = (await _cpuRepository.GetByFilterAsync(socketId: targetSocketId)).ToList();
        var cpu = cpus.FirstOrDefault() ?? (await _cpuRepository.GetByFilterAsync()).First();

        var mbs = (await _motherboardRepository.GetByFilterAsync(socketId: cpu.SocketId)).ToList();
        var mb = mbs.FirstOrDefault() ?? (await _motherboardRepository.GetByFilterAsync()).First();

        var rams = (await _ramRepository.GetByFilterAsync(memoryType: [mb.MemoryType])).ToList();
        var ram = rams.FirstOrDefault() ?? (await _ramRepository.GetByFilterAsync()).First();

        var gpus = (await _gpuRepository.GetByFilterAsync()).ToList();
        var gpu = gpus.FirstOrDefault() ?? throw new InvalidOperationException("Відеокарти відсутні.");

        var storages = (await _storageRepository.GetByFilterAsync()).ToList();
        var storage = storages.FirstOrDefault() ?? throw new InvalidOperationException("Накопичувачі відсутні.");

        var coolers = (await _cpuCoolerRepository.GetByFilterAsync(socketId: cpu.SocketId)).ToList();
        var cooler = coolers.FirstOrDefault() ?? (await _cpuCoolerRepository.GetByFilterAsync()).First();

        var cases = (await _caseRepository.GetByFilterAsync()).ToList();
        var pcCase = cases.FirstOrDefault() ?? throw new InvalidOperationException("Корпуси відсутні.");

        var psus = (await _powerSupplyRepository.GetByFilterAsync()).ToList();
        var psu = psus.FirstOrDefault() ?? throw new InvalidOperationException("Блоки живлення відсутні.");

        var components = new List<ComponentDto>
        {
            ComponentMapper.ToDto(cpu),
            ComponentMapper.ToDto(mb),
            ComponentMapper.ToDto(gpu),
            ComponentMapper.ToDto(ram),
            ComponentMapper.ToDto(storage),
            ComponentMapper.ToDto(cooler),
            ComponentMapper.ToDto(pcCase),
            ComponentMapper.ToDto(psu)
        };

        decimal totalPrice = components.Sum(c => c.Price);
        int totalTdp = cpu.TdpWatts + gpu.TdpWatts + 50;

        return new PCBuildDto(
            TotalPrice: totalPrice,
            TotalTdp: totalTdp,
            IsCompatible: true,
            Components: components
        );
    }

    private static PCBuildDto AssembleBuildFromInMemory(int targetSocketId)
    {
        var cpu = InMemoryCatalog.Cpus.FirstOrDefault(c => c.SocketId == targetSocketId) ?? InMemoryCatalog.Cpus.First();
        var mb = InMemoryCatalog.Motherboards.FirstOrDefault(m => m.SocketId == cpu.SocketId) ?? InMemoryCatalog.Motherboards.First();
        var ram = InMemoryCatalog.Rams.FirstOrDefault(r => r.RamMemoryType == mb.MemoryType.ToString()) ?? InMemoryCatalog.Rams.First();
        var gpu = InMemoryCatalog.Gpus.First();
        var storage = InMemoryCatalog.Storages.First();
        var cooler = InMemoryCatalog.CpuCoolers.FirstOrDefault(c => c.SocketId == cpu.SocketId) ?? InMemoryCatalog.CpuCoolers.First();
        var pcCase = InMemoryCatalog.Cases.First();
        var psu = InMemoryCatalog.PowerSupplies.First();

        var components = new List<ComponentDto>
        {
            ComponentMapper.ToDto(cpu),
            ComponentMapper.ToDto(mb),
            ComponentMapper.ToDto(gpu),
            ComponentMapper.ToDto(ram),
            ComponentMapper.ToDto(storage),
            ComponentMapper.ToDto(cooler),
            ComponentMapper.ToDto(pcCase),
            ComponentMapper.ToDto(psu)
        };

        decimal totalPrice = components.Sum(c => c.Price);
        int totalTdp = cpu.TdpWatts + gpu.TdpWatts + 50;

        return new PCBuildDto(
            TotalPrice: totalPrice,
            TotalTdp: totalTdp,
            IsCompatible: true,
            Components: components
        );
    }
}
