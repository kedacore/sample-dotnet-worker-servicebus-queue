using Bogus;
using Keda.Samples.Dotnet.Contracts;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Keda.Samples.Dotnet.OrderGenerator
{
    class Program
    {
        private static string QueueName = Environment.GetEnvironmentVariable("KEDA_SERVICEBUS_QUEUE_NAME");
        private static string ConnectionString = Environment.GetEnvironmentVariable("KEDA_SERVICEBUS_QUEUE_CONNECTIONSTRING");

        static async Task Main(string[] args)
        {
            Console.WriteLine("Let's queue some orders, how many do you want?");

            var requestedAmount = DetermineOrderAmount();
            Console.WriteLine($"{requestedAmount} orders are going to be queued in ${QueueName} queue");
            await QueueOrders(requestedAmount);

            Console.WriteLine("That's it, see you later!");
        }

        private static async Task QueueOrders(int requestedAmount)
        {
            var serviceBusClient = new ServiceBusClient(ConnectionString);
            var serviceBusSender = serviceBusClient.CreateSender(QueueName);

            for (int currentOrderAmount = 0; currentOrderAmount < requestedAmount; currentOrderAmount++)
            {
                var order = GenerateOrder();
                var rawOrder = JsonConvert.SerializeObject(order);
                var orderMessage = new ServiceBusMessage(rawOrder);

                Console.WriteLine($"Queuing order {order.Id} - A {order.ArticleNumber} for {order.Customer.FirstName} {order.Customer.LastName}");
                await serviceBusSender.SendMessageAsync(orderMessage);
            }
        }

        private static Order GenerateOrder()
        {
            var customerGenerator = new Faker<Customer>()
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName())
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName());

            var orderGenerator = new Faker<Order>()
                .RuleFor(u => u.Customer, () => customerGenerator)
                .RuleFor(u => u.Id, f => Guid.NewGuid().ToString())
                .RuleFor(u => u.Amount, f => f.Random.Int())
                .RuleFor(u => u.ArticleNumber, f => f.Commerce.Product());

            return orderGenerator.Generate();
        }

        private static int DetermineOrderAmount()
        {
            var requestedAmountFromEnvVariable = DetermineOrderAmountFromEnvVariable();
            if (requestedAmountFromEnvVariable > 0) {
                return requestedAmountFromEnvVariable;
            }
            var requestedAmountFromEnvVariableFromConsole = DetermineOrderAmountFromConsole();
            return requestedAmountFromEnvVariableFromConsole;
        }

        private static int DetermineOrderAmountFromEnvVariable()
        {
           var OrderAmount = Environment.GetEnvironmentVariable("ORDER_AMOUNT");
            if (int.TryParse(OrderAmount, out int orderAmount))
            {
                return orderAmount;
            }
            return 0; 
        }

        private static int DetermineOrderAmountFromConsole()
        {

            var rawAmount = Console.ReadLine();
            if (int.TryParse(rawAmount, out int amount))
            {
                return amount;
            }

            Console.WriteLine("That's not a valid amount, let's try that again");
            return DetermineOrderAmountFromConsole();
        }
    }
}
