using System.ComponentModel;
using System.Diagnostics;
using AITerminal.Core.Security;
using Microsoft.Extensions.Logging;

namespace AITerminal.Infrastructure.Security;

public sealed class SecretToolCredentialStore(ILogger<SecretToolCredentialStore> logger)
    : ICredentialStore
{
    private const string ExecutableName = "secret-tool";
    private const string ApplicationAttribute = "ai-terminal";

    public string BackendName => "Linux Secret Service (secret-tool)";

    public static bool IsAvailable => FindOnPath(ExecutableName) is not null;

    public async Task<string?> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var result = await RunAsync(
            ["lookup", "application", ApplicationAttribute, "key", key],
            input: null,
            cancellationToken);

        if (result.ExitCode == 1)
        {
            return null;
        }

        ThrowIfFailed(result, "read", key);
        return result.StandardOutput.TrimEnd('\r', '\n');
    }

    public async Task SetAsync(
        string key,
        string secret,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ArgumentException.ThrowIfNullOrEmpty(secret);

        var result = await RunAsync(
            [
                "store",
                "--label",
                "AI Terminal credential",
                "application",
                ApplicationAttribute,
                "key",
                key,
            ],
            secret,
            cancellationToken);

        ThrowIfFailed(result, "save", key);
        logger.LogInformation("Stored credential {CredentialKey} in the system keyring", key);
    }

    public async Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);

        var result = await RunAsync(
            ["clear", "application", ApplicationAttribute, "key", key],
            input: null,
            cancellationToken);

        if (result.ExitCode is not (0 or 1))
        {
            ThrowIfFailed(result, "delete", key);
        }

        logger.LogInformation("Removed credential {CredentialKey} from the system keyring", key);
    }

    internal static bool IsValidKey(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
        {
            return false;
        }

        return key.All(character =>
            char.IsAsciiLetterOrDigit(character) ||
            character is '-' or '_' or '.');
    }

    private static void ValidateKey(string key)
    {
        if (!IsValidKey(key))
        {
            throw new ArgumentException(
                "Credential keys may only contain letters, digits, dots, hyphens, and underscores.",
                nameof(key));
        }
    }

    private static async Task<ProcessResult> RunAsync(
        IReadOnlyList<string> arguments,
        string? input,
        CancellationToken cancellationToken)
    {
        var executable = FindOnPath(ExecutableName)
            ?? throw new InvalidOperationException(
                "secret-tool is unavailable. Install libsecret and start a Secret Service provider.");

        var startInfo = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = new Process { StartInfo = startInfo };
        try
        {
            process.Start();

            var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

            if (input is not null)
            {
                await process.StandardInput.WriteAsync(input.AsMemory(), cancellationToken);
            }

            process.StandardInput.Close();
            await process.WaitForExitAsync(cancellationToken);

            return new ProcessResult(
                process.ExitCode,
                await standardOutput,
                await standardError);
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException(
                "Could not launch secret-tool for credential storage.",
                exception);
        }
        catch
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }

            throw;
        }
    }

    private static string? FindOnPath(string executableName)
    {
        var path = System.Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        foreach (var directory in path.Split(Path.PathSeparator))
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                continue;
            }

            var candidate = Path.Combine(directory, executableName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void ThrowIfFailed(ProcessResult result, string operation, string key)
    {
        if (result.ExitCode == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            $"The system keyring could not {operation} credential '{key}'. " +
            $"secret-tool exited with code {result.ExitCode}: {result.StandardError.Trim()}");
    }

    private sealed record ProcessResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);
}
