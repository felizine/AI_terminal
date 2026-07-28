using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AI_terminal.Models;
namespace AI_terminal.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // public string Greeting { get; } = "Welcome to Avalonia!";
    private readonly TerminalService _terminalService = new();

    // This attribute automatically creates a "StatusText" property behind the scenes
    [ObservableProperty]
    private string _statusText = "Initializing...";

    // This attribute turns this method into an executable UI button command
    [RelayCommand]
    private void RefreshStatus()
    {
        StatusText = _terminalService.GetLatestStatus();
    }
}