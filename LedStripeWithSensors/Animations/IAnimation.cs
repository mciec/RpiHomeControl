﻿namespace LedStripeWithSensors.Animations;

internal enum Direction
{
    NONE = 0,
    LEFT = 1,
    RIGHT = 2
}

internal interface IAnimation : IDisposable
{
    void NextFrame();
    void Stop();
    void Start(Direction direction);
}
