using System.Collections.Specialized;
using System.ComponentModel;
using AITerminal.UI.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;

namespace AITerminal.UI.Views;

public partial class MainWindow : Window
{
    private const double SidebarBreakpoint = 1040;
    private const double ExpandedSidebarBreakpoint = 1400;

    private readonly MainWindowViewModel _viewModel;

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = viewModel;

        Opened += OnOpened;
        Closed += OnClosed;
        SizeChanged += (_, _) => ApplyLayout();
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        _viewModel.Messages.CollectionChanged += OnMessagesCollectionChanged;

        ApplyLayout();
    }

    private async void OnOpened(object? sender, EventArgs e)
    {
        await _viewModel.InitializeAsync();
        ApplyLayout();
        ScrollChatToEnd();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _viewModel.Messages.CollectionChanged -= OnMessagesCollectionChanged;
        _viewModel.Dispose();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MainWindowViewModel.IsWorkspaceOpen)
            or nameof(MainWindowViewModel.ActiveWorkspace))
        {
            ApplyLayout();
        }
    }

    private void OnMessagesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) =>
        Dispatcher.UIThread.Post(ScrollChatToEnd, DispatcherPriority.Background);

    private void OnComposerKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        if (_viewModel.SendCommand.CanExecute(null))
        {
            _viewModel.SendCommand.Execute(null);
        }

        e.Handled = true;
    }

    private void ScrollChatToEnd()
    {
        if (_viewModel.HasMessages)
        {
            ChatScroll.ScrollToEnd();
        }
    }

    private void ApplyLayout()
    {
        var width = ClientSize.Width;
        var showSidebar = width >= SidebarBreakpoint;
        var expandedSidebar = width >= ExpandedSidebarBreakpoint;
        var wideWorkspace = width >= ExpandedSidebarBreakpoint;
        var workspaceOpen = _viewModel.IsWorkspaceOpen;

        var sidebarWidth = showSidebar
            ? expandedSidebar ? 236 : 76
            : 0;

        var showRail = wideWorkspace && !workspaceOpen;
        var railWidth = showRail ? 332 : 0;

        BodyGrid.ColumnDefinitions = new ColumnDefinitions($"{sidebarWidth},*,{railWidth}");
        Sidebar.IsVisible = showSidebar;
        SidebarExpandedContent.IsVisible = showSidebar && expandedSidebar;
        SidebarCompactContent.IsVisible = showSidebar && !expandedSidebar;
        NotificationRail.IsVisible = showRail;
        RailToggleButton.IsVisible = !showRail;
        ConnectionPill.IsVisible = width >= 880;
        BrandSubtitle.IsVisible = width >= 560;
        TopConversationTitle.IsVisible = width >= 780;

        if (!workspaceOpen)
        {
            ChatPanel.IsVisible = true;
            WorkspaceSplitter.IsVisible = false;
            WorkspaceHost.IsVisible = false;
            ContentGrid.ColumnDefinitions = new ColumnDefinitions("*,0,0");
            return;
        }

        WorkspaceHost.IsVisible = true;

        if (wideWorkspace)
        {
            ChatPanel.IsVisible = true;
            WorkspaceSplitter.IsVisible = true;
            Grid.SetColumn(WorkspaceHost, 2);
            ContentGrid.ColumnDefinitions = new ColumnDefinitions(
                "minmax(310px,0.9*),8,minmax(380px,1.1*)");
            return;
        }

        ChatPanel.IsVisible = false;
        WorkspaceSplitter.IsVisible = false;
        Grid.SetColumn(WorkspaceHost, 0);
        ContentGrid.ColumnDefinitions = new ColumnDefinitions("*,0,0");
    }
}
