using AITerminal.Infrastructure.Security;
using Microsoft.Extensions.Logging.Abstractions;

namespace AITerminal.Infrastructure.Tests;

public sealed class CredentialStoreTests
{
    [Theory]
    [InlineData("")]
    [InlineData("../escape")]
    [InlineData("contains space")]
    [InlineData("contains/slash")]
    public async Task Secret_tool_store_rejects_invalid_credential_keys(string key)
    {
        var store = new SecretToolCredentialStore(
            NullLogger<SecretToolCredentialStore>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() => store.GetAsync(key));
    }
}
