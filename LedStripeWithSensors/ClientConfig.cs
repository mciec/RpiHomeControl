using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedStripeWithSensors;
internal sealed class ClientConfig
{
    public string ClientId { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string OverrideTopic { get; set; } = @"entrance/override";
    public string MotionDetectedTopic { get; set; } = @"entrance/motion";

},
}

