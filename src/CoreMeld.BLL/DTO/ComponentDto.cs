using CoreMeld.Domain.Models;

namespace CoreMeld.BLL.DTO;

public record ComponentDto(
        int Id,
        ComponentCategory Category,
        string Name,
        string Brand,
        decimal Price,
        string? ImageUrl,
        IReadOnlyList<CharacteristicDto> Characteristics
    );