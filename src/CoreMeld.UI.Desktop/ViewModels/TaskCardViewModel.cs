using CoreMeld.Domain.Enums;

namespace CoreMeld.UI.Desktop.ViewModels;

public class TaskCardViewModel : ViewModelBase
{
    private bool _isSelected;

    public TaskCardViewModel(UserTask task, string title, string? subtitle = null)
    {
        Task = task;
        Title = title;
        Subtitle = subtitle;
    }

    public UserTask Task { get; }
    public string Title { get; }
    public string? Subtitle { get; }
    public bool HasSubtitle => !string.IsNullOrEmpty(Subtitle);

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

public class TaskGroupViewModel
{
    public TaskGroupViewModel(string title, IReadOnlyList<TaskCardViewModel> cards)
    {
        Title = title;
        Cards = cards;
    }

    public string Title { get; }
    public IReadOnlyList<TaskCardViewModel> Cards { get; }
}