using System;
using System.Threading.Tasks;

namespace OneSolution.Core.Messaging
{
    public interface IServiceBusSender<T> where T : class
    {
        Task SendMessagesAsync(T payload);
        Task ScheduleMessageAsync(T payload, DateTimeOffset scheduleEnqueueTimeUtc);
        Task CloseAsync();
    }
}