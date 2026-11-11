using System.Windows.Input;

namespace AITerminal.UI.ViewModels.Workspaces;

internal static class DemoWorkspaceData
{
    public static IReadOnlyList<SummaryStatViewModel> CreateStats() =>
    [
        new("3", "Meetings", "Next at 11:30"),
        new("5", "Open tasks", "2 due before noon"),
        new("1", "Conflict", "Needs a decision", isWarning: true),
    ];

    public static IReadOnlyList<AgendaItemViewModel> CreateAgenda() =>
    [
        new("09:00", "09:20", "Team stand-up", "Studio · 20 min", isPast: true),
        new("11:30", "12:15", "Candidate interview", "Meet · 45 min"),
        new("13:30", "14:30", "Weekly review", "Focus room · 60 min"),
    ];

    public static IReadOnlyList<TaskItemViewModel> CreateTasks() =>
    [
        new("Approve Q4 board narrative", "Board", "Due 10:00", "High", isHighPriority: true),
        new("Send revised hiring plan", "People", "Due 11:30", "High", isHighPriority: true),
        new("Review vendor security pack", "Operations", "Due 15:00", "Normal"),
        new("Reply to Maya's board update", "Inbox", "Due today", "Normal"),
        new("Prepare travel brief", "Admin", "Tomorrow", "Low"),
    ];

    public static IReadOnlyList<ScheduleItemViewModel> CreateSchedule() =>
    [
        new("09:00", "09:20", "Team stand-up", "Studio · 20 min · Daily sync", isPast: true),
        new("11:30", "12:15", "Candidate interview", "Meet · 45 min · Interview panel"),
        new("13:30", "14:30", "Weekly review", "Focus room · 60 min · Recurring"),
        new("14:00", "15:00", "Project planning", "Deep work · 60 min · Conflict with Weekly review", isConflict: true),
        new("16:00", "16:30", "Board narrative review", "Meet · 30 min · With Maya"),
    ];

    public static IReadOnlyList<NotificationItemViewModel> CreateNotifications(
        ICommand scheduleCommand,
        ICommand tasksCommand,
        ICommand briefingCommand) =>
    [
        new(
            "CALENDAR",
            "Weekly review",
            "Starts at 13:30 in the focus room.",
            "In 2 hours",
            "◷",
            isWarning: false,
            "View schedule",
            scheduleCommand),
        new(
            "INBOX",
            "Maya shared a board update",
            "The Q4 narrative needs your approval before tomorrow.",
            "12 minutes ago",
            "✉",
            isWarning: false,
            "Open briefing",
            briefingCommand),
        new(
            "TASKS",
            "Two priorities due before noon",
            "Board narrative and the revised hiring plan are still open.",
            "Today",
            "✓",
            isWarning: true,
            "View tasks",
            tasksCommand),
        new(
            "PROJECT",
            "Aurora workspace",
            "Design review is complete. Two blockers remain.",
            "Since yesterday",
            "◇",
            isWarning: false,
            "Open briefing",
            briefingCommand),
    ];
}
