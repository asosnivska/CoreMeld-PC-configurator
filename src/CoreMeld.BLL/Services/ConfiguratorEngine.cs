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

        var totalPrice = cpu.Price + pcCase.Price + cooler.Price +
                         (gpu is null ? 0m : gpu.Price) + motherboard.Price + powerSupply.Price + ram.Price +
                         storage.Price;

        if (totalPrice > request.Budget)
        {
            var ThresholdRes = new ThresholdResult(false, totalPrice, totalPrice - request.Budget);
            throw new InsufficientBudgetException(ThresholdRes);
        }

        var isCompatible =
            _compatibilityChecker.IsSocketCompatible(cpu, motherboard)
            && _compatibilityChecker.IsRamCompatible(ram, motherboard)
            && _compatibilityChecker.IsPowerConnectorCompatible(gpu, powerSupply)
            && _spaceControlService.FitsInCase(pcCase, motherboard, gpu, cooler)
            && powerSupply.Wattage >= requiredWattage;
        
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

    public PCBuildDto ReplaceComponent(PCBuild currentBuild, ComponentCategory category, Component newComponent)
    {
        throw new NotImplementedException();
    }
}