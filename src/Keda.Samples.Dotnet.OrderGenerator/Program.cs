using Bogus;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Keda.Samples.Dotnet.OrderGenerator
{
    class Program
    {
        private const string ConnectionString = "Endpoint=sb://kedasb.servicebus.windows.net/;SharedAccessKeyName=order-consumer;SharedAccessKey=4IfcwDBrDCf1TUINaQDPIJBvzJyct31XbDzRICYnSaE=;EntityPath=orders";

        static async Task Main(string[] args)
        {
            var deadletterReceiver = new MessageReceiver("Endpoint=sb://kedasb.servicebus.windows.net/;SharedAccessKeyName=order-consumer;SharedAccessKey=4IfcwDBrDCf1TUINaQDPIJBvzJyct31XbDzRICYnSaE=", EntityNameHelper.FormatDeadLetterPath("orders"), ReceiveMode.PeekLock);
            while (true)
            {
                // receive a message
                var msg = await deadletterReceiver.ReceiveAsync(TimeSpan.FromSeconds(10));
                if (msg != null)
                {
                    // write out the message and its user properties
                    Console.WriteLine("Deadletter message:");
                    foreach (var prop in msg.UserProperties)
                    {
                        Console.WriteLine("{0}={1}", prop.Key, prop.Value);
                    }
                    // complete and therefore remove the message from the DLQ
                    await deadletterReceiver.CompleteAsync(msg.SystemProperties.LockToken);
                }
                else
                {
                    // DLQ was empty on last receive attempt
                    break;
                }
            }
            

            Console.WriteLine("Let's queue some orders, how many do you want?");

            var requestedAmount = DetermineOrderAmount();
            await QueueOrders(requestedAmount);

            Console.WriteLine("That's it, see you later!");
        }

        private static async Task QueueOrders(int requestedAmount)
        {
            var serviceBusConnectionStringBuilder = new ServiceBusConnectionStringBuilder(ConnectionString);

            var queueClient = new QueueClient(serviceBusConnectionStringBuilder.GetNamespaceConnectionString(), serviceBusConnectionStringBuilder.EntityPath, ReceiveMode.PeekLock);

            for (int currentOrderAmount = 0; currentOrderAmount < requestedAmount; currentOrderAmount++)
            {
                var order = GenerateOrder();
                var rawOrder = JsonConvert.SerializeObject(order);
                var orderMessage = new Message(Encoding.UTF8.GetBytes(rawOrder));

                Console.WriteLine($"Queuing order {order.Id} - A {order.ArticleNumber} for {order.Customer.FirstName} {order.Customer.LastName}");
                await queueClient.SendAsync(orderMessage);
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
                .RuleFor(u => u.Amount, f => f.Random.Int(min:10, max:5000))
                .RuleFor(u => u.ArticleNumber, f => f.Commerce.Product());

            return orderGenerator.Generate();
        }

        private static int DetermineOrderAmount()
        {
            var rawAmount = Console.ReadLine();
            if (int.TryParse(rawAmount, out int amount))
            {
                return amount;
            }

            Console.WriteLine("That's not a valid amount, let's try that again");
            return DetermineOrderAmount();
        }
    }
}
