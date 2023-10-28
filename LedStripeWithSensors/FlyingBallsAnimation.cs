using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Device.Spi;
using System.Drawing;
using Iot.Device.Graphics;
using Iot.Device.Ws28xx;


namespace LedStripeWithSensors;

internal sealed class FlyingBallsAnimation : IAnimation
{
    private readonly int _length;
    private readonly int _staticBallsCount;
    private readonly int _movingBallsCount;
    private readonly Ws2812b _neopixel;

    private FlyingBallsAnimation(int length, int staticBallsCount, int movingBallsCount, Ws2812b neopixel)
    {
        _length = length;
        _staticBallsCount = staticBallsCount;
        _movingBallsCount = movingBallsCount;
        _neopixel = neopixel;
    }

    public static FlyingBallsAnimation Create(Ws2812b neopixel, int staticBallsCount = 4, int movingBallsCount = 3)
    {
        return new FlyingBallsAnimation(neopixel.Image.Width, staticBallsCount, movingBallsCount, neopixel);
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void NextFrame()
    {

    }

    public void Start(Direction direction)
    {
        throw new NotImplementedException();
    }
}

