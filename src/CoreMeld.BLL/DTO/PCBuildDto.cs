namespace CoreMeld.BLL.DTO;

public record PCBuildDto(
    decimal TotalPrice,
    int TotalTdp,
    bool IsCompatible,
    IReadOnlyList<ComponentDto> Components
    );