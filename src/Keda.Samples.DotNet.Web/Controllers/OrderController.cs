using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Keda.Samples.DotNet.Web.Hubs;
using Keda.Samples.Dotnet.Contracts;

namespace Keda.Samples.DotNet.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public OrderController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost]
        public async Task Post([FromBody] Order order)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage",  order.Amount);
        }

    }
}