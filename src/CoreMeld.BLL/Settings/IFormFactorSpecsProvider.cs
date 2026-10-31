namespace CoreMeld.BLL.Settings;

public interface IFormFactorSpecsProvider
{
    BoardSize GetBoardSize(int formFactorId);
}