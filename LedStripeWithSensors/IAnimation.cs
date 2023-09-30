internal enum AnimationDirection
{
    Left = 1,
    Right = 2
}

internal interface IAnimation
{
    void NextFrame();
    void Clear();
    void Start(AnimationDirection direction)
}
