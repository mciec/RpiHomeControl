using Animations1d.Display;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Animations1d;

public class AnimationFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<AnimationsConfig> _animationsConfig;
    private readonly IDisplay _display;
    private readonly ILogger<AnimationFactory> _logger;

    public AnimationFactory(IServiceProvider serviceProvider, IOptions<AnimationsConfig> animationsConfig, IDisplay display, ILogger<AnimationFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _animationsConfig = animationsConfig;
        _display = display;
        _logger = logger;
    }

    public IAnimation GetAnimation(Type type)
    {
        //if (type == typeof(FlyingBallsAnimation))
        //{
        //    return FlyingBallsAnimation.Create(_animationsConfig.Value.FlyingBallsAnimation, _display, _logger);
        //}
        //if (type == typeof(WavesAnimation))
        //{
        //    return WavesAnimation.Create(_animationsConfig.Value.FlyingBallsAnimation, _display, _logger);
        //}
        //if (type == typeof(TraceAnimation))
        //{
        //    return TraceAnimation.Create(_animationsConfig.Value.FlyingBallsAnimation, _display, _logger);
        //}
        if (type == typeof(FlyingBallsAnimation))
        {
            return _serviceProvider.GetKeyedService<IAnimation>("FlyingBalls");
        }
        if (type == typeof(WavesAnimation))
        {
            return _serviceProvider.GetKeyedService<IAnimation>("Waves");
        }
        if (type == typeof(TraceAnimation))
        {
            return _serviceProvider.GetKeyedService<IAnimation>("Trace");
        }

        throw new Exception($"Unknown IAnimation: {type.Name}");
    }
}
