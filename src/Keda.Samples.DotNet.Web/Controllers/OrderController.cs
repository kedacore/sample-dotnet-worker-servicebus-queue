using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Keda.Samples.DotNet.Web.Hubs;

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

        [HttpGet]
        [Route("sale")]
        public async Task Sale()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "jakob", "sale order receieved");
        }

        [HttpGet]
        [Route("purchase")]
        public async Task Purchase()
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "jakob", "purchase order receieved");
        }
    }
}