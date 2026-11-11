namespace AITerminal.UI.ViewModels.Workspaces;

public sealed class TasksWorkspaceViewModel : WorkspaceViewModel
{
    public TasksWorkspaceViewModel()
    {
        Tasks = DemoWorkspaceData.CreateTasks();
    }

    public override string Kicker => "TASKS";

    public override string Title => "Today's priorities";

    public override string Description =>
        "A complete workspace opens only when the summary is not enough. Writes remain disabled in Phase 1.";

    public IReadOnlyList<TaskItemViewModel> Tasks { get; }

    public int OpenCount => Tasks.Count(task => !task.IsComplete);

    public int HighPriorityCount => Tasks.Count(task => task.IsHighPriority && !task.IsComplete);
}
