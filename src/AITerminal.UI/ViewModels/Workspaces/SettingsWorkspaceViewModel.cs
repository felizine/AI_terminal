using AITerminal.Core.Environment;

namespace AITerminal.UI.ViewModels.Workspaces;

public sealed class SettingsWorkspaceViewModel(
    AppEnvironmentInfo environment,
    string inferenceStatus) : WorkspaceViewModel
{
    public override string Kicker => "SETTINGS";

    public override string Title => "Local environment";

    public override string Description =>
        "Phase 1 configuration is visible here. Provider and connector settings are added in Phase 2.";

    public string DataDirectory { get; } = environment.DataDirectory;

    public string CredentialBackend { get; } = environment.CredentialBackend;

    public string InferenceStatus { get; } = inferenceStatus;

    public string MotionStatus { get; } =
        environment.ReduceMotion ? "Reduced motion" : "Full motion";
}
