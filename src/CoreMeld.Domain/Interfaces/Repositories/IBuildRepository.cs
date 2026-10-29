using CoreMeld.Domain.Models;

namespace CoreMeld.Domain.Interfaces.Repositories;

public interface IBuildRepository
{
    Task<int> CreateBuildAsync(string name, decimal totalPrice, int totalTdp, IEnumerable<int> componentIds);

    Task UpdateBuildAsync(int buildId, string name, decimal totalPrice, int totalTdp, IEnumerable<int> componentIds);

    Task<IEnumerable<BuildSummary>> GetAllBuildsAsync(int? limit = null);

    Task<IEnumerable<BuildSummary>> GetBuildsByIdsAsync(IEnumerable<int> ids);

    Task<IEnumerable<int>> GetComponentIdsForBuildAsync(int buildId);
}
