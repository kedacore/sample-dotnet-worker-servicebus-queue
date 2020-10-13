using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.OrderProcessor
{
    public static class ServiceBusClientFactory
    {
        public static ServiceBusClient CreateWithManagedIdentityAuthentication(IConfiguration configuration, ILogger logger)
        {
            var hostname = configuration.GetValue<string>("KEDA_SERVICEBUS_HOST_NAME");
           
            var clientIdentityId = configuration.GetValue<string>("KEDA_SERVICEBUS_IDENTITY_USERASSIGNEDID", defaultValue: null);
            if (string.IsNullOrWhiteSpace(clientIdentityId) == false)
            {
                logger.LogInformation("Using user-assigned identity with ID {UserAssignedIdentityId}", clientIdentityId);
            }

            return new ServiceBusClient(hostname, new ManagedIdentityCredential(clientId: clientIdentityId));
        }

        public static ServiceBusClient CreateWithServicePrincipleAuthentication(IConfiguration configuration)
        {
            var hostname = configuration.GetValue<string>("KEDA_SERVICEBUS_HOST_NAME");
            var tenantId = configuration.GetValue<string>("KEDA_SERVICEBUS_TENANT_ID");
            var appIdentityId = configuration.GetValue<string>("KEDA_SERVICEBUS_IDENTITY_APPID");
            var appIdentitySecret = configuration.GetValue<string>("KEDA_SERVICEBUS_IDENTITY_SECRET");

            return new ServiceBusClient(hostname, new ClientSecretCredential(tenantId, appIdentityId, appIdentitySecret));
        }

        public static ServiceBusClient CreateWithConnectionStringAuthentication(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("KEDA_SERVICEBUS_QUEUE_CONNECTIONSTRING");
            return new ServiceBusClient(connectionString);
        }
    }
}
