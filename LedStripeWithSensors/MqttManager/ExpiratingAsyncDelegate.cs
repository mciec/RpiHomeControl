namespace LedStripeWithSensors.MqttManager;

internal class ExpiratingAsyncDelegate : IExpiratingAsyncDelegate
{
    public Task Delegate { get; set; } = null!;
    public DateTime? ExpirationDate { get; set; }
}
