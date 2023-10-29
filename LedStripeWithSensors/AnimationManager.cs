using Iot.Device.Ws28xx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using Iot.Device.Bno055;

namespace LedStripeWithSensors;

internal enum AnimationState
{
    Stopped = 0,
    OverrideLeftRunning = 1,
    OverrideRightRunning = 2,
    MovementLeftRunning = 3,
    MovementRightRunning = 4,
}

internal sealed class AnimationManager : IAsyncDisposable
{
    private const int TimeGraularityMs = 1000;
    private readonly int _leftMotionDetectorPin;
    private readonly int _rightMotionDetectorPin;
    private readonly IAnimation _animation;
    private bool MovementLeft { get; set; } = false;
    private bool MovementRight { get; set; } = false;
    private bool OverrideLeft { get; set; } = false;
    private bool OverrideRight { get; set; } = false;

    private AnimationState State { get; set; } = AnimationState.Stopped;
    private DateTime AnimationStart { get; set; } = DateTime.Now;

    public AnimationManager(int leftMotionDetectorPin, int rightMotionDetectorPin, IAnimation animation)
    {
        _leftMotionDetectorPin = leftMotionDetectorPin;
        _rightMotionDetectorPin = rightMotionDetectorPin;
        _animation = animation;
    }

    public async Task Run(CancellationToken ct)
    {
        DateTime now = DateTime.Now;
        using var motionDetectorLeft = MotionDetector.CreateDetector(_leftMotionDetectorPin,
            () => MovementLeft = true,
            () => MovementLeft = false);
        using var motionDetectorRight = MotionDetector.CreateDetector(_rightMotionDetectorPin,
            () => MovementRight = true,
            () => MovementRight = false);

        _neopixel.




        while (!ct.IsCancellationRequested)
        {
            now = DateTime.Now;

            if (OverrideLeft)
            {
                _animation.Clear();
                _animation.Start(Direction.LEFT);
                State = AnimationState.OverrideLeftRunning;
                AnimationStart = now; ;
            }
            else
            if (OverrideRight)
            {
                _animation.Clear();
                _animation.Start(Direction.RIGHT);
                State = AnimationState.OverrideRightRunning;
                AnimationStart = now;
            }


            if (State == AnimationState.OverrideLeftRunning || State == AnimationState.OverrideLeftRunning)
            {
                var timeLeftMs = (int)(now - AnimationStart).TotalMilliseconds;
                if (timeLeftMs > 0)
                {
                    _animation.NextFrame();
                    await Task.Delay(Math.Min(timeLeftMs, TimeGraularityMs));
                    continue;
                }
                _animation.Clear();
                State = AnimationState.Stopped;
            }

            if ((State == AnimationState.MovementLeftRunning && MovementLeft) || (State == AnimationState.MovementRightRunning && MovementRight))
            {
                AnimationStart = now;
            }

            if (State == AnimationState.MovementLeftRunning || State == AnimationState.MovementRightRunning)
            {
                var timeLeftMs = (int)(now - AnimationStart).TotalMilliseconds;
                if (timeLeftMs > 0)
                {
                    _animation.NextFrame();
                    await Task.Delay(Math.Min(timeLeftMs, TimeGraularityMs));
                    continue;
                }
                _animation.Clear();
                State = AnimationState.Stopped;
            }

            if (MovementLeft)
            {
                _animation.Clear();
                _animation.Start(Direction.LEFT);
                State = AnimationState.MovementLeftRunning;
                AnimationStart = now;

            }
            else
            if (MovementRight)
            {
                _animation.Clear();
                _animation.Start(Direction.RIGHT);
                State = AnimationState.MovementRightRunning;
                AnimationStart = now;
            }

        }


    }





    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
