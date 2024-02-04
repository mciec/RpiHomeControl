using Animations1d.Display;
using System.Drawing;

namespace Animations1d;

public sealed class FlyingBallsAnimation : AnimationBase
{
    private readonly int _staticBallsCount;
    private readonly int _movingBallsCount;

    private int stretchingOmega = 0;
    private int stretchingOmegaDir = 1;

    private static double oneDegree = Math.PI / 180.0;
    private static byte[] SinLookup;
    private static byte[] SinSqrLookup;
    private static byte[] CosLookup;
    private static byte[] CosSqrLookup;

    static FlyingBallsAnimation()
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

    private FlyingBallsAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display) : base(display)
    {
        _staticBallsCount = flyingBallsAnimationConfig.StaticBallsCount;
        _movingBallsCount = flyingBallsAnimationConfig.MovingBallsCount;
    }

    public static FlyingBallsAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display)
    {
        return new FlyingBallsAnimation(flyingBallsAnimationConfig, display);
    }

    public override void Dispose()
    {
        Console.WriteLine($"FlyingBallsAnimation disposed");
    }

    protected override void GenerateNextFrame()
    {
        GenerateSin();
        Display.Flush();
    }

    private void GenerateSin()
    {
        //Console.WriteLine();

        int maxStretchingOmega = 20;
        int minStretchingOmega = 3;

        int[] movingSinus1 = new int[Display.Width];
        for (int i = 0; i < Display.Width; i++)
        {
            int startingAngle = FrameNumber * 5 * (Direction == Direction.LEFT ? -1 : 1);
            int omegaT = i * 10;

            movingSinus1[i] = SinusSqr(startingAngle + omegaT);
            //Console.Write($"{movingSinus1[i],4}");
        }

        int[] movingSinus2 = new int[Display.Width];
        for (int i = 0; i < Display.Width; i++)
        {
            int startingAngle = FrameNumber * 7 * (Direction == Direction.LEFT ? -1 : 1);
            int omegaT = i * 11;

            movingSinus2[i] = SinusSqr(startingAngle + omegaT);
            //Console.Write($"{movingSinus1[i],4}");
        }

        int[] movingSinus3 = new int[Display.Width];
        for (int i = 0; i < Display.Width; i++)
        {
            int startingAngle = FrameNumber * 9 * (Direction == Direction.LEFT ? -1 : 1);
            int omegaT = i * 12;

            movingSinus3[i] = SinusSqr(startingAngle + omegaT);
            //Console.Write($"{movingSinus1[i],4}");
        }

        int[] stretchingSinus = new int[Display.Width];
        stretchingOmega += stretchingOmegaDir;
        if (stretchingOmega > maxStretchingOmega)
        {
            stretchingOmega = maxStretchingOmega - (stretchingOmega - maxStretchingOmega);
            stretchingOmegaDir = -stretchingOmegaDir;
        }
        else
        if (stretchingOmega < minStretchingOmega)
        {
            stretchingOmega = minStretchingOmega + (minStretchingOmega - stretchingOmega);
            stretchingOmegaDir = -stretchingOmegaDir;
        }
        for (int i = 0; i < Display.Width; i++)
        {
            int t = Display.Width / 2 - i;

            //stretchingSinus[i] = Sinus(stretchingOmega * t) + 128;
            stretchingSinus[i] = SinusSqr(stretchingOmega * t);
            //Console.Write($"{stretchingSinus[i],4}");
        }

        for (int i = 0; i < Display.Width; i++)
        {
            Display.Matrix[i] = new RGB() { R = (byte)movingSinus1[i], G = (byte)movingSinus1[i], B = (byte)movingSinus1[i] };
            Display.Flush();
        }
    }

    private int Sinus(int angle) => angle < 0 ? -SinLookup[-angle % 360] : SinLookup[angle % 360];

    private int SinusSqr(int angle) => SinSqrLookup[Math.Abs(angle) % 360];




}

internal class Ball
{
    Color Color { get; set; }




}



