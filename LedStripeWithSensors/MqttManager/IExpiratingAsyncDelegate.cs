using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LedStripeWithSensors.MqttManager;

internal interface IExpiratingAsyncDelegate
{
    Task Delegate { get; set; }
    DateTime? ExpirationDate { get; set; }

}
