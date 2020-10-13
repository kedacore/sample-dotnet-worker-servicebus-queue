using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Keda.Samples.Dotnet.OrderProcessor
{
    public abstract class QueueWorker<TMessage> : BackgroundService
    {
        protected ILogger<QueueWorker<TMessage>> Logger { get; }
        protected IConfiguration Configuration { get; }

        protected QueueWorker(IConfiguration configuration, ILogger<QueueWorker<TMessage>> logger)
        {
            Configuration = configuration;
            Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var queueName = Configuration.GetValue<string>("KEDA_SERVICEBUS_QUEUE_NAME");
            var messageProcessor = CreateServiceBusProcessor(queueName);
            messageProcessor.ProcessMessageAsync += HandleMessageAsync;
            messageProcessor.ProcessErrorAsync += HandleReceivedExceptionAsync;
            
            Logger.LogInformation($"Starting message pump on queue {queueName} in namespace {messageProcessor.FullyQualifiedNamespace}");
            await messageProcessor.StartProcessingAsync(stoppingToken);
            Logger.LogInformation("Message pump started");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            Logger.LogInformation("Closing message pump");
            await messageProcessor.CloseAsync(cancellationToken: stoppingToken);
            Logger.LogInformation("Message pump closed : {Time}", DateTimeOffset.UtcNow);
        }

        private ServiceBusProcessor CreateServiceBusProcessor(string queueName)
        {
            var serviceBusClient = AuthenticateToAzureServiceBus();
            var messageProcessor = serviceBusClient.CreateProcessor(queueName);
            return messageProcessor;
        }

        private ServiceBusClient AuthenticateToAzureServiceBus()
        {
            var authenticationMode = Configuration.GetValue<AuthenticationMode>("KEDA_SERVICEBUS_AUTH_MODE");
            
            ServiceBusClient serviceBusClient;

            switch (authenticationMode)
            {
                case AuthenticationMode.ConnectionString:
                    Logger.LogInformation($"Authentication by using connection string");
                    serviceBusClient = ServiceBusClientFactory.CreateWithConnectionStringAuthentication(Configuration);
                    break;
                case AuthenticationMode.ServicePrinciple:
                    Logger.LogInformation("Authentication by using service principle");
                    serviceBusClient = ServiceBusClientFactory.CreateWithServicePrincipleAuthentication(Configuration);
                    break;
                case AuthenticationMode.ManagedIdentity:
                    Logger.LogInformation("Authentication by using managed identity");
                    serviceBusClient = ServiceBusClientFactory.CreateWithManagedIdentityAuthentication(Configuration, Logger);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return serviceBusClient;
        }

        private async Task HandleMessageAsync (ProcessMessageEventArgs processMessageEventArgs)
        {
            try
            {
                var rawMessageBody = Encoding.UTF8.GetString(processMessageEventArgs.Message.Body.ToBytes().ToArray());
                Logger.LogInformation("Received message {MessageId} with body {MessageBody}",
                    processMessageEventArgs.Message.MessageId, rawMessageBody);

                var order = JsonConvert.DeserializeObject<TMessage>(rawMessageBody);
                if (order != null)
                {
                    await ProcessMessage(order, processMessageEventArgs.Message.MessageId,
                        processMessageEventArgs.Message.ApplicationProperties,
                        processMessageEventArgs.CancellationToken);
                }
                else
                {
                    Logger.LogError(
                        "Unable to deserialize to message contract {ContractName} for message {MessageBody}",
                        typeof(TMessage), rawMessageBody);
                }

                Logger.LogInformation("Message {MessageId} processed", processMessageEventArgs.Message.MessageId);

                await processMessageEventArgs.CompleteMessageAsync(processMessageEventArgs.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unable to handle message");
            }
        }

        private Task HandleReceivedExceptionAsync(ProcessErrorEventArgs exceptionEvent)
        {
            Logger.LogError(exceptionEvent.Exception, "Unable to process message");
            return Task.CompletedTask;
        }

        protected abstract Task ProcessMessage(TMessage order, string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken);
    }
}
