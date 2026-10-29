using System.Diagnostics;
using CoreMeld.BLL.DTO;
using CoreMeld.DAL.InMemory;
using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.Services;

public class ComponentService : IComponentService
{
    private readonly ICpuRepository _cpuRepository;
    private readonly IMotherboardRepository _motherboardRepository;
    private readonly IGpuRepository _gpuRepository;
    private readonly IRamRepository _ramRepository;
    private readonly IStorageRepository _storageRepository;
    private readonly ICpuCoolerRepository _cpuCoolerRepository;
    private readonly IPCCaseRepository _caseRepository;
    private readonly IPowerSupplyRepository _powerSupplyRepository;

    public ComponentService(
        ICpuRepository cpuRepository,
        IMotherboardRepository motherboardRepository,
        IGpuRepository gpuRepository,
        IRamRepository ramRepository,
        IStorageRepository storageRepository,
        ICpuCoolerRepository cpuCoolerRepository,
        IPCCaseRepository caseRepository,
        IPowerSupplyRepository powerSupplyRepository)
    {
        _cpuRepository = cpuRepository;
        _motherboardRepository = motherboardRepository;
        _gpuRepository = gpuRepository;
        _ramRepository = ramRepository;
        _storageRepository = storageRepository;
        _cpuCoolerRepository = cpuCoolerRepository;
        _caseRepository = caseRepository;
        _powerSupplyRepository = powerSupplyRepository;
    }

    public async Task<IReadOnlyList<ComponentDto>> GetComponentsByCategoryAsync(
        ComponentCategory category,
        string? search = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        bool? sortByPriceAsc = null,
        IEnumerable<string>? brands = null)
    {
        var searchFilter = string.IsNullOrWhiteSpace(search) ? null : search.Trim();
        var brandList = brands?.Where(b => !string.IsNullOrWhiteSpace(b)).ToList();
        IEnumerable<string>? brandsArg = brandList is { Count: > 0 } ? brandList : null;

        IEnumerable<ComponentDto> results;

        try
        {
            results = category switch
            {
                ComponentCategory.Cpu =>
                    (await _cpuRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.Motherboard =>
                    (await _motherboardRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.Gpu =>
                    (await _gpuRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.Ram =>
                    (await _ramRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.Storage =>
                    (await _storageRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.CpuCooler =>
                    (await _cpuCoolerRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.Case =>
                    (await _caseRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                ComponentCategory.PowerSupply =>
                    (await _powerSupplyRepository.GetByFilterAsync(minPrice: minPrice, maxPrice: maxPrice, brands: brandsArg, nameContains: searchFilter))
                    .Select(ComponentMapper.ToDto),

                _ => Array.Empty<ComponentDto>()
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ComponentService] Database query error: {ex.Message}. Falling back to InMemory catalog.");
            results = GetFromInMemoryFallback(category, searchFilter, minPrice, maxPrice, brandsArg);
        }

        if (sortByPriceAsc == true)
            results = results.OrderBy(c => c.Price);
        else if (sortByPriceAsc == false)
            results = results.OrderByDescending(c => c.Price);

        return results.ToList();
    }

    public async Task<IReadOnlyList<string>> GetBrandsByCategoryAsync(ComponentCategory category)
    {
        var components = await GetComponentsByCategoryAsync(category);
        return components
            .Select(c => c.Brand)
            .Where(b => !string.IsNullOrWhiteSpace(b))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(b => b)
            .ToList();
    }

    private static IEnumerable<ComponentDto> GetFromInMemoryFallback(
        ComponentCategory category,
        string? search,
        decimal? minPrice,
        decimal? maxPrice,
        IEnumerable<string>? brands)
    {
        var brandSet = brands != null ? new HashSet<string>(brands, StringComparer.OrdinalIgnoreCase) : null;

        bool Filter(Component c)
        {
            if (minPrice.HasValue && c.Price < minPrice.Value) return false;
            if (maxPrice.HasValue && c.Price > maxPrice.Value) return false;
            if (brandSet != null && !brandSet.Contains(c.Brand)) return false;
            if (!string.IsNullOrWhiteSpace(search) && !c.Name.Contains(search, StringComparison.OrdinalIgnoreCase)) return false;
            return true;
        }

        return category switch
        {
            ComponentCategory.Cpu => InMemoryCatalog.Cpus.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.Motherboard => InMemoryCatalog.Motherboards.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.Gpu => InMemoryCatalog.Gpus.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.Ram => InMemoryCatalog.Rams.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.Storage => InMemoryCatalog.Storages.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.CpuCooler => InMemoryCatalog.CpuCoolers.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.Case => InMemoryCatalog.Cases.Where(Filter).Select(ComponentMapper.ToDto),
            ComponentCategory.PowerSupply => InMemoryCatalog.PowerSupplies.Where(Filter).Select(ComponentMapper.ToDto),
            _ => Enumerable.Empty<ComponentDto>()
        };
    }
}

