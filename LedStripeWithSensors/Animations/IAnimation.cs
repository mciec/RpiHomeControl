internal enum Direction
{
    NONE = 0,
    LEFT = 1,
    RIGHT = 2
}

internal interface IAnimation : IDisposable
{
    void NextFrame();
    void Clear();
    void Start(Direction direction);
}
