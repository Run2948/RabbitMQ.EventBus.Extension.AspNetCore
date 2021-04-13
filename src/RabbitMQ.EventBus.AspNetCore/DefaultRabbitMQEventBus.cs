﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.EventBus.Extension.AspNetCore.Attributes;
using RabbitMQ.EventBus.Extension.AspNetCore.Configurations;
using RabbitMQ.EventBus.Extension.AspNetCore.Events;
using RabbitMQ.EventBus.Extension.AspNetCore.Extensions;
using RabbitMQ.EventBus.Extension.AspNetCore.Factories;
using RabbitMQ.EventBus.Extension.AspNetCore.Modules;

namespace RabbitMQ.EventBus.Extension.AspNetCore
{
    /// <summary>
    /// 默认的 RabbitMQ EventBus 实现
    /// </summary>
    internal class DefaultRabbitMQEventBus : IRabbitMQEventBus
    {
        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<DefaultRabbitMQEventBus> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventHandlerModuleFactory _eventHandlerFactory;

        public DefaultRabbitMQEventBus(IRabbitMQPersistentConnection persistentConnection, IServiceProvider serviceProvider, IEventHandlerModuleFactory eventHandlerFactory, ILogger<DefaultRabbitMQEventBus> logger)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _eventHandlerFactory = eventHandlerFactory ?? throw new ArgumentNullException(nameof(eventHandlerFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private IModel _publishChannel;

        public void Publish<TMessage>(TMessage message, string exchange, string routingKey, string type = ExchangeType.Topic)
        {
            var body = message.Serialize();
            if (_publishChannel?.IsOpen != true)
            {
                if (_persistentConnection.IsConnected)
                {
                    _persistentConnection.TryConnect();
                }
                _publishChannel = _persistentConnection.ExchangeDeclare(exchange, type: type);
                _publishChannel.BasicReturn += async (se, ex) => await Task.Delay((int)_persistentConnection.Configuration.ConsumerFailRetryInterval.TotalMilliseconds).ContinueWith(t => Publish(body, ex.Exchange, ex.RoutingKey));
            }
            var properties = _publishChannel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            _publishChannel.BasicPublish(exchange: exchange,
                             routingKey: routingKey,
                             mandatory: true,
                             basicProperties: properties,
                             body: body.GetBytes());
            _logger.WriteLog(_persistentConnection.Configuration.Level, $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{exchange}\t{routingKey}\t{body}");
            _eventHandlerFactory?.PublishEvent(new EventBusArgs(_persistentConnection.Endpoint, exchange, "", routingKey, type, _persistentConnection.Configuration.ClientProvidedName, body, true));
        }

        public void Subscribe<TEvent>(string type = ExchangeType.Topic) where TEvent : class, IEvent
        {
            Subscribe(typeof(TEvent), type);
        }

        public void Subscribe(Type eventType, string type = ExchangeType.Topic)
        {
            var attributes = eventType.GetCustomAttributes(typeof(EventBusAttribute), true);
            var millisecondsDelay = (int?)_persistentConnection?.Configuration?.ConsumerFailRetryInterval.TotalMilliseconds ?? 1000;
            foreach (var attribute in attributes)
            {
                if (attribute is EventBusAttribute attr)
                {
                    string queue = attr.Queue ?? (_persistentConnection.Configuration.Prefix == QueuePrefixType.ExchangeName
                        ? $"{ attr.Exchange }.{ eventType.Name }"
                        : $"{_persistentConnection.Configuration.ClientProvidedName}.{ eventType.Name }");
                    if (!_persistentConnection.IsConnected)
                    {
                        _persistentConnection.TryConnect();
                    }
                    IModel channel;
                    #region snippet
                    var arguments = new Dictionary<string, object>();

                    #region 死信队列设置
                    if (_persistentConnection.Configuration.DeadLetterExchange.Enabled)
                    {
                        string deadExchangeName = $"{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNamePrefix}{_persistentConnection.Configuration.DeadLetterExchange.CustomizeExchangeName ?? attr.Exchange}{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNameSuffix}";
                        string deadQueueName = $"{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNamePrefix}{queue}{_persistentConnection.Configuration.DeadLetterExchange.ExchangeNameSuffix}";
                        IModel dlxChannel;
                        try
                        {
                            dlxChannel = _persistentConnection.ExchangeDeclare(exchange: deadExchangeName, type: type);
                            dlxChannel.QueueDeclarePassive(deadQueueName);
                        }
                        catch
                        {
                            dlxChannel = _persistentConnection.ExchangeDeclare(exchange: deadExchangeName, type: type);
                            dlxChannel.QueueDeclare(queue: deadQueueName,
                                                durable: true,
                                                exclusive: false,
                                                autoDelete: false,
                                                arguments: null);
                        }
                        dlxChannel.QueueBind(deadQueueName, deadExchangeName, attr.RoutingKey, null);
                        arguments.Add("x-dead-letter-exchange", deadExchangeName);
                    }
                    #endregion

                    try
                    {
                        channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                        channel.QueueDeclarePassive(queue);
                    }
                    catch
                    {
                        channel = _persistentConnection.ExchangeDeclare(exchange: attr.Exchange, type: type);
                        if (_persistentConnection.Configuration.MessageTTL != null && _persistentConnection.Configuration.MessageTTL > 0)
                        {
                            arguments.Add("x-message-ttl", _persistentConnection.Configuration.MessageTTL);
                        }
                        channel.QueueDeclare(queue: queue,
                                             durable: true,
                                             exclusive: false,
                                             autoDelete: false,
                                             arguments: arguments);
                    }
                    #endregion
                    channel.QueueBind(queue, attr.Exchange, attr.RoutingKey, null);
                    channel.BasicQos(0, _persistentConnection.Configuration.PrefetchCount, false);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray().GetString();
                        var isAck = false;
                        try
                        {
                            await ProcessEvent(body, eventType, ea);
                            //不确定是否需要改变Multiple是否需要改为true
                            channel.BasicAck(ea.DeliveryTag, multiple: false);
                            isAck = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(new EventId(ex.HResult), ex, ex.Message);
                        }
                        finally
                        {
                            _eventHandlerFactory?.SubscribeEvent(new EventBusArgs(_persistentConnection.Endpoint, ea.Exchange, queue, attr.RoutingKey, type, _persistentConnection.Configuration.ClientProvidedName, body, isAck));
                            _logger.WriteLog(_persistentConnection.Configuration.Level, $"{DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss}\t{isAck}\t{ea.Exchange}\t{ea.RoutingKey}\t{body}");
                            if (!isAck)
                            {
                                await Task.Delay(millisecondsDelay);
                                channel.BasicNack(ea.DeliveryTag, false, true);
                            }
                        }
                    };
                    channel.CallbackException += (sender, ex) =>
                    {
                        _logger.LogError(new EventId(ex.Exception.HResult), ex.Exception, ex.Exception.Message);
                    };
                    channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="eventType"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private async Task ProcessEvent(string body, Type eventType, BasicDeliverEventArgs args)
        {
            using var scope = _serviceProvider.CreateScope();
            foreach (var eventHandleType in typeof(IEventHandler<>).GetMakeGenericType(eventType))
            {
                var eventHandler = scope.ServiceProvider.GetRequiredService(eventHandleType);
                var logger = scope.ServiceProvider.GetRequiredService(typeof(ILogger<>).MakeGenericType(eventType));
                if (eventHandler == null)
                {
                    throw new InvalidOperationException(eventHandleType.Name);
                }
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                concreteType.GetMethod(nameof(IEventHandler<IEvent>.Handle))?.Invoke(eventHandler, new[] { Activator.CreateInstance(typeof(EventHandlerArgs<>).MakeGenericType(eventType), body, args.Redelivered, args.Exchange, args.RoutingKey, logger) });
                await Task.CompletedTask;
            }
        }
    }
}
