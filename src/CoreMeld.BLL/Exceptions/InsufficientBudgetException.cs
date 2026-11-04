using CoreMeld.BLL.DTO;

namespace CoreMeld.BLL.Exceptions;

public class InsufficientBudgetException:Exception
{
    public ThresholdResult Result { get; }

    public InsufficientBudgetException(ThresholdResult result) : base(BuildMessage(result))
    {
        Result = result;
    }

    private static string BuildMessage(ThresholdResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return $"Бюджету недостатньо. Мінімум: {result.MinimumBudget} грн, не вистачає: {result.Shortfall} грн.";
    }
}