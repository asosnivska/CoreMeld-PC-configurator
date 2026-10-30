namespace CoreMeld.BLL.DTO;

public record ThresholdResult(
    bool IsSufficient,
    decimal MinimumBudget,
    decimal Shortfall
    );