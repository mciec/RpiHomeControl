using Microsoft.Extensions.DependencyInjection;

namespace Animations1d;

public static class AnimationFactoryHelper
{
    public static IServiceCollection AddAnimations(this IServiceCollection services)
    {
        services.AddKeyedSingleton<IAnimation, FlyingBallsAnimation>("FlyingBalls");
        services.AddKeyedSingleton<IAnimation, TraceAnimation>("Trace");
        services.AddKeyedSingleton<IAnimation, WavesAnimation>("Waves");

        return services;
    }
}
