﻿using System;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.Extension.AspNetCore.Extensions;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Events
{
    public class EventHandlerArgs<TEvent>
    {
        private readonly ILogger<TEvent> _logger;
        /// <summary>
        /// 原始消息
        /// </summary>
        public string Original { get; }
        /// <summary>
        /// 是否为打回的消息
        /// </summary>
        public bool Redelivered { get; }
        /// <summary>
        /// 交换机
        /// </summary>
        public string Exchange { get; }
        /// <summary>
        /// 路由key
        /// </summary>
        public string RoutingKey { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <param name="redelivered"></param>
        /// <param name="exchange"></param>
        /// <param name="routingKey"></param>
        /// <param name="logger"></param>
        public EventHandlerArgs(string original, bool redelivered, string exchange, string routingKey, ILogger<TEvent> logger = null)
        {
            Original = original;
            Redelivered = redelivered;
            Exchange = exchange;
            RoutingKey = routingKey;
            _logger = logger;
        }
        private TEvent _event;
        /// <summary>
        /// 序列化后的对象
        /// </summary>
        public TEvent Event
        {
            get
            {
                if (_event == null)
                {
                    try
                    {
                        _event = Original.Deserialize<TEvent>();
                    }
                    catch
                    {
                        try
                        {
                            _event = (TEvent)TypeDescriptor.GetConverter(typeof(TEvent)).ConvertFromInvariantString(Original);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(new EventId(ex.HResult), ex, $"content {Original} deserialize type {typeof(TEvent).Name} error {ex.Message}");
                            throw;
                        }
                    }
                }
                return _event;
            }
        }
    }
}
