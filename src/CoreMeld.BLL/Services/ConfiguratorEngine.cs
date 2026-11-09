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

        if (request.Category is null)
        {
            throw new ArgumentException("Категорію ПК не вказано.", nameof(request));
        }

        var categories = new[] { request.Category.Value };

        var threshold = await _budgetCalculator.CheckThresholdAsync(request.Budget, categories);
        if (!threshold.IsSufficient)
        {
            throw new InsufficientBudgetException(threshold);
        }

        var limits = _budgetCalculator.CalculateAllocation(request.Budget, categories);

        var needIntegratedGpu = limits[ComponentCategory.Gpu] == 0m;

        var cpuCandidates = await _cpuRepository.GetByFilterAsync(
            hasIntegratedGpu: needIntegratedGpu ? true : null,
            maxPrice: limits[ComponentCategory.Cpu]);

        var cpu = cpuCandidates
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (cpu is null)
        {
            var allCpus = await _cpuRepository.GetByFilterAsync(
                hasIntegratedGpu: needIntegratedGpu ? true : null);

            cpu = allCpus
                      .OrderBy(x => x.Price)
                      .ThenBy(x => x.Id)
                      .FirstOrDefault()
                  ?? throw new InvalidOperationException("Не вдалося підібрати процесор.");
        }

        var motherBoardCandidates = await _motherboardRepository.GetByFilterAsync(
            socketId: cpu.SocketId,
            maxPrice: limits[ComponentCategory.Motherboard],
            hasWifi: request.RequireWifi == true ? true : null);

        var motherboard = motherBoardCandidates
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (motherboard is null)
        {
            var allmotherboard = await _motherboardRepository.GetByFilterAsync(
                socketId: cpu.SocketId,
                hasWifi: request.RequireWifi == true ? true : null);

            motherboard = allmotherboard
                              .OrderBy(x => x.Price)
                              .ThenBy(x => x.Id)
                              .FirstOrDefault()
                          ?? throw new InvalidOperationException("Не вдалося підібрати материнську плату.");
        }

        var ramCandidates = await _ramRepository.GetByFilterAsync(
            memoryType: new[] { motherboard.MemoryType },
            minCapacityGb: request.RequestedRamGb,
            maxModuleCount: motherboard.MemorySlots,
            maxPrice: limits[ComponentCategory.Ram]);

        var ram = ramCandidates
            .Where(x => _compatibilityChecker.IsRamCompatible(x, motherboard))
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (ram is null)
        {
            var allRam = await _ramRepository.GetByFilterAsync(
                memoryType: new[] { motherboard.MemoryType },
                minCapacityGb: request.RequestedRamGb,
                maxModuleCount: motherboard.MemorySlots);

            ram = allRam
                      .Where(x => _compatibilityChecker.IsRamCompatible(x, motherboard))
                      .OrderBy(x => x.Price)
                      .ThenBy(x => x.Id)
                      .FirstOrDefault()
                  ?? throw new InvalidOperationException("Не вдалося підібрати ОЗП.");
        }


        Gpu? gpu = null;

        if (limits[ComponentCategory.Gpu] > 0m)
        {
            var gpuCandidates = await _gpuRepository.GetByFilterAsync(
                maxPrice: limits[ComponentCategory.Gpu]);

            gpu = gpuCandidates
                .OrderByDescending(x => x.Price)
                .ThenBy(x => x.Id)
                .FirstOrDefault();

            if (gpu is null)
            {
                var allGpus = await _gpuRepository.GetByFilterAsync();

                gpu = allGpus
                          .OrderBy(x => x.Price)
                          .ThenBy(x => x.Id)
                          .FirstOrDefault()
                      ?? throw new InvalidOperationException("Не вдалося підібрати відеокарту.");
            }
        }

        var storageTypes = request.StorageType is null
            ? null
            : new[] { request.StorageType.Value };

        var storageCandidates = await _storageRepository.GetByFilterAsync(
            driveType: storageTypes,
            capacityGb: request.RequestedStorageGb,
            maxPrice: limits[ComponentCategory.Storage]);

        var storage = storageCandidates
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (storage is null)
        {
            var allStorage = await _storageRepository.GetByFilterAsync(
                driveType: storageTypes,
                capacityGb: request.RequestedStorageGb);

            storage = allStorage
                          .OrderBy(x => x.Price)
                          .ThenBy(x => x.Id)
                          .FirstOrDefault()
                      ?? throw new InvalidOperationException("Не вдалося підібрати накопичувач.");
        }

        var coolerCandidates = await _coolerRepository.GetByFilterAsync(
            socketId: cpu.SocketId,
            minTdpRatingWatts: cpu.TdpWatts,
            coolingType: request.CoolingType,
            maxPrice: limits[ComponentCategory.CpuCooler]);

        var cooler = coolerCandidates
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (cooler is null)
        {
            var allCoolers = await _coolerRepository.GetByFilterAsync(
                socketId: cpu.SocketId,
                minTdpRatingWatts: cpu.TdpWatts,
                coolingType: request.CoolingType);

            cooler = allCoolers
                         .OrderBy(x => x.Price)
                         .ThenBy(x => x.Id)
                         .FirstOrDefault()
                     ?? throw new InvalidOperationException("Не вдалося підібрати кулер.");
        }

        var caseCandidates = await _caseRepository.GetByFilterAsync(
            minSupportedGpuLengthMm: gpu?.LengthMm,
            minSupportedCoolerHeightMm: cooler.HeightMm,
            color: request.PreferredColor,
            maxPrice: limits[ComponentCategory.Case]);

        var pcCase = caseCandidates
            .Where(x => x.MaxBoardFormFactorId is not null
                                               && _spaceControlService.FitsInCase(x, motherboard, gpu, cooler))
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (pcCase is null)
        {
            var allCases = await _caseRepository.GetByFilterAsync(
                minSupportedGpuLengthMm: gpu?.LengthMm,
                minSupportedCoolerHeightMm: cooler.HeightMm,
                color: request.PreferredColor);

            pcCase = allCases
                         .Where(x => x.MaxBoardFormFactorId is not null
                                     && _spaceControlService.FitsInCase(x, motherboard, gpu, cooler))
                         .OrderBy(x => x.Price)
                         .ThenBy(x => x.Id)
                         .FirstOrDefault()
                     ?? throw new InvalidOperationException("Не вдалося підібрати корпус.");
        }

        var totalTdp = _tdpCalculator.CalculateTotalTdp(cpu, gpu);
        var requiredWattage = _tdpCalculator.GetRecommendedPsuWattage(totalTdp);

        bool IsPsuUsable(PowerSupply psu) =>
            psu.Wattage >= requiredWattage
            && (gpu is null || (psu.Pcie8PinCount is not null && psu.Has12Vhpwr is not null))
            && _compatibilityChecker.IsPowerConnectorCompatible(gpu, psu);

        var psuCandidates = await _powerSupplyRepository.GetByFilterAsync(
            wattage: requiredWattage,
            maxPrice: limits[ComponentCategory.PowerSupply]);

        var powerSupply = psuCandidates
            .Where(IsPsuUsable)
            .OrderByDescending(x => x.Price)
            .ThenBy(x => x.Id)
            .FirstOrDefault();

        if (powerSupply is null)
        {
            var allPsu = await _powerSupplyRepository.GetByFilterAsync(wattage: requiredWattage);

            powerSupply = allPsu
                              .Where(IsPsuUsable)
                              .OrderBy(x => x.Price)
                              .ThenBy(x => x.Id)
                              .FirstOrDefault()
                          ?? throw new InvalidOperationException("Не вдалося підібрати блок живлення.");
        }

        var build = new PCBuild
        {
            Cpu = cpu,
            Case = pcCase,
            CpuCooler = cooler,
            Gpu = gpu,
            Motherboard = motherboard,
            PowerSupply = powerSupply,
            Ram = ram,
            Storage = storage
        };

        var totalPrice = CalculateTotalPrice(build);

        if (totalPrice > request.Budget)
        {
            var thresholdResult = new ThresholdResult(false, totalPrice, totalPrice - request.Budget);
            throw new InsufficientBudgetException(thresholdResult);
        }

        return CreateBuildDto(build);
    }

    public IReadOnlyList<Component> FilterCompatibleComponents(IEnumerable<Component> pool, PCBuild currentBuild)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(currentBuild);

        return pool
            .Where(component => IsBuildCompatible(WithComponent(currentBuild, component)))
            .ToList();
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