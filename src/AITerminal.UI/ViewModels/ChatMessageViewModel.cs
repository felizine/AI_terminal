using AITerminal.Core.Conversations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AITerminal.UI.ViewModels;

public partial class ChatMessageViewModel : ObservableObject
{
    public ChatMessageViewModel(
        ChatRole role,
        string content,
        DateTimeOffset createdAt,
        bool reduceMotion = false,
        bool isStreaming = false)
    {
        Role = role;
        _content = content;
        _createdAt = createdAt;
        ReduceMotion = reduceMotion;
        _isStreaming = isStreaming;
    }

    public ChatRole Role { get; }

    public bool ReduceMotion { get; }

    public bool IsUser => Role == ChatRole.User;

    public bool IsAssistant => Role == ChatRole.Assistant;

    public string Author => IsUser ? "You" : "Assistant";

    public string Badge => IsUser ? "YOU" : "AI";

    public string TimeLabel => CreatedAt.ToLocalTime().ToString("HH:mm");

    [ObservableProperty]
    private string _content;

    [ObservableProperty]
    private DateTimeOffset _createdAt;

    [ObservableProperty]
    private bool _isStreaming;

    public void Append(string delta) => Content += delta;

    public void Complete(string content, DateTimeOffset createdAt)
    {
        Content = content;
        CreatedAt = createdAt;
        IsStreaming = false;
    }

    public void Stop(string content)
    {
        Content = content;
        IsStreaming = false;
    }

    partial void OnCreatedAtChanged(DateTimeOffset value) =>
        OnPropertyChanged(nameof(TimeLabel));
}
