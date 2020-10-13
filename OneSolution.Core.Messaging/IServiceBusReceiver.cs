using System;
using System.Threading.Tasks;

namespace OneSolution.Core.Messaging
{
    public interface IServiceBusReceiver
    {
        void Start();
        Task<DateTime> RenewLockAsync(string lockToken);
        Task StopAsync();
    }
}