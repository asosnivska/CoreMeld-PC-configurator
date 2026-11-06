using CoreMeld.BLL.Settings;

namespace CoreMeld.BLL.Tests.Fakes;

public class FakeFormFactorSpecsProvider: IFormFactorSpecsProvider
{
    private readonly Dictionary<int, BoardSize> _sizes;

    public FakeFormFactorSpecsProvider(Dictionary<int, BoardSize> sizes)
    {
        _sizes = sizes;
    }

    public BoardSize GetBoardSize(int formFactorId)
    {
        if (!_sizes.TryGetValue(formFactorId, out var size))
        {
            throw new InvalidOperationException($"Unknown form factor id: {formFactorId}.");
        }

        return size;
    }
}