using Iot.Device.Ws28xx;
using System.Drawing;

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
        SetAllBlack();
        var x = FrameNumber % Length;
        Neopixel.Image.SetPixel(x, 0, System.Drawing.Color.DarkOrange);
        Neopixel.Update();

    }

    private void SetAllBlack()
    {
        for (int i = 0; i < Length; i++)
        {
            Neopixel.Image.SetPixel(i, 0, Color.Black);
        }
    }
}

