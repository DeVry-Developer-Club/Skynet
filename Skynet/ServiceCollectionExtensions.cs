using DisCatSharp.Hosting;
using DisCatSharp.Hosting.DependencyInjection;
using Microsoft.Extensions.Options;
using Skynet.Core.Interfaces;
using Skynet.Core.Options;
using Skynet.Core.Services;
using Skynet.ImageCreator;
using Skynet.Interfaces;
using Skynet.Options;
using Skynet.QuoteService;
using Skynet.Services;
using Skynet.SnippetAssistant;

namespace Skynet.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOptionsFor<T>(this IServiceCollection services, IConfiguration config, string sectionName)
        where T : class
    {
        services.Configure<T>(config.GetSection(sectionName));
        services.AddSingleton(s => s.GetRequiredService<IOptions<T>>().Value);

        return services;
    }

    public static IServiceCollection AddSkynetServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddOptions();

        services.AddOptionsFor<DiscordOptions>(config, "Discord");
        services.AddOptionsFor<WelcomeOptions>(config, "WelcomeSettings");
        services.AddOptionsFor<StorageOptions>(config, "Storage");
        services.AddOptionsFor<ImageSearchOptions>(config, "ImageSearchOptions");
        services.AddDiscordHostedService<IDiscordHostedService, Bot>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<IWelcomeHandler, WelcomeHandler>();
        services.AddSingleton<IRoleService, RoleService>();
        services.AddSingleton<IStorageService, StorageService>();
        services.AddSingleton<EngineersNotebookService>();
        services.AddImageCreatorServices();
        
        services.AddQuoteServices();
        
        services.AddSingleton<QuoteHandler>();

        // Why.. why dotnet 6 must you be like this
        services.AddSnippetAssistanceServices();
        return services;
    }
}
