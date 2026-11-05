using CoreMeld.BLL.Services;
using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Tests.Services;

public class UserTaskCategoryMapperTests
{
    [Fact]
    public void ToCategories_UnknownTask_Throws()
    {
        var fakeUserTasks = (UserTask)999;
        Assert.Throws<ArgumentException>(() =>
            UserTaskCategoryMapper.ToCategories(new List<UserTask>() { fakeUserTasks }));
    }

    [Theory]
    [InlineData(new UserTask[] { UserTask.AiAndBigData, UserTask.AiAndBigData, UserTask.FileStorage }, 2)]
    [InlineData(new UserTask[]  { UserTask.Gaming4K, UserTask.AiAndBigData, UserTask.FileStorage }, 3)]
    [InlineData(new UserTask[]  { UserTask.OfficeWork, UserTask.OfficeWork, UserTask.OfficeWork }, 1)]
    [InlineData(new UserTask[] { UserTask.OfficeWork, UserTask.Programming, UserTask.AudioProduction }, 1)]
    public void ToCategories_TasksWithSameCategory_ReturnsDistinctCategories(UserTask[] tasks, int expectedCount)
    {
        var res = UserTaskCategoryMapper.ToCategories(tasks.ToList());
        Assert.Equal(expectedCount,res.Count);
        Assert.Equal(res.Count,res.Distinct().Count());
    }
}