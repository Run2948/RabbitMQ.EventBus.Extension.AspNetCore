using System;
using RabbitMQ.EventBus.Extension.AspNetCore.Attributes;
using RabbitMQ.EventBus.Extension.AspNetCore.Events;

namespace AspNetCore.WebSample.Models
{
    [EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test")]
    public class MessageBody : IEvent
    {
        public string Body { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    [EventBus(Exchange = "RabbitMQ.EventBus.Simple", RoutingKey = "rabbitmq.eventbus.test1")]
    public class MessageBody1 : IEvent
    {
        public string Body { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
