using System.Threading.Tasks;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Options;

namespace Keda.Samples.DotNet.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly string _connectionString;

        public QueueController(IOptions<OrderQueueSettings> queueSettings)
        {
            _connectionString = queueSettings.Value.ConnectionString;
        }

        [HttpGet]
        public async Task<QueueStatus> Get()
        {
            //Check current queue length
            var client = new ManagementClient(new ServiceBusConnectionStringBuilder(_connectionString));
            var queueInfo = await client.GetQueueRuntimeInfoAsync("orders");

            return new QueueStatus()
            {
                MessageCount = queueInfo.MessageCount
            };
        }

    }


}