namespace OneSolution.Core.Messaging
{
    public interface IServiceBusFactory
    {
        IServiceBusSender<T> CreateSender<T>(string name) where T : class;
        IServiceBusReceiver CreateReceiver<T>(string name, IMessageProcessor<T> processor) where T : class;
    }
}