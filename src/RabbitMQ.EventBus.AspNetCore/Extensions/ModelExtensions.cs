using System.Collections.Generic;
using RabbitMQ.Client;
using RabbitMQ.EventBus.Extension.AspNetCore.Factories;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Extensions
{
    internal static class ModelExtensions
    {
        /// <summary>
        /// Do a passive exchange declaration.
        /// Or
        /// (Spec method) Declare an exchange.
        ///  This method performs a "passive declare" on an exchange, which verifies whether. It will do nothing if the exchange already exists and result in a channel-levelprotocol exception (channel closure) if not.
        /// </summary>
        /// <param name="persistentConnection"></param>
        /// <param name="exchange"></param>
        /// <param name="type"></param>
        /// <param name="durable"></param>
        /// <param name="autoDelete"></param>
        /// <param name="arguments"></param>
        public static IModel ExchangeDeclare(this IRabbitMQPersistentConnection persistentConnection, string exchange, string type = ExchangeType.Topic, bool durable = true, bool autoDelete = false, IDictionary<string, object> arguments = null)
        {
            IModel channel;
            try
            {
                channel = persistentConnection.CreateModel();
                channel.ExchangeDeclarePassive(exchange);
            }
            catch
            {
                channel = persistentConnection.CreateModel();
                channel.ExchangeDeclare(exchange, type, durable, autoDelete, arguments);
            }
            return channel;
        }
    }
}
