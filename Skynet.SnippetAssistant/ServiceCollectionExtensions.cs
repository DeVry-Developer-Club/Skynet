namespace Skynet.SnippetAssistant;
using Microsoft.Extensions.DependencyInjection;
using Interfaces;
using Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the Snippet Assistant related services to the <paramref name="services"/> collection
    /// </summary>
    /// <param name="services"></param>
    /// <returns>Chainable reference</returns>
    public static IServiceCollection AddSnippetAssistanceServices(this IServiceCollection services)
    {
        services.AddSingleton<ISnippetStorageService, SnippetStorageService>();
        services.AddSingleton<ICodeReviewService, CodeReviewService>();

        return services;
    }
}
