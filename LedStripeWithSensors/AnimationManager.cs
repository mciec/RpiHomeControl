using Iot.Device.Ws28xx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;

namespace LedStripeWithSensors;

internal class AnimationManager : IAsyncDisposable
{
    private readonly int _leftMotionDetectorPin;
    private readonly int _rightMotionDetectorPin;
    private readonly Ws2812b _neopixel;
    private readonly IAnimation _animation;
    private bool MovementLeft { get; set; } = false;
    private bool MovementRight { get; set; } = false;
    private bool OverrideLeft { get; set; } = false;
    private bool OverrideRight { get; set; } = false;



    public AnimationManager(int leftMotionDetectorPin, int rightMotionDetectorPin, Ws2812b neopixel, IAnimation animation)
    {
        _leftMotionDetectorPin = leftMotionDetectorPin;
        _rightMotionDetectorPin = rightMotionDetectorPin;
        _neopixel = neopixel;
        _animation = animation;
    }

    public async Task Run(CancellationToken ct)
    {
        using var motionDetectorLeft = MotionDetector.CreateDetector(_leftMotionDetectorPin,
            () => MovementLeft = true,
            () => MovementLeft = false);
        using var motionDetectorRight = MotionDetector.CreateDetector(_rightMotionDetectorPin,
            () => MovementRight = true,
            () => MovementRight = false);
        
        while (!ct.IsCancellationRequested) 
        {
            
        
        
        
        }


    }





    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
