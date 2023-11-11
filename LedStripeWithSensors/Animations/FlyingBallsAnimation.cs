using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Spi;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Ws28xx;
using Microsoft.Extensions.Options;

namespace LedStripeWithSensors.Animations;

internal sealed class FlyingBallsAnimation : IAnimation
{
    private readonly int _length;
    private readonly int _staticBallsCount;
    private readonly int _movingBallsCount;
    private readonly Ws2812b _neopixel;

    private int FrameNo { get; set; } = 0;
    private Direction Direction { get; set; } = Direction.LEFT;


    private FlyingBallsAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, Ws2812b neopixel)
    {
        _length = neopixel.Image.Width;
        _staticBallsCount = flyingBallsAnimationConfig.StaticBallsCount;
        _movingBallsCount = flyingBallsAnimationConfig.MovingBallsCount;
        _neopixel = neopixel;
    }

    public static FlyingBallsAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, Ws2812b neopixel)
    {
        return new FlyingBallsAnimation(flyingBallsAnimationConfig, neopixel);
    }

    public void Clear()
    {
        Direction = Direction.NONE;
        FrameNo = 0;
        PrintStatus();
    }

    public void NextFrame()
    {
        FrameNo++;
        PrintStatus();
    }

    public void Start(Direction direction)
    {
        Direction = direction;
        FrameNo = 0;
        PrintStatus();
    }

    private void PrintStatus()
    {
        Console.WriteLine($"Animation: {Direction} - Frame {FrameNo}");
    }

    public void Dispose()
    {
        Console.WriteLine($"FlyingBallsAnimation disposed");
    }
}

