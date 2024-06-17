using Animations1d.Display;
using Microsoft.Extensions.Logging;

namespace Animations1d;

public abstract class AnimationBase : IAnimation
{
    private readonly bool _verbose = true;
    private ILogger<AnimationBase> _logger;
    protected readonly IDisplay Display;
    protected int FrameNumber;
    protected Direction Direction = Direction.NONE;

    protected AnimationBase(IDisplay display, ILogger<AnimationBase> logger)
    {
        Display = display;
        _logger = logger;
    }

    public void Stop()
    {
        Direction = Direction.NONE;
        FrameNumber = 0;
        Display.Reset();
        if (_verbose) PrintStatus();
        _logger.LogInformation("STOP: animation {0}", GetType().Name);
    }

    public void Start(Direction direction)
    {
        Direction = direction;
        FrameNumber = 0;
        if (_verbose) PrintStatus();
        _logger.LogInformation("START: animation {0}", GetType().Name);
    }

    public void NextFrame()
    {
        FrameNumber++;
        GenerateNextFrame();
        if (_verbose) PrintStatus();
        _logger.LogInformation("NextFrame: animation {0}: {1}", GetType().Name, FrameNumber);
    }

    protected abstract void GenerateNextFrame();

    public abstract void Dispose();

    protected virtual void PrintStatus()
    {
        Console.SetCursorPosition(0, 5);
        Console.WriteLine($"{DateTime.Now:yyyyMMdd HH:mm:ss}: [{GetType().Name}]: Dir: {Direction} Frame: {FrameNumber}");
    }
}
