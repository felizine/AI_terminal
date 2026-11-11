using System.Windows.Input;

namespace AITerminal.UI.ViewModels.Workspaces;

public sealed class SummaryStatViewModel(
    string value,
    string label,
    string detail,
    bool isWarning = false)
{
    public string Value { get; } = value;

    public string Label { get; } = label;

    public string Detail { get; } = detail;

    public bool IsWarning { get; } = isWarning;
}

public sealed class AgendaItemViewModel(
    string start,
    string end,
    string title,
    string meta,
    bool isConflict = false,
    bool isPast = false)
{
    public string Start { get; } = start;

    public string End { get; } = end;

    public string TimeLabel => $"{Start}–{End}";

    public string Title { get; } = title;

    public string Meta { get; } = meta;

    public bool IsConflict { get; } = isConflict;

    public bool IsPast { get; } = isPast;
}

public sealed class TaskItemViewModel(
    string title,
    string project,
    string due,
    string priority,
    bool isHighPriority = false,
    bool isComplete = false)
{
    public string Title { get; } = title;

    public string Project { get; } = project;

    public string Due { get; } = due;

    public string Priority { get; } = priority;

    public bool IsHighPriority { get; } = isHighPriority;

    public bool IsComplete { get; } = isComplete;

    public string StatusGlyph => IsComplete ? "✓" : "○";
}

public sealed class ScheduleItemViewModel(
    string start,
    string end,
    string title,
    string detail,
    bool isConflict = false,
    bool isPast = false)
{
    public string Start { get; } = start;

    public string End { get; } = end;

    public string TimeLabel => $"{Start}–{End}";

    public string Title { get; } = title;

    public string Detail { get; } = detail;

    public bool IsConflict { get; } = isConflict;

    public bool IsPast { get; } = isPast;
}

public sealed class WorkspaceActionViewModel(string label, ICommand command)
{
    public string Label { get; } = label;

    public ICommand Command { get; } = command;
}
