using Animations1d.Display;
using System.Numerics;

namespace Animations1d;

public sealed class ShootingLaserAnimation : AnimationBase
{
    private readonly int _laserBeamsCount;

    private LaserBeam[] LaserBeams { get; set; }


    private static double oneDegree = Math.PI / 180.0;
    private static byte[] SinLookup;
    private static byte[] SinSqrLookup;
    private static byte[] CosLookup;
    private static byte[] CosSqrLookup;

    static ShootingLaserAnimation()
    {
        SinLookup = new byte[360];
        SinSqrLookup = new byte[360];
        CosLookup = new byte[360];
        CosSqrLookup = new byte[360];
        double degree = 0;
        for (int i = 0; i < SinLookup.Length; i++)
        {
            var sinus = Math.Sin(degree);
            SinLookup[i] = (byte)(sinus * 127);
            SinSqrLookup[i] = (byte)(sinus * sinus * 255);
            CosLookup[i] = (byte)(Math.Cos(degree) * 127);
            CosSqrLookup[i] = (byte)(255 - SinSqrLookup[i]);
            degree += oneDegree;
        }

    }

    private ShootingLaserAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display) : base(display)
    {
        _laserBeamsCount = 5;   // flyingBallsAnimationConfig.MovingBallsCount;
        LaserBeams = new LaserBeam[_laserBeamsCount];
        for (int i = 0; i < _laserBeamsCount; i++)
            LaserBeams[i] = new();
    }

    public static ShootingLaserAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display)
    {
        return new ShootingLaserAnimation(flyingBallsAnimationConfig, display);
    }

    public override void Dispose()
    {
        Console.WriteLine($"ShootingLaserAnimation disposed");
    }

    protected override void GenerateNextFrame()
    {
        MoveLaserBeams();
        Display.Flush();
    }

    private void MoveLaserBeams()
    {
        int[] tmpR = new int[Display.Width];
        int[] tmpG = new int[Display.Width];
        int[] tmpB = new int[Display.Width];

        for (int i = 0; i < _laserBeamsCount; i++)
        {
            if (LaserBeams[i].T == 0 || !LaserBeams[i].IsVisible(Display.Width))
            {
                LaserBeams[i].Initialize(0);
                //LaserBeams[i].Initialize(Display.Width);
            }

            LaserBeams[i].T++;

            for (int x = 0; x < Display.Width; x++)
            {
                var color = LaserBeams[i].TraceColor(x);
                tmpR[x] += color.R;
                tmpG[x] += color.G;
                tmpB[x] += color.B;
            }
        }

        for (int x = 0; x < Display.Width; x++)
        {
            Display.Matrix[x] = new RGB(
                tmpR[x] <= 255 ? (byte)tmpR[x] : (byte)255,
                tmpG[x] <= 255 ? (byte)tmpG[x] : (byte)255,
                tmpB[x] <= 255 ? (byte)tmpB[x] : (byte)255
                );
        }
    }
    private int Sinus(int angle) => angle < 0 ? -SinLookup[-angle % 360] : SinLookup[angle % 360];

    private int SinusSqr(int angle) => SinSqrLookup[Math.Abs(angle) % 360];

}

internal class LaserBeam
{
    public RGB Color { get; set; }

    public int X0 { get; set; }
    public int T { get; set; }
    public int V { get; set; }
    public double GlowIntensity(double t) => t < 15 ? 1 - t / 5 : 0;

    public int X => X0 + (T * V);   //constant speed movement
    public double TatX(int x) => (x - X0) / (double)V;
    public int Dir => V > 0 ? 1 : -1;

    public bool IsVisible(int displayWidth)
    {
        double intervalTillNow;
        double glowIntensity;

        if (V > 0)
        {
            var tAtRightEnd = TatX(displayWidth);
            if (tAtRightEnd > T)
                return true;
            intervalTillNow = T - tAtRightEnd;
            glowIntensity = GlowIntensity(intervalTillNow);
            if (glowIntensity > 0.01)
                return true;
            return false;
        }

        var tAtLeftEnd = TatX(0);
        if (tAtLeftEnd > T)
            return true;
        intervalTillNow = T - tAtLeftEnd;
        glowIntensity = GlowIntensity(intervalTillNow);
        if (glowIntensity > 0.01)
            return true;
        return false;
    }

    public RGB TraceColor(int x)
    {
        double tAtX = TatX(x);

        if (tAtX > T)
            return new RGB(0, 0, 0);

        double intervalTillNow = T - tAtX;

        double glowIntensity = GlowIntensity(intervalTillNow);

        return new RGB(
            (byte)(glowIntensity * Color.R),
            (byte)(glowIntensity * Color.G),
            (byte)(glowIntensity * Color.B)
            );
    }

    public void Initialize(int x0)
    {
        int v = Random.Shared.Next(4) + 2;

        //byte r = (byte)Random.Shared.Next(256);
        //byte g = (byte)Random.Shared.Next(256);
        //int leftover = 510 - r - g;
        //byte b = leftover <= 255 ? (byte)leftover : (byte)255;

        byte r = (byte)(Random.Shared.Next(2) * 128 + Random.Shared.Next(2) * 127);
        byte g = (byte)(Random.Shared.Next(2) * 128 + Random.Shared.Next(2) * 127);
        byte b = (byte)(Random.Shared.Next(2) * 128 + Random.Shared.Next(2) * 127);


        Color = new RGB(r, g, b);
        T = 0;
        X0 = x0;
        V = x0 == 0 ? v : -v;
    }

}



