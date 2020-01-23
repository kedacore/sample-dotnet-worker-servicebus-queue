using System.Threading.Tasks;
using Keda.Samples.Dotnet.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;

namespace Keda.Samples.DotNet.Web.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class QueueController : ControllerBase
    {
        protected IConfiguration Configuration { get; }

        public QueueController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<QueueStatus> Get()
        {
            var connectionString = Configuration.GetValue<string>("KEDA_SERVICEBUS_QUEUE_CONNECTIONSTRING");

            // Check current queue length
            var client = new ManagementClient(new ServiceBusConnectionStringBuilder(connectionString));
            var queueInfo = await client.GetQueueRuntimeInfoAsync("orders");

            return new QueueStatus
            {
                MessageCount = queueInfo.MessageCount
            };
        }
    }
}