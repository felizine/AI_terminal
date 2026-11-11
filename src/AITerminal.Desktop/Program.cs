using AITerminal.Infrastructure.Persistence;
using AITerminal.UI;
using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AITerminal.Desktop;

internal static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        using var services = Bootstrap.BuildServices(args);
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");

        try
        {
            var database = services.GetRequiredService<SqliteDatabase>();
            database.InitializeAsync().GetAwaiter().GetResult();
            logger.LogInformation("Local database ready at {DatabasePath}", database.DatabasePath);

            var exitCode = BuildAvaloniaApp(services)
                .StartWithClassicDesktopLifetime(args);

            logger.LogInformation("AI Terminal shut down cleanly");
            return exitCode;
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "AI Terminal stopped unexpectedly");
            return 1;
        }
    }

    public static AppBuilder BuildAvaloniaApp(IServiceProvider services) =>
        AppBuilder.Configure(() => new App(services))
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();
}
