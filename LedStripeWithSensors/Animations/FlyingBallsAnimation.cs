using Iot.Device.Ws28xx;
using System.Drawing;
using UnitsNet;

namespace LedStripeWithSensors.Animations;

internal sealed class FlyingBallsAnimation : AnimationBase
{
    private readonly int _staticBallsCount;
    private readonly int _movingBallsCount;
    private readonly Color[] _temp;

    private int stretchingOmega = 0;
    private int stretchingOmegaDir = 1;

    private static double oneDegree = Math.PI / 180.0;
    private static int[] SinLookup;
    private static int[] SinSqrLookup;
    private static int[] CosLookup;

    static FlyingBallsAnimation()
    {
        SinLookup = new int[360];
        SinSqrLookup = new int[360];
        CosLookup = new int[360];
        double degree = 0;
        for (int i = 0; i < SinLookup.Length; i++)
        {
            var sinus = Math.Sin(degree);
            SinLookup[i] = (int)(sinus * 127);
            SinSqrLookup[i] = (int)(sinus * sinus * sinus * sinus * sinus * sinus * 255);
            CosLookup[i] = (int)(Math.Cos(degree) * 127);
            degree += oneDegree;
        }
    }

    private FlyingBallsAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, Ws2812b neopixel) : base(neopixel)
    {
        _staticBallsCount = flyingBallsAnimationConfig.StaticBallsCount;
        _movingBallsCount = flyingBallsAnimationConfig.MovingBallsCount;
        _temp = new Color[Length];
    }

    public static FlyingBallsAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, Ws2812b neopixel)
    {
        return new FlyingBallsAnimation(flyingBallsAnimationConfig, neopixel);
    }

    public override void Dispose()
    {
        Console.WriteLine($"FlyingBallsAnimation disposed");
    }

    protected override void GenerateNextFrame()
    {
        GenerateSin();
        Neopixel.Update();
    }

    private void SetAllBlack()
    {
        for (int i = 0; i < Length; i++)
        {
            Neopixel.Image.SetPixel(i, 0, Color.Black);
        }
    }

    private void GenerateSin()
    {
        Console.WriteLine();

        int maxStretchingOmega = 20;
        int minStretchingOmega = 3;

        int[] movingSinus1 = new int[Length];
        for (int i = 0; i < Length; i++)
        {
            int startingAngle = FrameNumber * 5;
            int omegaT = i * 10;

            //movingSinus1[i] = Sinus(startingAngle + omegaT) + 128;
            movingSinus1[i] = SinusSqr(startingAngle + omegaT);
            //Console.Write($"{movingSinus1[i],4}");
        }

        int[] movingSinus2 = new int[Length];
        for (int i = 0; i < Length; i++)
        {
            int startingAngle = FrameNumber * 7;
            int omegaT = i * 13;

            //movingSinus2[i] = Sinus(startingAngle + omegaT) + 128;
            movingSinus2[i] = SinusSqr(startingAngle + omegaT);
            //Console.Write($"{movingSinus1[i],4}");
        }

        int[] stretchingSinus = new int[Length];
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
        for (int i = 0; i < Length; i++)
        {
            int t = (Length / 2) - i;

            //stretchingSinus[i] = Sinus(stretchingOmega * t) + 128;
            stretchingSinus[i] = SinusSqr(stretchingOmega * t);
            Console.Write($"{stretchingSinus[i],4}");
        }


        for (int i = 0; i < Length; i++)
        {
            Neopixel.Image.SetPixel(i, 0, System.Drawing.Color.FromArgb(movingSinus1[i], stretchingSinus[i], movingSinus2[i]));
        }
    }

    private int Sinus(int angle) => angle < 0 ? -SinLookup[(-angle) % 360] : SinLookup[angle % 360];

    private int SinusSqr(int angle) => SinSqrLookup[Math.Abs(angle) % 360];




}

internal class Ball
{
    Color Color { get; set; }




}



