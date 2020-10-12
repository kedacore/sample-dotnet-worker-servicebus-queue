using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.OrderProcessor
{
    public class OrdersQueueProcessor : QueueWorker<Order>
    {
        public OrdersQueueProcessor(IConfiguration configuration, ILogger<OrdersQueueProcessor> logger)
            : base(configuration, logger)
        {
        }

        protected override async Task ProcessMessage(Order order, string messageId, IReadOnlyDictionary<string, object> userProperties, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName);

            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

            Logger.LogInformation("Order {OrderId} processed", order.Id);
        }
    }
}
