using CoreMeld.Domain.Interfaces.Repositories;
using CoreMeld.Domain.Models;

namespace CoreMeld.DAL.InMemory;

public class InMemoryBuildRepository : IBuildRepository
{
    private static readonly List<(BuildSummary Summary, List<int> ComponentIds)> Builds = new()
    {
        (
            new BuildSummary { Id = 1, Name = "Бюджетна ігрова збірка AM4", TotalPrice = 29503m, TotalTdpWatts = 230, CreatedAt = DateTime.UtcNow },
            new List<int> { 1, 5, 2, 3, 13, 4, 18, 20 }
        ),
        (
            new BuildSummary { Id = 2, Name = "Оптимальна збірка AM5", TotalPrice = 60369m, TotalTdpWatts = 340, CreatedAt = DateTime.UtcNow },
            new List<int> { 22, 6, 12, 10, 14, 17, 19, 21 }
        ),
        (
            new BuildSummary { Id = 3, Name = "Потужна збірка Intel Core i7", TotalPrice = 69369m, TotalTdpWatts = 470, CreatedAt = DateTime.UtcNow },
            new List<int> { 24, 8, 12, 10, 14, 17, 19, 21 }
        )
    };

    public Task<int> CreateBuildAsync(string name, decimal totalPrice, int totalTdp, IEnumerable<int> componentIds)
    {
        int newId = Builds.Count > 0 ? Builds.Max(b => b.Summary.Id) + 1 : 1;
        var summary = new BuildSummary
        {
            Id = newId,
            Name = name,
            TotalPrice = totalPrice,
            TotalTdpWatts = totalTdp,
            CreatedAt = DateTime.UtcNow
        };
        Builds.Insert(0, (summary, componentIds.ToList()));
        return Task.FromResult(newId);
    }

    public Task UpdateBuildAsync(int buildId, string name, decimal totalPrice, int totalTdp, IEnumerable<int> componentIds)
    {
        var idx = Builds.FindIndex(b => b.Summary.Id == buildId);
        if (idx >= 0)
        {
            var updatedSummary = new BuildSummary
            {
                Id = buildId,
                Name = name,
                TotalPrice = totalPrice,
                TotalTdpWatts = totalTdp,
                CreatedAt = Builds[idx].Summary.CreatedAt
            };
            Builds[idx] = (updatedSummary, componentIds.ToList());
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<BuildSummary>> GetAllBuildsAsync(int? limit = null)
    {
        var query = Builds.Select(b => b.Summary);
        if (limit.HasValue) query = query.Take(limit.Value);
        return Task.FromResult<IEnumerable<BuildSummary>>(query.ToList());
    }

    public Task<IEnumerable<BuildSummary>> GetBuildsByIdsAsync(IEnumerable<int> ids)
    {
        var idSet = new HashSet<int>(ids);
        var res = Builds.Where(b => idSet.Contains(b.Summary.Id)).Select(b => b.Summary).ToList();
        return Task.FromResult<IEnumerable<BuildSummary>>(res);
    }

    public Task<IEnumerable<int>> GetComponentIdsForBuildAsync(int buildId)
    {
        var match = Builds.FirstOrDefault(b => b.Summary.Id == buildId);
        return Task.FromResult<IEnumerable<int>>(match.ComponentIds ?? new List<int>());
    }
}
