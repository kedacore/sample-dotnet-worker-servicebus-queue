using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Keda.Samples.Dotnet.OrderProcessor
{
    public class OrdersQueueProcessor : QueueWorker<Order>
    {
        private readonly HttpClient _client;

        public OrdersQueueProcessor(IConfiguration configuration, ILogger<OrdersQueueProcessor> logger, IHttpClientFactory httpClientFactory)
            : base(configuration, logger)
        {
            _client = httpClientFactory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        protected override async Task ProcessMessage(Order order, string messageId, Message.SystemPropertiesCollection systemProperties, IDictionary<string, object> userProperties, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Processing order {OrderId} for {OrderAmount} units of {OrderArticle} bought by {CustomerFirstName} {CustomerLastName}", order.Id, order.Amount, order.ArticleNumber, order.Customer.FirstName, order.Customer.LastName);

            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            var serializedOrder = new StringContent(JsonSerializer.Serialize(order), Encoding.UTF8, "application/json");
            var webBaseUrl = Configuration.GetValue<string>("KEDA_SAMPLEWEB_URL");
            var result = await _client.PostAsync($"{webBaseUrl}/api/order/", serializedOrder);

            if( result.IsSuccessStatusCode)
            { 
                Logger.LogInformation("Order {OrderId} processed", order.Id);
            }
            else
            {
                Logger.LogError("Order processing failed: " + result.ReasonPhrase);
            }
        }
    }
}
