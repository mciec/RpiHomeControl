﻿using System.Device.Gpio;

namespace LedStripeWithSensors.MotionSensor;

internal sealed class MotionSensor : IDisposable
{
    private readonly int _gpio;
    private readonly PinChangeEventHandler _onMotionOn;
    private readonly PinChangeEventHandler _onMotionOff;
    private GpioController _gpioController = null;

    private MotionSensor(int gpio, Action onMotionOn, Action onMotionOff)
    {
        _gpio = gpio;
        _onMotionOn = (sender, args) => onMotionOn();
        _onMotionOff = (sender, args) => onMotionOff();
    }

    public void Run()
    {
        _gpioController = new GpioController();
        _gpioController.OpenPin(_gpio, PinMode.InputPullDown);

        _gpioController.RegisterCallbackForPinValueChangedEvent(
            _gpio,
            PinEventTypes.Rising,
            _onMotionOn
            );

        _gpioController.RegisterCallbackForPinValueChangedEvent(
            _gpio,
            PinEventTypes.Falling,
            _onMotionOff
            );
    }

    public static MotionSensor CreateSensor(int gpio, Action onMotionOn, Action onMotionOff)
    {
        return new MotionSensor(gpio, onMotionOn, onMotionOff);
    }

    public void Dispose()
    {
        _gpioController?.UnregisterCallbackForPinValueChangedEvent(_gpio, _onMotionOn);
        _gpioController?.UnregisterCallbackForPinValueChangedEvent(_gpio, _onMotionOff);
        _gpioController?.Dispose();
    }
}

