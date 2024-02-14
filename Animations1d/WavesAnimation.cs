using Animations1d.Display;

namespace Animations1d;

public sealed class WavesAnimation : AnimationBase
{
    private readonly int _wavesCount;
    private readonly int _version;

    private static double oneDegree = Math.PI / 180.0;
    private static sbyte[] SinLookup;
    private static byte[] SinSqrLookup;
    private static sbyte[] CosLookup;
    private static byte[] CosSqrLookup;

    static WavesAnimation()
    {
        SinLookup = new sbyte[360];
        SinSqrLookup = new byte[360];
        CosLookup = new sbyte[360];
        CosSqrLookup = new byte[360];
        double angle = 0;
        for (int i = 0; i < SinLookup.Length; i++)
        {
            var sinus = Math.Sin(angle);
            SinLookup[i] = (sbyte)(sinus * 127);
            SinSqrLookup[i] = (byte)(sinus * sinus * 255);
            CosLookup[i] = (sbyte)(Math.Cos(angle) * 127);
            CosSqrLookup[i] = (byte)(255 - SinSqrLookup[i]);
            angle += oneDegree;
        }

    }
    private Wave[] Waves { get; set; }

    private WavesAnimation(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display) : base(display)
    {
        //TODO: temporary
        _version = flyingBallsAnimationConfig.StaticBallsCount;

        _wavesCount = 1;   // flyingBallsAnimationConfig.MovingBallsCount;
        Waves = new Wave[_wavesCount];
        for (int i = 0; i < _wavesCount; i++)
            Waves[i] = new();
    }

    public static WavesAnimation Create(FlyingBallsAnimationConfig flyingBallsAnimationConfig, IDisplay display)
    {
        return new WavesAnimation(flyingBallsAnimationConfig, display);
    }

    public override void Dispose()
    {
        Console.WriteLine($"ShootingLaserAnimation disposed");
    }

    protected override void GenerateNextFrame()
    {
        MoveWaves();
        Display.Flush();
    }

    private void MoveWaves()
    {
        int[] tmpR = new int[Display.Width];
        int[] tmpG = new int[Display.Width];
        int[] tmpB = new int[Display.Width];

        for (int i = 0; i < _wavesCount; i++)
        {
            Waves[i].T++;

            for (int x = 0; x < Display.Width; x++)
            {
                var color = Waves[i].TraceColor(x, _version);
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

internal class Wave
{
    private static double oneDegree = Math.PI / 180.0;
    private static byte[] SinLookup;
    private static byte[] SinSqrLookup;
    private static byte[] CosLookup;
    private static byte[] CosSqrLookup;

    static Wave()
    {
        SinLookup = new byte[360];
        SinSqrLookup = new byte[360];
        CosLookup = new byte[360];
        CosSqrLookup = new byte[360];
        double degree = 0;
        for (int i = 0; i < SinLookup.Length; i++)
        {
            var sinus = Math.Sin(degree);
            SinLookup[i] = (byte)(((sinus + 1.0) * 0.5) * 255);
            SinSqrLookup[i] = (byte)(sinus * sinus * 255);
            CosLookup[i] = (byte)(((Math.Cos(degree) + 1) * 0.5) * 255);
            CosSqrLookup[i] = (byte)(255 - SinSqrLookup[i]);
            degree += oneDegree;
        }

    }

    public RGB Color { get; set; }

    public int X0 { get; set; }
    public int T { get; set; }
    public int V { get; set; }

    public int X => X0 + (T * V);   //constant speed movement
    public double TatX(int x) => (x - X0) / (double)V;
    public int Dir => V > 0 ? 1 : -1;

    public RGB TraceColor(int x, int version)
    {
        var sin1 = Sinus(x * 23 - T * 5);
        var sin2 = Sinus(x * 17 - (T * 0));
        var res1 = (byte)((sin1 * 255) >> 8);

        sin1 = Sinus(x * 23 - T * 8);
        sin2 = Sinus(x * 13 - (T * 7));
        var res2 = (byte)((sin1 * 255) >> 8);

        sin1 = Sinus(x * 7 - T * 7);
        sin2 = Sinus(x * 11 - (T * 13));
        var res3 = (byte)((sin1 * 255) >> 8);

        var res4 = (res1 * res2) >> 8;

        return new RGB(
            (byte)(version == 1 ? res1 : version == 2 ? res2 : version == 3 ? res3 : res4),
            (byte)(0),
            (byte)(0)
            );
    }

    public void Initialize(int x0)
    {
        byte r = 0, g = 0, b = 0;

        int v = Random.Shared.Next(4) + 1;

        Color = new RGB(r, g, b);
        T = 0;
        X0 = x0;
        V = x0 == 0 ? v : -v;
    }

    private byte Sinus(int angle)
    {
        int limitedAngle, res;
        byte byteRes;
        if (angle < 0)
        {
            limitedAngle = -angle % 360;
            res = 255 - SinLookup[limitedAngle];
            byteRes = (byte)res;
            return byteRes;
        }
        limitedAngle = angle % 360;
        res = SinLookup[limitedAngle];
        byteRes = (byte)res;
        return byteRes;
    }
    private byte SinusSqr(int angle) => SinSqrLookup[Math.Abs(angle) % 360];
    private byte Cosinus(int angle) => CosLookup[Math.Abs(angle) % 360];

}



