using System;
using System.Collections.Generic;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Modules
{
    internal class EventHandlerModuleFactory : IEventHandlerModuleFactory
    {
        private readonly List<IModuleHandle> _modules;
        private readonly object _syncRoot = new object();
        private readonly IServiceProvider _serviceProvider;

        public EventHandlerModuleFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _modules = new List<IModuleHandle>();
        }
        /// <summary>
        /// 发布事件
        /// </summary>
        public void PublishEvent(EventBusArgs e)
        {
            lock (_syncRoot)
            {
                foreach (var model in _modules)
                {
                    model.PublishEvent(e);
                }
            }
        }
        /// <summary>
        /// 订阅事件
        /// </summary>
        public void SubscribeEvent(EventBusArgs e)
        {
            lock (_syncRoot)
            {
                foreach (var model in _modules)
                {
                    model.SubscribeEvent(e);
                }
            }
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        public void TryAddModule(IModuleHandle module)
        {
            lock (_syncRoot)
            {
                _modules.Add(module);
            }
        }
    }
}
