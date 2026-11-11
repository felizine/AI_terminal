using AITerminal.Core.Environment;
using AITerminal.Core.Security;

namespace AITerminal.Infrastructure;

public sealed class AppEnvironment(
    StorageOptions storage,
    ICredentialStore credentialStore,
    bool reduceMotion) : IAppEnvironment
{
    public AppEnvironmentInfo Info => new(
        storage.DataDirectory,
        credentialStore.BackendName,
        reduceMotion);
}
