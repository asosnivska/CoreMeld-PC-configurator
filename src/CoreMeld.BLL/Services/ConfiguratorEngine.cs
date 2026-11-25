using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Exceptions;
using CoreMeld.Domain.Enums;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class ConfiguratorEngine:IConfiguratorEngine
{
    private readonly IBudgetCalculator _budgetCalculator;
    private readonly ICompatibilityChecker _compatibilityChecker;
    private readonly ITdpCalculator _tdpCalculator;
    private readonly ISpaceControlService _spaceControlService;
    private readonly ICpuRepository _cpuRepository;
    private readonly IMotherboardRepository _motherboardRepository;
    private readonly IGpuRepository _gpuRepository;
    private readonly IRamRepository _ramRepository;
    private readonly IStorageRepository _storageRepository;
    private readonly ICpuCoolerRepository _coolerRepository;
    private readonly IPCCaseRepository _caseRepository;
    private readonly IPowerSupplyRepository _powerSupplyRepository;

    public ConfiguratorEngine(IBudgetCalculator budgetCalculator, ICompatibilityChecker compatibilityChecker,
        ITdpCalculator tdpCalculator, ISpaceControlService spaceControlService, ICpuRepository cpuRepository,
        IMotherboardRepository motherboardRepository, IGpuRepository gpuRepository, IRamRepository ramRepository,
        IStorageRepository storageRepository, ICpuCoolerRepository cpuCoolerRepository,
        IPCCaseRepository pcCaseRepository,
        IPowerSupplyRepository powerSupplyRepository)
    {
        ArgumentNullException.ThrowIfNull(budgetCalculator);
        ArgumentNullException.ThrowIfNull(compatibilityChecker);
        ArgumentNullException.ThrowIfNull(tdpCalculator);
        ArgumentNullException.ThrowIfNull(spaceControlService);
        ArgumentNullException.ThrowIfNull(cpuRepository);
        ArgumentNullException.ThrowIfNull(motherboardRepository);
        ArgumentNullException.ThrowIfNull(gpuRepository);
        ArgumentNullException.ThrowIfNull(ramRepository);
        ArgumentNullException.ThrowIfNull(storageRepository);
        ArgumentNullException.ThrowIfNull(cpuCoolerRepository);
        ArgumentNullException.ThrowIfNull(pcCaseRepository);
        ArgumentNullException.ThrowIfNull(powerSupplyRepository);


        _budgetCalculator = budgetCalculator;
        _caseRepository = pcCaseRepository;
        _compatibilityChecker = compatibilityChecker;
        _coolerRepository = cpuCoolerRepository;
        _cpuRepository = cpuRepository;
        _gpuRepository = gpuRepository;
        _tdpCalculator = tdpCalculator;
        _motherboardRepository = motherboardRepository;
        _powerSupplyRepository = powerSupplyRepository;
        _ramRepository = ramRepository;
        _spaceControlService = spaceControlService;
        _storageRepository = storageRepository;
    }

    public async Task<PCBuildDto> GenerateBuildAsync(BuildRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Budget <= 0)
        {
            throw new ArgumentException("Бюджет має бути більшим за 0.", nameof(request));
        }

        var categories = ResolveCategories(request);

        var limits = _budgetCalculator.CalculateAllocation(request.Budget, categories);
        var needGpu = limits[ComponentCategory.Gpu] > 0m;

        var cache = new QueryCache();
        var attempts = new IReadOnlyDictionary<ComponentCategory, decimal>?[] { limits, null };

        decimal? cheapestTotal = null;

        foreach (var attemptLimits in attempts)
        {
            var cpus = await GetCpuCandidatesAsync(!needGpu, attemptLimits?[ComponentCategory.Cpu], cache);

            foreach (var cpu in cpus)
            {
                var build = await TryBuildAroundCpuAsync(cpu, request, needGpu, attemptLimits, cache);
                if (build is null)
                {
                    continue;
                }

                var total = CalculateTotalPrice(build);
                if (total <= request.Budget)
                {
                    return CreateBuildDto(build);
                }

                if (cheapestTotal is null || total < cheapestTotal)
                {
                    cheapestTotal = total;
                }
            }
        }

        if (cheapestTotal is null)
        {
            throw new InvalidOperationException("Не вдалося підібрати сумісну збірку.");
        }

        throw new InsufficientBudgetException(
            new ThresholdResult(false, cheapestTotal.Value, cheapestTotal.Value - request.Budget));
    }

    private async Task<IReadOnlyList<Cpu>> GetCpuCandidatesAsync(
        bool requireIntegratedGpu, decimal? limit, QueryCache cache)
    {
        var cpus = await cache.GetAsync(
            $"cpus:{requireIntegratedGpu}",
            async () => (await _cpuRepository.GetByFilterAsync(
                hasIntegratedGpu: requireIntegratedGpu ? true : null)).ToList());

        return OrderByPreference(cpus, limit).ToList();
    }

    private async Task<PCBuild?> TryBuildAroundCpuAsync(
        Cpu cpu,
        BuildRequestDto request,
        bool needGpu,
        IReadOnlyDictionary<ComponentCategory, decimal>? limits,
        QueryCache cache)
    {
        decimal? LimitFor(ComponentCategory category) => limits?[category];

        var storageTypes = request.StorageType is null ? null : new[] { request.StorageType.Value };

        var storage = PickByLimit(
            await cache.GetAsync(
                $"storage:{request.StorageType}:{request.RequestedStorageGb}",
                async () => (await _storageRepository.GetByFilterAsync(
                    driveType: storageTypes, capacityGb: request.RequestedStorageGb)).ToList()),
            LimitFor(ComponentCategory.Storage));
        if (storage is null)
        {
            return null;
        }

        var cooler = PickByLimit(
            await cache.GetAsync(
                $"cooler:{cpu.SocketId}:{cpu.TdpWatts}:{request.CoolingType}",
                async () => (await _coolerRepository.GetByFilterAsync(
                    socketId: cpu.SocketId, minTdpRatingWatts: cpu.TdpWatts,
                    coolingType: request.CoolingType)).ToList()),
            LimitFor(ComponentCategory.CpuCooler));
        if (cooler is null)
        {
            return null;
        }

        var boards = OrderByPreference(
                await cache.GetAsync(
                    $"boards:{cpu.SocketId}:{request.RequireWifi == true}",
                    async () => (await _motherboardRepository.GetByFilterAsync(
                        socketId: cpu.SocketId, hasWifi: request.RequireWifi == true ? true : null)).ToList()),
                LimitFor(ComponentCategory.Motherboard))
            .ToList();
        if (boards.Count == 0)
        {
            return null;
        }

        IReadOnlyList<Gpu?> gpus = needGpu
            ? OrderByPreference(
                    await cache.GetAsync("gpus", async () => (await _gpuRepository.GetByFilterAsync()).ToList()),
                    LimitFor(ComponentCategory.Gpu))
                .Select(g => (Gpu?)g)
                .ToList()
            : new Gpu?[] { null };

        PCBuild? best = null;
        var bestTotal = 0m;

        foreach (var gpu in gpus)
        {
            var totalTdp = _tdpCalculator.CalculateTotalTdp(cpu, gpu);
            var requiredWattage = _tdpCalculator.GetRecommendedPsuWattage(totalTdp);

            bool IsPsuUsable(PowerSupply psu) =>
                psu.Wattage >= requiredWattage
                && (gpu is null || (psu.Pcie8PinCount is not null && psu.Has12Vhpwr is not null))
                && _compatibilityChecker.IsPowerConnectorCompatible(gpu, psu);

            var powerSupply = PickByLimit(
                (await cache.GetAsync(
                    $"psu:{requiredWattage}",
                    async () => (await _powerSupplyRepository.GetByFilterAsync(wattage: requiredWattage)).ToList()))
                .Where(IsPsuUsable),
                LimitFor(ComponentCategory.PowerSupply));
            if (powerSupply is null)
            {
                continue;
            }

            var cases = await cache.GetAsync(
                $"cases:{gpu?.LengthMm}:{cooler.HeightMm}:{request.PreferredColor}",
                async () => (await _caseRepository.GetByFilterAsync(
                    minSupportedGpuLengthMm: gpu?.LengthMm,
                    minSupportedCoolerHeightMm: cooler.HeightMm,
                    color: request.PreferredColor)).ToList());

            foreach (var board in boards)
            {
                var ram = PickByLimit(
                    (await cache.GetAsync(
                        $"ram:{board.MemoryType}:{request.RequestedRamGb}:{board.MemorySlots}",
                        async () => (await _ramRepository.GetByFilterAsync(
                            memoryType: new[] { board.MemoryType },
                            minCapacityGb: request.RequestedRamGb,
                            maxModuleCount: board.MemorySlots)).ToList()))
                    .Where(x => _compatibilityChecker.IsRamCompatible(x, board)),
                    LimitFor(ComponentCategory.Ram));
                if (ram is null)
                {
                    continue;
                }

                var pcCase = PickByLimit(
                    cases.Where(x => x.MaxBoardFormFactorId is not null
                                     && _spaceControlService.FitsInCase(x, board, gpu, cooler)),
                    LimitFor(ComponentCategory.Case));
                if (pcCase is null)
                {
                    continue;
                }

                var candidate = new PCBuild
                {
                    Cpu = cpu,
                    Motherboard = board,
                    Ram = ram,
                    Gpu = gpu,
                    Storage = storage,
                    CpuCooler = cooler,
                    Case = pcCase,
                    PowerSupply = powerSupply
                };

                if (limits is not null)
                {
                    return candidate;
                }

                var total = CalculateTotalPrice(candidate);
                if (best is null || total < bestTotal)
                {
                    best = candidate;
                    bestTotal = total;
                }
            }
        }

        return best;
    }

    private static Category[] ResolveCategories(BuildRequestDto request)
    {
        var result = new List<Category>();

        if (request.Priorities is not null && request.Priorities.Any())
        {
            result.AddRange(UserTaskCategoryMapper.ToCategories(request.Priorities));
        }

        if (request.Category is not null)
        {
            result.Add(request.Category.Value);
        }

        if (result.Count == 0)
        {
            throw new ArgumentException("Категорію ПК не вказано.", nameof(request));
        }

        return result.Distinct().ToArray();
    }

    private static T? PickByLimit<T>(IEnumerable<T> candidates, decimal? limit) where T : Component
        => OrderByPreference(candidates, limit).FirstOrDefault();

    private static IEnumerable<T> OrderByPreference<T>(IEnumerable<T> items, decimal? limit) where T : Component
        => limit is null
            ? items.OrderBy(x => x.Price).ThenBy(x => x.Id)
            : items
                .OrderBy(x => x.Price > limit ? 1 : 0)
                .ThenBy(x => x.Price <= limit ? -x.Price : x.Price)
                .ThenBy(x => x.Id);

    private sealed class QueryCache
    {
        private readonly Dictionary<string, object> _items = new();

        public async Task<T> GetAsync<T>(string key, Func<Task<T>> load)
        {
            if (_items.TryGetValue(key, out var cached))
            {
                return (T)cached;
            }

            var value = await load();
            _items[key] = value!;
            return value;
        }
    }
    public IReadOnlyList<Component> FilterCompatibleComponents(IEnumerable<Component> pool, PCBuild currentBuild)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(currentBuild);

        return pool
            .Where(component => IsBuildCompatible(WithComponent(currentBuild, component)))
            .ToList();
    }

    private static InvalidOperationException NotFound(ComponentDto item)
        => new($"Деталь {item.Id} категорії {item.Category} не знайдена.");
     private async Task<PCBuild> RestoreBuildAsync(PCBuildDto dto)
    {
        var build = new PCBuild();
        var seen = new HashSet<ComponentCategory>();

        foreach (var item in dto.Components)
        {
            if (!seen.Add(item.Category))
            {
                throw new ArgumentException($"У збірці кілька деталей категорії {item.Category}.", nameof(dto));
            }

            switch (item.Category)
            {
                case ComponentCategory.Cpu:
                    build.Cpu = await _cpuRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.Motherboard:
                    build.Motherboard = await _motherboardRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.Gpu:
                    build.Gpu = await _gpuRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.Ram:
                    build.Ram = await _ramRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.Storage:
                    build.Storage = await _storageRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.CpuCooler:
                    build.CpuCooler = await _coolerRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.Case:
                    build.Case = await _caseRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                case ComponentCategory.PowerSupply:
                    build.PowerSupply = await _powerSupplyRepository.GetByIdAsync(item.Id) ?? throw NotFound(item);
                    break;
                default:
                    throw new ArgumentException($"Непідтримувана категорія: {item.Category}.", nameof(dto));
            }
        }

        return build;
    }

    public async Task<IReadOnlyList<Component>> FilterCompatibleComponentsAsync(
        IEnumerable<Component> pool, PCBuildDto currentBuild)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(currentBuild);

        var build = await RestoreBuildAsync(currentBuild);

        return FilterCompatibleComponents(pool, build);
    }

    public async Task<PCBuildDto> ReplaceComponentAsync(
        PCBuildDto currentBuild, ComponentCategory category, Component newComponent)
    {
        ArgumentNullException.ThrowIfNull(currentBuild);
        ArgumentNullException.ThrowIfNull(newComponent);

        var build = await RestoreBuildAsync(currentBuild);

        return ReplaceComponent(build, category, newComponent);
    }

    public PCBuildDto ReplaceComponent(PCBuild currentBuild, ComponentCategory category, Component newComponent)
    {
        ArgumentNullException.ThrowIfNull(currentBuild);
        ArgumentNullException.ThrowIfNull(newComponent);

        if (GetCategory(newComponent) != category)
        {
            throw new ArgumentException("Тип деталі не відповідає вказаній категорії.", nameof(newComponent));
        }

        if (currentBuild.Cpu is null
            || currentBuild.Motherboard is null
            || currentBuild.Ram is null
            || currentBuild.Storage is null
            || currentBuild.CpuCooler is null
            || currentBuild.Case is null
            || currentBuild.PowerSupply is null)
        {
            throw new InvalidOperationException("Збірка неповна: замінювати деталь можна лише в повній збірці.");
        }

        return CreateBuildDto(WithComponent(currentBuild, newComponent));
    }

    private static ComponentCategory GetCategory(Component component) => component switch
    {
        Cpu => ComponentCategory.Cpu,
        Motherboard => ComponentCategory.Motherboard,
        Gpu => ComponentCategory.Gpu,
        Ram => ComponentCategory.Ram,
        Storage => ComponentCategory.Storage,
        CpuCooler => ComponentCategory.CpuCooler,
        PCCase => ComponentCategory.Case,
        PowerSupply => ComponentCategory.PowerSupply,
        _ => throw new ArgumentException(
            $"Непідтримуваний тип компонента: {component.GetType().Name}.", nameof(component))
    };

    private static PCBuild WithComponent(PCBuild build, Component component)
    {
        var copy = new PCBuild
        {
            Cpu = build.Cpu,
            Motherboard = build.Motherboard,
            Gpu = build.Gpu,
            Ram = build.Ram,
            Storage = build.Storage,
            CpuCooler = build.CpuCooler,
            Case = build.Case,
            PowerSupply = build.PowerSupply
        };

        switch (component)
        {
            case Cpu cpu: copy.Cpu = cpu; break;
            case Motherboard motherboard: copy.Motherboard = motherboard; break;
            case Gpu gpu: copy.Gpu = gpu; break;
            case Ram ram: copy.Ram = ram; break;
            case Storage storage: copy.Storage = storage; break;
            case CpuCooler cooler: copy.CpuCooler = cooler; break;
            case PCCase pcCase: copy.Case = pcCase; break;
            case PowerSupply powerSupply: copy.PowerSupply = powerSupply; break;
            default:
                throw new ArgumentException(
                    $"Непідтримуваний тип компонента: {component.GetType().Name}.", nameof(component));
        }

        return copy;
    }

    private bool IsBuildCompatible(PCBuild build)
    {
        var cpu = build.Cpu;
        var motherboard = build.Motherboard;
        var gpu = build.Gpu;
        var ram = build.Ram;
        var cooler = build.CpuCooler;
        var pcCase = build.Case;
        var powerSupply = build.PowerSupply;

        if (cpu is not null && motherboard is not null
                            && !_compatibilityChecker.IsSocketCompatible(cpu, motherboard))
        {
            return false;
        }

        if (ram is not null && motherboard is not null
                            && !_compatibilityChecker.IsRamCompatible(ram, motherboard))
        {
            return false;
        }

        if (cpu is not null && cooler is not null
                            && !_compatibilityChecker.IsCoolerCompatible(cpu, cooler))
        {
            return false;
        }

        if (pcCase is not null)
        {
            if (pcCase.MaxBoardFormFactorId is null)
            {
                return false;
            }

            if (motherboard is not null && !_spaceControlService.FitsBoard(pcCase, motherboard))
            {
                return false;
            }

            if (gpu is not null && !_spaceControlService.FitsGpu(pcCase, gpu))
            {
                return false;
            }

            if (cooler is not null && !_spaceControlService.FitsCooler(pcCase, cooler))
            {
                return false;
            }
        }

        if (powerSupply is not null)
        {
            if (gpu is not null)
            {
                if (gpu.PowerConnectorType is null || gpu.PowerConnectorCount is null
                    || powerSupply.Pcie8PinCount is null || powerSupply.Has12Vhpwr is null)
                {
                    return false;
                }

                if (!_compatibilityChecker.IsPowerConnectorCompatible(gpu, powerSupply))
                {
                    return false;
                }
            }

            if (cpu is not null)
            {
                var totalTdp = _tdpCalculator.CalculateTotalTdp(cpu, gpu);
                var requiredWattage = _tdpCalculator.GetRecommendedPsuWattage(totalTdp);

                if (powerSupply.Wattage < requiredWattage)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static decimal CalculateTotalPrice(PCBuild build) =>
        build.Cpu.Price
        + build.Motherboard.Price
        + build.Ram.Price
        + build.Storage.Price
        + build.CpuCooler.Price
        + build.Case.Price
        + build.PowerSupply.Price
        + (build.Gpu?.Price ?? 0m);

    private PCBuildDto CreateBuildDto(PCBuild build)
    {
        var totalTdp = _tdpCalculator.CalculateTotalTdp(build.Cpu, build.Gpu);
        var totalPrice = CalculateTotalPrice(build);
        var isCompatible = IsBuildCompatible(build);

        var components = new List<Component>
        {
            build.Cpu,
            build.Motherboard,
            build.Ram,
            build.Storage,
            build.CpuCooler,
            build.Case,
            build.PowerSupply
        };

        if (build.Gpu is not null)
        {
            components.Insert(2, build.Gpu);
        }

        var componentDtos = components.Select(ComponentDtoMapper.ToDto).ToList();

        return new PCBuildDto(totalPrice, totalTdp, isCompatible, componentDtos);
    }
}