using Microsoft.Azure.ServiceBus;

namespace Common.Interfaces
{
    public interface IServiceBusHandler
    {
        void CloseAllClients();
        void RegisterMessageHandler<T>(string createTaskQueue, Func<T?, Task> createAsync);
        Task SendMessageAsync<T>(T serviceBusMessage, string queueName);
    }
}