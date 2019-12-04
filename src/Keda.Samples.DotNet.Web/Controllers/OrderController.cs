using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Keda.Samples.DotNet.Web.Hubs;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.Management.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;

namespace Keda.Samples.DotNet.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private const string ConnectionString = "Endpoint=sb://kedasb.servicebus.windows.net/;SharedAccessKeyName=order-consumer;SharedAccessKey=rBYw57bJjPT4BqffX9IlBNE78BF3UEz54M2cWDlN720=;EntityPath=orders";

        private readonly IHubContext<ChatHub> _hubContext;

        public OrderController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task Post([FromBody] Order order)
        {
           
            //Check current queue length
            var client = new ManagementClient(new ServiceBusConnectionStringBuilder(ConnectionString));
            var queueInfo = await client.GetQueueRuntimeInfoAsync("orders");
            var messageCount = queueInfo.MessageCount;

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", messageCount);
        }

    }
}