using LedStripeWithSensors.MqttManager;
using Microsoft.Extensions.Options;
using LedStripeWithSensors.MotionSensor;
using Animations1d;
using Microsoft.Extensions.Logging;

namespace LedStripeWithSensors.AnimationManager;

internal enum AnimationState
{
    Stopped = 0,
    OverrideLeftRunning = 1,
    OverrideRightRunning = 2,
    MovementLeftRunning = 3,
    MovementRightRunning = 4,
}

internal sealed class AnimationManager
{
    private const string MessageOverrideLeft = "LEFT";
    private const string MessageOverrideRight = "RIGHT";
    private readonly bool _verbose = false;
    private readonly int _frameDurationMs;
    private readonly int _switchOffDelayMs;
    private readonly int _leftMotionDetectorPin;
    private readonly int _rightMotionDetectorPin;
    private readonly IOptions<AnimationManagerConfig> _animationManagerConfig;
    private readonly AnimationFactory _animationFactory;
    private readonly MqttClient _mqttClient;
    private readonly ILogger<AnimationManager> _logger;

    private bool MovementLeft { get; set; } = false;
    private bool MovementRight { get; set; } = false;
    private bool OverrideLeft { get; set; } = false;
    private bool OverrideRight { get; set; } = false;

    private AnimationState State { get; set; } = AnimationState.Stopped;
    private DateTime AnimationStart { get; set; } = DateTime.Now;

    public AnimationManager(IOptions<MotionSensorsConfig> motionSensorsConfig,
        IOptions<AnimationManagerConfig> animationManagerConfig,
        AnimationFactory animationFactory,
        MqttClient mqttClient,
        ILogger<AnimationManager> logger)
    {
        _leftMotionDetectorPin = motionSensorsConfig.Value.LeftMotionDetectorPin;
        _rightMotionDetectorPin = motionSensorsConfig.Value.RightMotionDetectorPin;
        _frameDurationMs = animationManagerConfig.Value.FrameDurationMs;
        _switchOffDelayMs = animationManagerConfig.Value.SwitchOffDelaySec * 1000;
        _animationFactory = animationFactory;
        _mqttClient = mqttClient;
        _logger = logger;
    }

    public async Task Run(CancellationToken ct)
    {
        DateTime now = DateTime.Now;

        _logger.LogTrace("Started at {startTime}", now);


        using var motionDetectorLeft = MotionSensor.MotionSensor.CreateSensor(_leftMotionDetectorPin,
            () => 
            {
                MovementLeft = true;
                _logger.LogInformation("Motion detected: {direction}", "LEFT");
            },
            () =>
            {
                MovementLeft = false;
                _logger.LogInformation("Motion stopped: {direction}", "LEFT");
            });

        try
        {
            motionDetectorLeft.Run();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LEFT motion detector disabled");
        }

        using var motionDetectorRight = MotionSensor.MotionSensor.CreateSensor(_rightMotionDetectorPin,
            () =>
            {
                MovementRight = true;
                _logger.LogInformation("Motion detected: {direction}", "RIGHT");
            },
            () =>
            {
                MovementRight = false;
                _logger.LogInformation("Motion stopped: {direction}", "RIGHT");
            });
        try
        {
            motionDetectorRight.Run();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RIGHT motion detector disabled");
        }

        using var animation = _animationFactory.GetAnimation(typeof(TraceAnimation));

        _mqttClient.Connect(
            () => 
            { 
                OverrideLeft = true; 
                OverrideRight = false;
                _logger.LogInformation("Override signal: {direction}", "LEFT");
            },
            () => 
            { 
                OverrideRight = true; 
                OverrideLeft = false;
                _logger.LogInformation("Override signal: {direction}", "RIGHT");
            },
            ct);

        while (!ct.IsCancellationRequested)
        {
            now = DateTime.Now;

            if (OverrideLeft)
            {
                animation.Start(Direction.LEFT);
                State = AnimationState.OverrideLeftRunning;
                AnimationStart = now;
                OverrideLeft = false;
            }
            else
            if (OverrideRight)
            {
                animation.Start(Direction.RIGHT);
                State = AnimationState.OverrideRightRunning;
                AnimationStart = now;
                OverrideRight = false;
            }

            if (State == AnimationState.OverrideLeftRunning || State == AnimationState.OverrideRightRunning)
            {
                var timeLeftMs = (int)(AnimationStart - now).TotalMilliseconds + _switchOffDelayMs;
                if (timeLeftMs > 0)
                {
                    animation.NextFrame();
                    await Task.Delay(Math.Min(timeLeftMs, _frameDurationMs));
                    continue;
                }
                animation.Stop();
                State = AnimationState.Stopped;
            }

            if ((State == AnimationState.MovementLeftRunning && MovementLeft) || (State == AnimationState.MovementRightRunning && MovementRight))
            {
                AnimationStart = now;
            }

            if (State == AnimationState.MovementLeftRunning || State == AnimationState.MovementRightRunning)
            {
                var timeLeftMs = (int)(AnimationStart - now).TotalMilliseconds + _switchOffDelayMs;
                if (timeLeftMs > 0)
                {
                    animation.NextFrame();
                    await Task.Delay(Math.Min(timeLeftMs, _frameDurationMs));
                    continue;
                }
                animation.Stop();
                State = AnimationState.Stopped;
            }

            if (MovementLeft)
            {
                animation.Start(Direction.LEFT);
                State = AnimationState.MovementLeftRunning;
                _mqttClient.Send(MessageOverrideLeft);
                AnimationStart = now;
            }
            else
            if (MovementRight)
            {
                animation.Start(Direction.RIGHT);
                State = AnimationState.MovementRightRunning;
                _mqttClient.Send(MessageOverrideRight);
                AnimationStart = now;
            }

        }
    }
}
