using CoreMeld.BLL.DTO;

namespace CoreMeld.BLL.Validation;

/// <summary>
/// Checks BuildRequestDto fields before the engine uses them.
/// The UI has its own checks, but they can be bypassed, so the BLL checks again.
/// </summary>
public static class BuildRequestValidator
{
    public static void Validate(BuildRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Budget > BuildRequestLimits.MaxBudget)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Budget,
                $"Бюджет не може перевищувати {BuildRequestLimits.MaxBudget:N0} грн.");
        }

        if (request.Category is { } category && !Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(nameof(request), category,
                "Невідома категорія ПК.");
        }

        if (request.StorageType is { } storageType && !Enum.IsDefined(storageType))
        {
            throw new ArgumentOutOfRangeException(nameof(request), storageType,
                "Невідомий тип накопичувача.");
        }

        if (request.CoolingType is { } coolingType && !Enum.IsDefined(coolingType))
        {
            throw new ArgumentOutOfRangeException(nameof(request), coolingType,
                "Невідомий тип охолодження.");
        }

        if (request.Priorities is not null)
        {
            foreach (var task in request.Priorities)
            {
                if (!Enum.IsDefined(task))
                {
                    throw new ArgumentOutOfRangeException(nameof(request), task,
                        "Невідома задача у списку пріоритетів.");
                }
            }
        }

        if (request.RequestedRamGb is { } ramGb
            && ramGb is < BuildRequestLimits.MinRamGb or > BuildRequestLimits.MaxRamGb)
        {
            throw new ArgumentOutOfRangeException(nameof(request), ramGb,
                $"Обсяг пам'яті має бути від {BuildRequestLimits.MinRamGb} до {BuildRequestLimits.MaxRamGb} ГБ.");
        }

        if (request.RequestedStorageGb is { } storageGb
            && storageGb is < BuildRequestLimits.MinStorageGb or > BuildRequestLimits.MaxStorageGb)
        {
            throw new ArgumentOutOfRangeException(nameof(request), storageGb,
                $"Обсяг накопичувача має бути від {BuildRequestLimits.MinStorageGb} до {BuildRequestLimits.MaxStorageGb} ГБ.");
        }

        if (request.PreferredColor is { Length: > BuildRequestLimits.MaxColorLength })
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.PreferredColor.Length,
                $"Назва кольору не може бути довшою за {BuildRequestLimits.MaxColorLength} символів.");
        }
    }
}
