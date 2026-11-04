using CoreMeld.BLL.DTO;
using CoreMeld.BLL.Exceptions;
using CoreMeld.BLL.Services;
namespace CoreMeld.BLL.Tests.Exceptions;

public class InsufficientBudgetExceptionTests
{
    [Fact]
    public void Ctor_StoresResult()
    {
        var result = new ThresholdResult(false, 30000, 10000);

        var ex = new InsufficientBudgetException(result);
        
        Assert.Same(result,ex.Result);
    }
    [Fact]
    public void Ctor_MessageContainsMinimumAndShortfall()
    {
        var result = new ThresholdResult(false, 30000, 10000);

        var ex = new InsufficientBudgetException(result);

        Assert.Contains("30000", ex.Message);
        Assert.Contains("10000", ex.Message);
    }

    [Fact]
    public void Ctor_NullResult_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new InsufficientBudgetException(null!));
    }
}