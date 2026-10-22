using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.DTO;

public record BuildRequestDto(
    decimal Budget,
    string? PreferredColor ,
    bool? RequireWifi,
    StorageType? StorageType,
    CoolingType? CoolingType, 
    Category? Category,
    int? RequestedRamGb,
    int? RequestedStorageGb 
    );
    