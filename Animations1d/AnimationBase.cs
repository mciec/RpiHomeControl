using Animations1d.Display;

namespace Animations1d;

public abstract class AnimationBase : IAnimation
{
    private readonly bool _verbose = true;
    protected readonly IDisplay Display;
    protected int FrameNumber;
    protected Direction Direction = Direction.NONE;

    protected AnimationBase(IDisplay display)
    {
        Display = display;
    }

    public void Stop()
    {
        Direction = Direction.NONE;
        FrameNumber = 0;
        Display.Reset();
        if (_verbose) PrintStatus();
    }

    public void Start(Direction direction)
    {
        Direction = direction;
        FrameNumber = 0;
        if (_verbose) PrintStatus();
    }

    public void NextFrame()
    {
        FrameNumber++;
        GenerateNextFrame();
        if (_verbose) PrintStatus();
    }

    protected abstract void GenerateNextFrame();

    public abstract void Dispose();

    protected virtual void PrintStatus() => Console.WriteLine($"{DateTime.Now:yyyyMMdd HH:mm:ss}: [{GetType().Name}]: Dir: {Direction} Frame: {FrameNumber}");
}
