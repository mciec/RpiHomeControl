internal enum Direction
{
    LEFT = 1,
    RIGHT = 2
}

internal interface IAnimation
{
    void NextFrame();
    void Clear();
    void Start(Direction direction);
}
