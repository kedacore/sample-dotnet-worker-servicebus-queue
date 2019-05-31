using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Keda.Samples.Dotnet.QueueWorker
{
    public abstract class QueueWorker<TMessage> : BackgroundService
    {
        protected ILogger<QueueWorker<TMessage>> Logger { get; }
        protected IConfiguration Configuration { get; }

        public QueueWorker(IConfiguration configuration, ILogger<QueueWorker<TMessage>> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connectionString = Configuration.GetValue<string>("KEDA_SERVICEBUS_CONNECTIONSTRING");
            var queueName = Configuration.GetValue<string>("KEDA_SERVICEBUS_QUEUENAME");

            var queueClient = new QueueClient(connectionString, queueName, ReceiveMode.PeekLock);

            Logger.LogInformation("Starting message pump at: {Time}", DateTimeOffset.UtcNow);
            queueClient.RegisterMessageHandler(HandleMessage, HandleReceivedException);
            Logger.LogInformation("Message pump started at: {Time}", DateTimeOffset.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            Logger.LogInformation("Closing message pump at: {Time}", DateTimeOffset.UtcNow);
            await queueClient.CloseAsync();
            Logger.LogInformation("Message pump closed : {Time}", DateTimeOffset.UtcNow);
        }

        private Task HandleReceivedException(ExceptionReceivedEventArgs exceptionEvent)
        {
            Logger.LogError(exceptionEvent.Exception, "Unable to process message at: {Time}", DateTimeOffset.UtcNow);
            return Task.CompletedTask;
        }

        protected abstract Task ProcessMessage(TMessage order, string messageId, Message.SystemPropertiesCollection systemProperties, IDictionary<string, object> userProperties, CancellationToken cancellationToken);

        private async Task HandleMessage(Message message, CancellationToken cancellationToken)
        {
            var rawMessageBody = Encoding.UTF8.GetString(message.Body);
            Logger.LogInformation("Received message {MessageId} with body {MessageBody}", message.MessageId, rawMessageBody);

            var order = JsonConvert.DeserializeObject<TMessage>(rawMessageBody);
            if (order != null)
            {
                await ProcessMessage(order, message.MessageId, message.SystemProperties, message.UserProperties, cancellationToken);
            }
            else
            {
                Logger.LogError("Unable to deserialize to message contract {ContractName} for message {MessageBody}", typeof(TMessage), rawMessageBody);
            }

            Logger.LogInformation("Message {MessageId} processed at: {Time}", message.MessageId, DateTimeOffset.UtcNow);
        }
    }
}
