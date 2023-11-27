using Iot.Device.Ws28xx;

namespace LedStripeWithSensors.Animations;

internal sealed class FlyingBallsAnimation : AnimationBase
{
    private readonly int _staticBallsCount;
    private readonly int _movingBallsCount;

    private FlyingBallsAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, Ws2812b neopixel) : base(neopixel)
    {
        _staticBallsCount = flyingBallsAnimationConfig.StaticBallsCount;
        _movingBallsCount = flyingBallsAnimationConfig.MovingBallsCount;
    }

    public static FlyingBallsAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, Ws2812b neopixel)
    {
        return new FlyingBallsAnimation(flyingBallsAnimationConfig, neopixel);
    }

    public override void Dispose()
    {
        Console.WriteLine($"FlyingBallsAnimation disposed");
    }

    protected override void GenerateNextFrame()
    {

    }
}

