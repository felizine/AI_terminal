using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace AITerminal.UI.ViewModels.Workspaces;

public sealed class TodayWorkspaceViewModel : WorkspaceViewModel
{
    public TodayWorkspaceViewModel(Action openTasks, Action openSchedule)
    {
        Stats = DemoWorkspaceData.CreateStats();
        Agenda = DemoWorkspaceData.CreateAgenda();
        Tasks = DemoWorkspaceData.CreateTasks().Take(3).ToArray();
        OpenTasksCommand = new RelayCommand(openTasks);
        OpenScheduleCommand = new RelayCommand(openSchedule);
    }

    public override string Kicker => "DAILY BRIEFING";

    public override string Title => "Today, at a glance";

    public override string Description =>
        "A compact view of what needs your attention. Ask for more detail whenever you need it.";

    public IReadOnlyList<SummaryStatViewModel> Stats { get; }

    public IReadOnlyList<AgendaItemViewModel> Agenda { get; }

    public IReadOnlyList<TaskItemViewModel> Tasks { get; }

    public ICommand OpenTasksCommand { get; }

    public ICommand OpenScheduleCommand { get; }

    public string ConflictTitle => "Project planning overlaps Weekly review";

    public string ConflictDescription =>
        "Both occupy 14:00–14:30. The assistant will propose alternatives before changing anything.";

    public string FocusSummary => "2 hours of focus time remain after 15:00";
}
