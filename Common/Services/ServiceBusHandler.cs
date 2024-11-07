using Common.Constants;
using Common.Interfaces;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Common.Services
{
    public class ServiceBusHandler(IConfiguration configuration,
        IEnumerable<IQueueClient> queueClients,
        ILogger<ServiceBusHandler> logger) : IServiceBusHandler
    {
        private readonly Dictionary<string, IQueueClient> _queueClients = 
            queueClients.ToDictionary(k => k.QueueName, v => v);

        private readonly int _maxRetry = int.Parse(configuration["MaxRetryCount"]
                ?? throw new NotImplementedException("MaxRetryCount config missing"));
        public async Task SendMessageAsync<T>(T serviceBusMessage, string queueName)
        {
            string messageBody = JsonSerializer.Serialize(serviceBusMessage);
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            int attempt = 0;

            while (attempt < _maxRetry)
            {
                try
                {
                    await _queueClients[queueName].SendAsync(message);
                    break;
                }
                catch (ServiceBusException ex)
                {
                    attempt++;
                    logger.LogError(ex, $"Attempt {attempt} failed: {ex.Message}");
                    if (attempt == _maxRetry)
                    {
                        logger.LogWarning("Max retry attempts reached. Could not send the message.");
                        throw;
                    }
                }
            }
        }

        public void RegisterMessageHandler<T>(string queueName, Func<T?, Task> dbAction)
        {
            var queueClient = _queueClients[queueName];

            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                MaxConcurrentCalls = 1,
                AutoComplete = false
            };

            async Task processMessagesAsync(Message message, CancellationToken ct)
            {
                int attempt = 0;
                try
                {
                    var jsonString = Encoding.UTF8.GetString(message.Body);
                    var dto = JsonSerializer.Deserialize<T>(jsonString);

                    await dbAction(dto);

                    await queueClient.CompleteAsync(message.SystemProperties.LockToken);
                }
                catch (ServiceBusException ex)
                {
                    attempt++;
                    logger.LogError(ex, $"Attempt {attempt} failed: {ex.Message}");
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    if (attempt == _maxRetry)
                    {
                        logger.LogWarning("Max retry attempts reached. Could not send the message.");
                        throw;
                    }

                    await queueClient.AbandonAsync(message.SystemProperties.LockToken);
                }
            }

            queueClient.RegisterMessageHandler(processMessagesAsync, messageHandlerOptions);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            Console.WriteLine($"Message handler exception: {arg.Exception}");
            return Task.CompletedTask;
        }

        public void CloseAllClients() =>
            Parallel.ForEach(_queueClients, async (client) => await client.Value.CloseAsync());

    }
}
