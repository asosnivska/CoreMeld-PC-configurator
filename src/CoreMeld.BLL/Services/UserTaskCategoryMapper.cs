using CoreMeld.Domain.Enums;

namespace CoreMeld.BLL.Services;

public static class UserTaskCategoryMapper
{
    private static readonly Dictionary<UserTask, Category> _categories = new()
    {
        [UserTask.WebBrowsing] = Category.HomeFullHd,
        [UserTask.MediaConsumption] = Category.Home2K,
        [UserTask.FileStorage] = Category.HomeFullHd,
        [UserTask.OfficeWork] = Category.Office,
        [UserTask.VideoConferencing] = Category.HomeFullHd,
        [UserTask.HeavyMultitasking] = Category.Design2D,
        [UserTask.Programming] = Category.Office,
        [UserTask.EngineeringAndCad] = Category.Design3D,
        [UserTask.MathModeling] = Category.Rendering,
        [UserTask.AudioProduction] = Category.Office,
        [UserTask.EconomicsAndAnalytics] = Category.Office,
        [UserTask.Gaming1080p] = Category.GamingFullHd,
        [UserTask.Gaming1440p] = Category.Gaming2K,
        [UserTask.Gaming4K] = Category.Gaming4K,
        [UserTask.UiDesign] = Category.Design2D,
        [UserTask.PhotoEditing] = Category.Design2D,
        [UserTask.VideoEditing] = Category.Design3D,
        [UserTask.ThreeDModeling] = Category.Design3D,
        [UserTask.AiAndBigData] = Category.Rendering
    };
    

    public static IReadOnlyCollection<Category> ToCategories(IEnumerable<UserTask> tasks)
    {
        ArgumentNullException.ThrowIfNull(tasks);

        List<Category> result = new List<Category>();
        foreach (var t in tasks)
        {
            if (_categories.ContainsKey(t))
            {
                if (!result.Contains(_categories[t]))
                {
                    result.Add(_categories[t]);
                }
            }
            else
            {
                throw new ArgumentException($"Для завдання {t} не задано категорію.");
            }
        }
        return result;
    }
}