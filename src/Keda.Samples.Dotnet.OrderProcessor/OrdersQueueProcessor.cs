using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.OrderProcessor
{
    public interface IOrdersQueueProcessor
    {

    }

    public class OrdersQueueProcessor : QueueWorker<Order>, IOrdersQueueProcessor
    {
        private readonly HttpClient _client;

        public OrdersQueueProcessor(IConfiguration configuration, ILogger<OrdersQueueProcessor> logger, IHttpClientFactory httpClientFactory)
            : base(configuration, logger)
        {
            _client = httpClientFactory.CreateClient();
        }

        protected override async Task ProcessMessage(Order order, string messageId, Message.SystemPropertiesCollection systemProperties, IDictionary<string, object> userProperties, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName);

            //await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            await _client.GetAsync("https://localhost:44348/api/order/purchase");

            Logger.LogInformation("Order {OrderId} processed", order.Id);
        }
    }
}
