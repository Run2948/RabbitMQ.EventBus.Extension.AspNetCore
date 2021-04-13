using System;
using RabbitMQ.Client;
using RabbitMQ.EventBus.Extension.AspNetCore.Configurations;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Factories
{
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        /// <summary>
        /// RabbitMQ 连接配置
        /// </summary>
        RabbitMQEventBusConnectionConfiguration Configuration { get; }
        /// <summary>
        /// 连接点
        /// </summary>
        string Endpoint { get; }
        /// <summary>
        /// 连接是否打开
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 尝试连接
        /// </summary>
        /// <returns></returns>
        bool TryConnect();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IModel CreateModel();
    }
}