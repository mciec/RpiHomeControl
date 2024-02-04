namespace LedStripeWithSensors.Display;

public struct RGB
{
    public byte R;
    public byte G;
    public byte B;
}
public interface IDisplay
{
    RGB[] Matrix { get; }
    public int Width { get; }
    public void Flush();
    public void Reset();
}
