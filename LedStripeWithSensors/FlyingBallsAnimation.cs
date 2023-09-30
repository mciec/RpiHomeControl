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

internal class FlyingBallsAnimation : IAnimation
{
    private readonly int _length;
    private readonly int _staticBallsCount;
    private readonly int _movingBallsCount;

    public FlyingBallsAnimation(int length = 100, int staticBallsCount = 4, int movingBallsCount = 3)
    {
        _length = length;
        _staticBallsCount = staticBallsCount;
        _movingBallsCount = movingBallsCount;
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void NextFrame()
    {

    }

    public void Start(AnimationDirection direction)
    {
        throw new NotImplementedException();
    }
}

