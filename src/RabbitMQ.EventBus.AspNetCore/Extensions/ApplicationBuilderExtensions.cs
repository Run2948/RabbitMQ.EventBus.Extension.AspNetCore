using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using RabbitMQ.EventBus.Extension.AspNetCore;
using RabbitMQ.EventBus.Extension.AspNetCore.Events;
using RabbitMQ.EventBus.Extension.AspNetCore.Extensions;
using RabbitMQ.EventBus.Extension.AspNetCore.Modules;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 自动订阅
        /// </summary>
        /// <param name="app"></param>
        public static void UseRabbitMQEventBus(this IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IRabbitMQEventBus>();
            var logger = app.ApplicationServices.GetRequiredService<ILogger<IRabbitMQEventBus>>();
            using (logger.BeginScope("EventBus Subscribe"))
            {
                logger.LogInformation($"=======================================================================");
                foreach (var mType in typeof(IEvent).GetAssemblies())
                {
                    var handlesAny = typeof(IEventHandler<>).GetMakeGenericType(mType).ToList();
                    if (handlesAny.Any())
                    {
                        logger.LogInformation($"{mType.Name}\t=>\t{string.Join("、", handlesAny)}");
                        eventBus.Subscribe(mType);
                    }
                }
                logger.LogInformation($"=======================================================================");
            }
        }
        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="app"></param>
        /// <param name="moduleOptions"></param>
        public static void AddRabbitMQEventBusModule(this IApplicationBuilder app, Action<RabbitMQEventBusModuleOption> moduleOptions)
        {
            var factory = app.ApplicationServices.GetRequiredService<IEventHandlerModuleFactory>();
            var moduleOption = new RabbitMQEventBusModuleOption(factory, app.ApplicationServices);
            moduleOptions?.Invoke(moduleOption);
        }
    }
}
