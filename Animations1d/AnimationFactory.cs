using Animations1d.Display;
using Microsoft.Extensions.Options;

namespace Animations1d;

public class AnimationFactory
{
    private readonly IOptions<AnimationsConfig> _animationsConfig;
    private readonly IDisplay _display;

    public AnimationFactory(IOptions<AnimationsConfig> animationsConfig, IDisplay display)
    {
        _animationsConfig = animationsConfig;
        _display = display;
    }
    public IAnimation GetAnimation(Type type)
    {
        if (type == typeof(FlyingBallsAnimation))
        {
            return FlyingBallsAnimation.Create(_animationsConfig.Value.FlyingBallsAnimation, _display);
        }
        if (type == typeof(ShootingLaserAnimation))
        {
            return ShootingLaserAnimation.Create(_animationsConfig.Value.FlyingBallsAnimation, _display);
        }

        throw new Exception($"Unknown IAnimation: {type.Name}");
    }
}
