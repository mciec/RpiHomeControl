using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace LedStripeWithSensors.MqttManager;

internal sealed class QueueConsumerWithRetry
{
    private readonly Func<Task<bool>> _recoveryFunc;
    private readonly int _maxAttempts;
    private readonly ActionBlock<IExpiratingAsyncDelegate> _actionQueue;

    private QueueConsumerWithRetry(Func<Task<bool>> recoveryFunc, int maxAttempts)
    {
        _recoveryFunc = recoveryFunc;
        _maxAttempts = maxAttempts;
        _actionQueue = new ActionBlock<IExpiratingAsyncDelegate>(async action =>
        {
            int delayMs = 100;
            int attemptNo = 0;
            bool recoveryFuncSuccess = true;
            do
            {
                try
                {
                    if (action.ExpirationDate > DateTime.Now)
                        return;

                    var now = DateTime.Now;
                    var timeLeftMs = action.ExpirationDate == null
                        ? 0
                        : (int)(action.ExpirationDate.Value - now).TotalMilliseconds;

                    if (timeLeftMs < 0)
                        return;

                    if (attemptNo > 0)
                        recoveryFuncSuccess = await recoveryFunc();

                    if (recoveryFuncSuccess)
                    {
                        await action.Delegate;
                        return;
                    }

                    attemptNo++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    attemptNo++;
                }

                await Task.Delay(delayMs);
                delayMs = Math.Min(delayMs * 2, 6_400);

            } while (maxAttempts == 0 || attemptNo >= maxAttempts);
        });
    }

    public static QueueConsumerWithRetry StartConsumer(Func<Task<bool>> recoveryAsyncFunc, int maxAttempts)
    {
        var consumer = new QueueConsumerWithRetry(recoveryAsyncFunc, maxAttempts);
        Task.Run(async () =>
        {
            while (true)
            {
                await consumer._actionQueue.Completion;
            }
        })
        return consumer;

    }

    private async Task Consume()
    {

    }
}
