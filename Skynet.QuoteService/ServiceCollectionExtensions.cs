using Microsoft.Extensions.DependencyInjection;

namespace Skynet.QuoteService;
using Interfaces;
using Services;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddQuoteServices(this IServiceCollection services)
    {
        services.AddSingleton<IQuoteService, QuoteService>();
        return services;
    }
}
