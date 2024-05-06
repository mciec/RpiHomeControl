using Animations1d.Display;
using System.Drawing;
using System.Numerics;

namespace Animations1d;

public sealed class TraceAnimation : AnimationBase
{
    private readonly int _tracedBallsCount;

    private static double oneDegree = Math.PI / 180.0;
    private static int[] SinLookup;
    private static byte[] SinSqrLookup;
    private static int[] CosLookup;
    private static byte[] CosSqrLookup;

    static TraceAnimation()
    {
        SinLookup = new int[360];
        SinSqrLookup = new byte[360];
        CosLookup = new int[360];
        CosSqrLookup = new byte[360];
        double degree = 0;
        for (int i = 0; i < SinLookup.Length; i++)
        {
            var sinus = Math.Sin(degree);
            SinLookup[i] = (int)(sinus * 127);
            SinSqrLookup[i] = (byte)(sinus * sinus * 255);
            CosLookup[i] = (int)(Math.Cos(degree) * 127);
            CosSqrLookup[i] = (byte)(255 - SinSqrLookup[i]);
            degree += oneDegree;
        }
    }
    private TracedBall[] TracedBalls { get; set; }

    private TraceAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display) : base(display)
    {
        _tracedBallsCount = 2;
        TracedBalls = new TracedBall[_tracedBallsCount];
        for (int i = 0; i < _tracedBallsCount; i++)
        {
            TracedBalls[i] = new(x0: -1, v: 0, colorPalette: 0, viewPortSize: 0, dimmingPercent: 100, (oldX, t) => 0);
        }
    }

    public static TraceAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display)
    {
        return new TraceAnimation(flyingBallsAnimationConfig, display);
    }

    public override void Dispose()
    {
        Console.WriteLine($"TraceAnimation disposed");
    }

    protected override void GenerateNextFrame()
    {
        MoveTracedBalls();
        Display.Flush();
    }

    private void MoveTracedBalls()
    {
        for (int i = 0; i < _tracedBallsCount; i++)
        {
            if (TracedBalls[i].IsBlank)
            {
                switch (i)
                {
                    case 0: 
                        TracedBalls[i] = new TracedBall(
                            x0: 0, 
                            v: 5, 
                            colorPalette: 1, 
                            viewPortSize: Display.Width, 
                            dimmingPercent: 70,
                            (oldX, t) => 50 + (SinLookup[(t * 2) % 360] / 3) + (SinLookup[(t * 13) % 360] / 10)); 
                        break;
                    
                    default:
                        var randomV = Random.Shared.Next(2, 3);
                        var colorPalette = Random.Shared.Next(0, 1);    // := 0 (fire)
                        TracedBalls[i] = new TracedBall(
                            x0: 0, 
                            v: 5, 
                            colorPalette: colorPalette, 
                            viewPortSize: Display.Width,
                            dimmingPercent: 90,
                            (oldX, t) => oldX + randomV); 
                        break;
                }
            }
            TracedBalls[i].MoveBall();
        }

        for (int x = 0; x < Display.Width; x++)
        {
            byte maxR = 0, maxG = 0, maxB = 0;
            foreach (var tracedBall in TracedBalls.Where(tb => !tb.IsBlank))
            {
                maxR = Math.Max(maxR, tracedBall.ColorView[x].R);
                maxG = Math.Max(maxG, tracedBall.ColorView[x].G);
                maxB = Math.Max(maxB, tracedBall.ColorView[x].B);
            }
            Display.Matrix[x] = new RGB(maxR, maxG, maxB);
        }
    }
    private int Sinus(int angle) => angle < 0 ? -SinLookup[-angle % 360] : SinLookup[angle % 360];
    private int SinusSqr(int angle) => SinSqrLookup[Math.Abs(angle) % 360];
}

internal class TracedBall
{
    private int _t = 0;
    private const int dimmingPercent = 90;
    private const int maxIntensity = 0xFFFFFF;
    private byte[] _intensityView;

    public RGB[] ColorView { get; private set; }
    public bool IsBlank { get; private set; } = true;

    private static double oneDegree = Math.PI / 180.0;
    private static byte[] SinLookup;
    private static byte[] SinSqrLookup;
    private static byte[] CosLookup;
    private static byte[] CosSqrLookup;
    private static (byte, byte, byte)[][] ColourLookup;

    public int X0 { get; set; }
    public int V { get; set; }
    public int ColorPalette { get; set; }
    public int ViewPortSize { get; }
    public int DimmingPercent { get; }
    public int X { get; set; }
    public int T { get; private set; } = 0;
    public int Dir => V > 0 ? 1 : -1;
    public delegate int NewXCalculator(int oldX, int t);
    public readonly NewXCalculator _newXcalculator;

