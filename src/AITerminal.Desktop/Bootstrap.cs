using AITerminal.Infrastructure;
using AITerminal.UI.ViewModels;
using AITerminal.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AITerminal.Desktop;

internal static class Bootstrap
{
    public static ServiceProvider BuildServices(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("AITERMINAL_")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(ReadMinimumLogLevel(configuration));
            builder.AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        });

        var dataDirectory = ResolveDataDirectory(configuration["Storage:DataDirectory"]);
        var reduceMotion = TryReadBoolean(configuration["Accessibility:ReduceMotion"]);
        services.AddAITerminalInfrastructure(dataDirectory, reduceMotion);
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true,
            });
    }

    private static string ResolveDataDirectory(string? configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.LocalApplicationData),
                "AITerminal");
        }

        if (configuredPath.StartsWith("~/", StringComparison.Ordinal))
        {
            return Path.Combine(
                System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.UserProfile),
                configuredPath[2..]);
        }

        return configuredPath;
    }

    private static bool TryReadBoolean(string? value) =>
        bool.TryParse(value, out var result) && result;

    private static LogLevel ReadMinimumLogLevel(IConfiguration configuration) =>
        Enum.TryParse<LogLevel>(
            configuration["Logging:LogLevel:Default"],
            ignoreCase: true,
            out var configured)
            ? configured
            : LogLevel.Information;
}
