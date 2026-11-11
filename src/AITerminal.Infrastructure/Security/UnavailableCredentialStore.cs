using AITerminal.Core.Security;

namespace AITerminal.Infrastructure.Security;

public sealed class UnavailableCredentialStore : ICredentialStore
{
    public string BackendName => "Unavailable (install libsecret and secret-tool)";

    public Task<string?> GetAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);

    public Task SetAsync(
        string key,
        string secret,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            "Secure credential storage is unavailable. " +
            "Install libsecret and ensure a Secret Service provider is running.");

    public Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
