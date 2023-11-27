using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedStripeWithSensors.Neopixel;

internal sealed class Neopixel
{
    private readonly NeopixelConfig _neopixelConfig;

    public Neopixel(NeopixelConfig neopixelConfig)
    {
        _neopixelConfig = neopixelConfig;


    }


}
