namespace AITerminal.UI.ViewModels.Workspaces;

public sealed class ScheduleWorkspaceViewModel : WorkspaceViewModel
{
    public ScheduleWorkspaceViewModel()
    {
        Items = DemoWorkspaceData.CreateSchedule();
    }

    public override string Kicker => "SCHEDULE";

    public override string Title => "Today's timeline";

    public override string Description =>
        "The shell demonstrates conflict visibility today. Calendar connections and rescheduling arrive in Phase 2.";

    public IReadOnlyList<ScheduleItemViewModel> Items { get; }
}
