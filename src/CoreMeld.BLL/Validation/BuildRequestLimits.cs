namespace CoreMeld.BLL.Validation;

/// <summary>
/// Limits for a build request. Shared by the BLL and the UI, so the UI check
/// and the BLL check can never disagree.
/// </summary>
public static class BuildRequestLimits
{
    public const decimal MinBudget = 1m;
    public const decimal MaxBudget = 10_000_000m;

    public const int MinRamGb = 1;
    public const int MaxRamGb = 256;

    public const int MinStorageGb = 1;
    public const int MaxStorageGb = 20_000;

    /// <summary>Same as components.color VARCHAR(50).</summary>
    public const int MaxColorLength = 50;
}
