using System.Windows.Input;

namespace AITerminal.UI.ViewModels;

public sealed class NotificationItemViewModel(
    string category,
    string title,
    string summary,
    string meta,
    string glyph,
    bool isWarning,
    string actionLabel,
    ICommand? openCommand)
{
    public string Category { get; } = category;

    public string Title { get; } = title;

    public string Summary { get; } = summary;

    public string Meta { get; } = meta;

    public string Glyph { get; } = glyph;

    public bool IsWarning { get; } = isWarning;

    public string ActionLabel { get; } = actionLabel;

    public ICommand? OpenCommand { get; } = openCommand;

    public bool HasAction => OpenCommand is not null && !string.IsNullOrWhiteSpace(ActionLabel);
}
