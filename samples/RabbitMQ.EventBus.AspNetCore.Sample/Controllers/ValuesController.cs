using System;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.EventBus.Extension.AspNetCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AspNetCore.WebSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IRabbitMQEventBus _eventBus;

        public ValuesController(IRabbitMQEventBus eventBus)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        }

        // GET: api/<ValuesController>
        [HttpGet]
        public ActionResult<string> Get()
        {
            return "The path variable must be 0 or 1.";
        }

        // GET: api/<ValuesController>/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            var routingKey = $"rabbitmq.eventbus.test{(id > 0 ? id + "" : "")}";

            _eventBus.Publish(new
            {
                Body = $"{routingKey} => 发送消息",
                Time = DateTimeOffset.Now
            }, exchange: "RabbitMQ.EventBus.Simple", routingKey: routingKey);

            return "Ok";
        }
    }
}
