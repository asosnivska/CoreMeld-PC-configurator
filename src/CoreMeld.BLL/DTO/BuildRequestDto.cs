using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.DTO;

public record BuildRequestDto(
    decimal Budget,
    string? PreferredColor ,
    bool? RequireWifi,
    StorageType? StorageType,
    CoolingType? CoolingType, 
    Category? Category,
    IReadOnlyList<UserTask>? Priorities,
    int? RequestedRamGb,
    int? RequestedStorageGb 
    );
    