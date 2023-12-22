using Iot.Device.Ws28xx;

namespace LedStripeWithSensors.Animations;

internal abstract class AnimationBase : IAnimation
{
    private readonly bool _verbose = false;
    protected readonly Ws2812b Neopixel;
    protected readonly int Length;
    protected int FrameNumber;
    protected Direction Direction = Direction.NONE;

    protected AnimationBase(Ws2812b neopixel)
    {
        Neopixel = neopixel;
        //TODO: remove
        Length = neopixel?.Image.Width ?? 0;
    }

    public void Stop()
    {
        Direction = Direction.NONE;
        FrameNumber = 0;
        //TODO: fix
        Neopixel?.Image.Clear();
        Neopixel?.Update();
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
