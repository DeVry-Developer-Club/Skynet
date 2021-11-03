using Skynet.ImageCreator.Interfaces;
using Skynet.ImageCreator.Services;

namespace Skynet.ImageCreator;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddImageCreatorServices(this IServiceCollection services)
    {
        services.AddSingleton<IImageService, UnsplashImageService>();
        return services;
    }
}
