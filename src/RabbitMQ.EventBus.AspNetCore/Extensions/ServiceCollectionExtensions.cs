using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.Extension.AspNetCore;
using RabbitMQ.EventBus.Extension.AspNetCore.Configurations;
using RabbitMQ.EventBus.Extension.AspNetCore.Events;
using RabbitMQ.EventBus.Extension.AspNetCore.Extensions;
using RabbitMQ.EventBus.Extension.AspNetCore.Factories;
using RabbitMQ.EventBus.Extension.AspNetCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加RabbitMQEventBus
        /// </summary>
        /// <param name="services"></param>
        /// <param name="connectionAction">使用匿名函数取得连接字符串,用来兼容使用Consul获取服务地址的情况</param>
        /// <param name="eventBusOptionAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, Func<string> connectionAction, Action<RabbitMQEventBusConnectionConfigurationBuilder> eventBusOptionAction)
        {
            var configuration = new RabbitMQEventBusConnectionConfiguration();
            var configurationBuilder = new RabbitMQEventBusConnectionConfigurationBuilder(configuration);
            eventBusOptionAction?.Invoke(configurationBuilder);
            services.TryAddSingleton<IRabbitMQPersistentConnection>(options =>
            {
                var logger = options.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                var connection = new DefaultRabbitMQPersistentConnection(configuration, connectionAction, logger);
                connection.TryConnect();
                return connection;
            });
            services.TryAddSingleton<IEventHandlerModuleFactory, EventHandlerModuleFactory>();
            services.TryAddSingleton<IRabbitMQEventBus, DefaultRabbitMQEventBus>();
            foreach (var mType in typeof(IEvent).GetAssemblies())
            {
                services.TryAddTransient(mType);
                foreach (var hType in typeof(IEventHandler<>).GetMakeGenericType(mType))
                {
                    services.TryAddTransient(hType);
                }
            }
            return services;
        }
    }
}
