using System.Threading;
using System.Threading.Tasks;

namespace OneSolution.Core.Messaging
{
    public interface IMessageProcessor<T> where T : class
    {
        Task ProcessMessagesAsync(T payload, string lockToken, CancellationToken token);
    }
}
