using Iot.Device.Ws28xx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HiveMQtt.Client;
using Iot.Device.Bno055;
using HiveMQtt.MQTT5.Exceptions;
using LedStripeWithSensors.MqttManager;
using Microsoft.Extensions.Options;
using LedStripeWithSensors.Animations;

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
    private const string MessageOverrideLeft = "LEFT";
    private const string MessageOverrideRight = "RIGHT";
    private const int TimeGraularityMs = 1000;
    private readonly int _leftMotionDetectorPin;
    private readonly int _rightMotionDetectorPin;
    private readonly AnimationFactory _animationFactory;
    private readonly MqttClient _mqttClient;
    private bool MovementLeft { get; set; } = false;
    private bool MovementRight { get; set; } = false;
    private bool OverrideLeft { get; set; } = false;
    private bool OverrideRight { get; set; } = false;

    private AnimationState State { get; set; } = AnimationState.Stopped;
    private DateTime AnimationStart { get; set; } = DateTime.Now;

    public AnimationManager(IOptions<MotionSensorsConfig> motionSensorsConfig, AnimationFactory animationFactory, MqttClient mqttClient)
    {
        _leftMotionDetectorPin = motionSensorsConfig.Value.LeftMotionDetectorPin;
        _rightMotionDetectorPin = motionSensorsConfig.Value.RightMotionDetectorPin;
        _animationFactory = animationFactory;
        _mqttClient = mqttClient;
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

        using var animation = _animationFactory.GetAnimation(typeof(FlyingBallsAnimation));


        _mqttClient.Connect(
            () => { OverrideLeft = true; OverrideRight = false; },
            () => { OverrideRight = true; OverrideLeft = false; },
            ct);

        while (!ct.IsCancellationRequested)
        {
            now = DateTime.Now;

            if (OverrideLeft)
            {
                animation.Clear();
                animation.Start(Direction.LEFT);
                State = AnimationState.OverrideLeftRunning;
                AnimationStart = now; ;
            }
            else
            if (OverrideRight)
            {
                animation.Clear();
                animation.Start(Direction.RIGHT);
                State = AnimationState.OverrideRightRunning;
                AnimationStart = now;
            }


            if (State == AnimationState.OverrideLeftRunning || State == AnimationState.OverrideLeftRunning)
            {
                var timeLeftMs = (int)(now - AnimationStart).TotalMilliseconds;
                if (timeLeftMs > 0)
                {
                    animation.NextFrame();
                    await Task.Delay(Math.Min(timeLeftMs, TimeGraularityMs));
                    continue;
                }
                animation.Clear();
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
                    animation.NextFrame();
                    await Task.Delay(Math.Min(timeLeftMs, TimeGraularityMs));
                    continue;
                }
                animation.Clear();
                State = AnimationState.Stopped;
            }

            if (MovementLeft)
            {
                animation.Clear();
                animation.Start(Direction.LEFT);
                State = AnimationState.MovementLeftRunning;
                _mqttClient.Send(MessageOverrideLeft);
                AnimationStart = now;
            }
            else
            if (MovementRight)
            {
                animation.Clear();
                animation.Start(Direction.RIGHT);
                State = AnimationState.MovementRightRunning;
                _mqttClient.Send(MessageOverrideRight);
                AnimationStart = now;
            }

        }
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
