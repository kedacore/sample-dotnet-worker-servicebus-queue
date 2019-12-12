using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Keda.Samples.DotNet.Web.Hubs;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Options;

namespace Keda.Samples.DotNet.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IHubContext<ChatHub> _hubContext;

        public OrderController(IHubContext<ChatHub> hubContext, IOptions<OrderQueueSettings> queueSettings)
        {
            _hubContext = hubContext;
            _connectionString = queueSettings.Value.ConnectionString;

            Task.Run(() => this.UpdateQueueStatus()).Wait();
        }

        [HttpPost]
        public async Task Post([FromBody] Order order)
        {         
            await UpdateQueueStatus();
        }

        private async Task UpdateQueueStatus()
        {
            //Check current queue length
            var client = new ManagementClient(new ServiceBusConnectionStringBuilder(_connectionString));
            var queueInfo = await client.GetQueueRuntimeInfoAsync("orders");
            var messageCount = queueInfo.MessageCount;

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", messageCount);
        }

    }
}