    public TracedBall(int x0, int v, int colorPalette, int viewPortSize, int dimmingPercent, NewXCalculator newXcalculator)
    {
        X0 = x0;
        V = v;
        ColorPalette = colorPalette;
        ViewPortSize = viewPortSize;
        DimmingPercent = dimmingPercent;
        _newXcalculator = newXcalculator;
        _intensityView = new byte[viewPortSize];
    }

    public bool MoveBall()
    {
        var oldx = X;
        var x = _newXcalculator(X, ++T);
        return MoveBall(x);
    }

    public bool MoveBall(int newX)
    {
        IsBlank = DimIntensityView(DimmingPercent);
        var oldX = X;
        var intensityAtOldX_16 = (maxIntensity * dimmingPercent) / 100;
        var intensityAtNewX_16 = maxIntensity;

        if (newX >= 0 && newX < _intensityView.Length)
        {
            _intensityView[newX] = 255;
            IsBlank = false;
        }

        if (newX != X)
        {
            var step_16 = Math.Abs((intensityAtNewX_16 - intensityAtOldX_16) / (newX - X));

            oldX = X < 0 ? 0 : oldX >= _intensityView.Length ? _intensityView.Length - 1 : oldX;

            intensityAtOldX_16 = (oldX - X) * step_16 + intensityAtOldX_16;

            if (newX <= X)
            {
                var currVal_16 = intensityAtOldX_16;
                for (int i = X - 1; i > newX; i--)
                {
                    currVal_16 += step_16;
                    if (i < 0 || i >= _intensityView.Length)
                        continue;
                    _intensityView[i] = (byte)(currVal_16 >> 16);
                    IsBlank &= _intensityView[i] == 0;
                }
            }
            else
            {
                var currVal_16 = intensityAtOldX_16;
                for (int i = X + 1; i < newX; i++)
                {
                    currVal_16 += step_16;
                    if (i < 0 || i >= _intensityView.Length)
                        continue;
                    _intensityView[i] = (byte)(currVal_16 >> 16);
                    IsBlank &= _intensityView[i] == 0;
                }
            }
        }

        X = newX;
        FlushColorView(ColorPalette);
        return IsBlank;
    }

    private bool DimIntensityView(int targetPercent)
    {
        bool isBlank = true;
        int ratio_16 = (targetPercent << 16) / 100;
        byte targetVal;

        for (int i = 0; i < _intensityView.Length; i++)
        {
            int intensityViewAtI = _intensityView[i];

            int intensity_16 = intensityViewAtI * ratio_16;
            targetVal = (byte)(intensity_16 >> 16);
            _intensityView[i] = targetVal;
            if (targetVal > 0)
                isBlank = false;
        }

        return isBlank;
    }

    private void FlushColorView(int colorPallette)
    {
        ColorView = _intensityView.Select(intensity => new RGB(
            ColourLookup[colorPallette][intensity].Item1,
            ColourLookup[colorPallette][intensity].Item2,
            ColourLookup[colorPallette][intensity].Item3)).ToArray();
    }

    static TracedBall()
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

        ColourLookup = new (byte, byte, byte)[3][];
        for (int i = 0; i < 3; i++)
            ColourLookup[i] = new (byte, byte, byte)[256];

        for (int i = 0; i < 256; i++)
        {
            ColourLookup[0][i] = (
                i < 70 ? (byte)(i * (256.0 / 70)) : i < 200 ? (byte)255 : (byte)255,    //(byte)(255 - (i - 200)),
                i < 70 ? (byte)0 : i < 200 ? (byte)((i - 70.0) * (256.0 / (200.0 - 70.0))) : (byte)255, //(byte)(255 - (i - 200)),
                i < 200 ? (byte)0 : (byte)((i - 200) * (255.0 / 55.0))
                );

            ColourLookup[1][i] = (
                i < 200 ? (byte)0 : (byte)((i - 200) * (255.0 / 55.0)),
                i < 70 ? (byte)0 : i < 200 ? (byte)((i - 70.0) * (256.0 / (200.0 - 70.0))) : (byte)255, //(byte)(255 - (i - 200)),
                i < 70 ? (byte)(i * (256.0 / 70)) : i < 200 ? (byte)255 : (byte)255    //(byte)(255 - (i - 200)),
                );

            ColourLookup[2][i] = (
                i < 200 ? (byte)0 : (byte)((i - 200) * (255.0 / 55.0)),
                i < 70 ? (byte)(i * (256.0 / 70)) : i < 200 ? (byte)255 : (byte)255,    //(byte)(255 - (i - 200)),
                i < 70 ? (byte)0 : i < 200 ? (byte)((i - 70.0) * (256.0 / (200.0 - 70.0))) : (byte)255 //(byte)(255 - (i - 200)),
                );
        }
    }
}
