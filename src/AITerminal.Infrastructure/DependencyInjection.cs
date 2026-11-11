using AITerminal.Core.Conversations;
using AITerminal.Core.Environment;
using AITerminal.Core.Security;
using AITerminal.Infrastructure.Persistence;
using AITerminal.Infrastructure.Security;
using AITerminal.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AITerminal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAITerminalInfrastructure(
        this IServiceCollection services,
        string dataDirectory,
        bool reduceMotion = false)
    {
        services.AddSingleton(new StorageOptions(dataDirectory));
        services.AddSingleton<SqliteDatabase>();
        services.AddSingleton<IConversationRepository, SqliteConversationRepository>();
        services.AddSingleton<IChatCompletionService, OfflineChatCompletionService>();
        services.AddSingleton<ConversationService>();
        services.AddSingleton<ICredentialStore>(serviceProvider =>
        {
            if (SecretToolCredentialStore.IsAvailable)
            {
                return new SecretToolCredentialStore(
                    serviceProvider
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger<SecretToolCredentialStore>());
            }

            return new UnavailableCredentialStore();
        });
        services.AddSingleton<IAppEnvironment>(serviceProvider =>
            new AppEnvironment(
                serviceProvider.GetRequiredService<StorageOptions>(),
                serviceProvider.GetRequiredService<ICredentialStore>(),
                reduceMotion));

        return services;
    }
}
