using System.Collections.ObjectModel;
using System.Windows.Input;
using AITerminal.Core.Conversations;
using AITerminal.Core.Environment;
using AITerminal.UI.ViewModels.Workspaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AITerminal.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private const string NewConversationTitle = "New conversation";

    private readonly ConversationService _conversationService;
    private readonly IChatCompletionService _chatCompletionService;
    private readonly IAppEnvironment _appEnvironment;
    private readonly ILogger<MainWindowViewModel> _logger;

    private Conversation? _conversation;
    private CancellationTokenSource? _sendCancellation;
    private bool _initialized;
    private bool _disposed;

    public MainWindowViewModel(
        ConversationService conversationService,
        IChatCompletionService chatCompletionService,
        IAppEnvironment appEnvironment,
        ILogger<MainWindowViewModel> logger)
    {
        _conversationService = conversationService;
        _chatCompletionService = chatCompletionService;
        _appEnvironment = appEnvironment;
        _logger = logger;

        OpenTodayCommand = new RelayCommand(OpenToday);
        OpenTasksCommand = new RelayCommand(OpenTasks);
        OpenScheduleCommand = new RelayCommand(OpenSchedule);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        CloseWorkspaceCommand = new RelayCommand(CloseWorkspace);
        NewConversationCommand = new RelayCommand(StartNewConversation);
        CancelResponseCommand = new RelayCommand(CancelResponse);
        ClearErrorCommand = new RelayCommand(() => ErrorMessage = null);

        Notifications = new ObservableCollection<NotificationItemViewModel>(
            DemoWorkspaceData.CreateNotifications(
                OpenScheduleCommand,
                OpenTasksCommand,
                OpenTodayCommand));

        Messages.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsConversationEmpty));
            OnPropertyChanged(nameof(HasMessages));
        };
    }

    public ObservableCollection<ChatMessageViewModel> Messages { get; } = [];

    public ObservableCollection<NotificationItemViewModel> Notifications { get; }

    public ICommand OpenTodayCommand { get; }

    public ICommand OpenTasksCommand { get; }

    public ICommand OpenScheduleCommand { get; }

    public ICommand OpenSettingsCommand { get; }

    public ICommand CloseWorkspaceCommand { get; }

    public ICommand NewConversationCommand { get; }

    public ICommand CancelResponseCommand { get; }

    public ICommand ClearErrorCommand { get; }

    public string DisplayDate => DateTime.Now.ToString("dddd, d MMMM");

    public string Greeting
    {
        get
        {
            var hour = DateTime.Now.Hour;
            return hour switch
            {
                < 12 => "Good morning",
                < 18 => "Good afternoon",
                _ => "Good evening",
            };
        }
    }

    public string ConnectionStatus => $"Phase 1 · {_chatCompletionService.DisplayName}";

    public bool ReduceMotion => _appEnvironment.Info.ReduceMotion;

    public string NotificationCount => Notifications.Count.ToString();

    public bool IsConversationEmpty => Messages.Count == 0;

    public bool HasMessages => Messages.Count > 0;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public string ContextLabel =>
        ActiveWorkspace is null ? "Assistant workspace" : ActiveWorkspace.Kicker;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string _composerText = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isWorkspaceOpen;

    [ObservableProperty]
    private WorkspaceViewModel? _activeWorkspace;

    [ObservableProperty]
    private string _conversationTitle = NewConversationTitle;

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            var recentConversations = await _conversationService.GetRecentAsync();
            var recent = recentConversations.FirstOrDefault();
            if (recent is null)
            {
                return;
            }

            _conversation = recent;
            ConversationTitle = recent.Title;

            var messages = await _conversationService.GetMessagesAsync(recent);
            foreach (var message in messages)
            {
                Messages.Add(new ChatMessageViewModel(
                    message.Role,
                    message.Content,
                    message.CreatedAt,
                    reduceMotion: ReduceMotion));
            }

            _logger.LogInformation(
                "Restored conversation {ConversationId} with {MessageCount} messages",
                recent.Id,
                messages.Count);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not restore the most recent conversation");
            ErrorMessage = "The previous conversation could not be loaded. You can still start a new one.";
        }
    }

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        var content = ComposerText.Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        ErrorMessage = null;
        ComposerText = string.Empty;
        IsBusy = true;

        var userMessage = new ChatMessageViewModel(
            ChatRole.User,
            content,
            DateTimeOffset.UtcNow,
            reduceMotion: ReduceMotion);
        var assistantMessage = new ChatMessageViewModel(
            ChatRole.Assistant,
            string.Empty,
            DateTimeOffset.UtcNow,
            reduceMotion: ReduceMotion,
            isStreaming: true);

        Messages.Add(userMessage);
        Messages.Add(assistantMessage);

        _sendCancellation = new CancellationTokenSource();

        try
        {
            _conversation ??= await _conversationService.CreateAsync(_sendCancellation.Token);

            var progress = new InlineProgress<string>(assistantMessage.Append);
            var result = await _conversationService.SendAsync(
                _conversation,
                content,
                progress,
                _sendCancellation.Token);

            assistantMessage.Complete(result.Message.Content, result.Message.CreatedAt);
            _conversation = _conversation with
            {
                Title = result.ConversationTitle,
                UpdatedAt = result.Message.CreatedAt,
            };
            ConversationTitle = result.ConversationTitle;
        }
        catch (OperationCanceledException)
        {
            if (string.IsNullOrWhiteSpace(assistantMessage.Content))
            {
                Messages.Remove(assistantMessage);
            }
            else
            {
                assistantMessage.Stop($"{assistantMessage.Content.TrimEnd()}\n\n[Response stopped]");
            }

            ErrorMessage = "Response stopped. Your message remains saved locally.";
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Chat response failed");

            if (string.IsNullOrWhiteSpace(assistantMessage.Content))
            {
                Messages.Remove(assistantMessage);
            }
            else
            {
                assistantMessage.Stop($"{assistantMessage.Content.TrimEnd()}\n\n[Response interrupted]");
            }

            ErrorMessage = "I couldn't complete that response. Check the application log for details.";
        }
        finally
        {
            _sendCancellation.Dispose();
            _sendCancellation = null;
            IsBusy = false;
            OnPropertyChanged(nameof(IsConversationEmpty));
            OnPropertyChanged(nameof(HasMessages));
        }
    }

    private bool CanSend() => !IsBusy && !string.IsNullOrWhiteSpace(ComposerText);

    private void CancelResponse() => _sendCancellation?.Cancel();

    private void StartNewConversation()
    {
        if (IsBusy)
        {
            return;
        }

        _conversation = null;
        Messages.Clear();
        ConversationTitle = NewConversationTitle;
        ErrorMessage = null;
        CloseWorkspace();
    }

    private void OpenToday() => ShowWorkspace(
        new TodayWorkspaceViewModel(OpenTasks, OpenSchedule));

    private void OpenTasks() => ShowWorkspace(new TasksWorkspaceViewModel());

    private void OpenSchedule() => ShowWorkspace(new ScheduleWorkspaceViewModel());

    private void OpenSettings() => ShowWorkspace(
        new SettingsWorkspaceViewModel(
            _appEnvironment.Info,
            _chatCompletionService.DisplayName));

    private void ShowWorkspace(WorkspaceViewModel workspace)
    {
        ActiveWorkspace = workspace;
        IsWorkspaceOpen = true;
        OnPropertyChanged(nameof(ContextLabel));
    }

    private void CloseWorkspace()
    {
        IsWorkspaceOpen = false;
        ActiveWorkspace = null;
        OnPropertyChanged(nameof(ContextLabel));
    }

    partial void OnErrorMessageChanged(string? value) =>
        OnPropertyChanged(nameof(HasError));

    partial void OnActiveWorkspaceChanged(WorkspaceViewModel? value) =>
        OnPropertyChanged(nameof(ContextLabel));

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _sendCancellation?.Cancel();
        _sendCancellation?.Dispose();
    }

    private sealed class InlineProgress<T>(Action<T> handler) : IProgress<T>
    {
        public void Report(T value) => handler(value);
    }
}
