namespace AITerminal.Core.Environment;

public sealed record AppEnvironmentInfo(
    string DataDirectory,
    string CredentialBackend,
    bool ReduceMotion);

public interface IAppEnvironment
{
    AppEnvironmentInfo Info { get; }
}
