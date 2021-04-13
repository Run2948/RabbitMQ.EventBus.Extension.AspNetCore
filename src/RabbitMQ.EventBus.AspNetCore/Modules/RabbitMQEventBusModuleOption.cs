using System;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Modules
{
    /// <summary>
    /// 模块
    /// </summary>
    public sealed class RabbitMQEventBusModuleOption
    {
        private readonly IEventHandlerModuleFactory _handlerFactory;
        
        public IServiceProvider ApplicationServices;
        
        public RabbitMQEventBusModuleOption(IEventHandlerModuleFactory handlerFactory, IServiceProvider applicationServices)
        {
            this._handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
            ApplicationServices = applicationServices;
        }
        /// <summary>
        /// 添加模块
        /// </summary>
        /// <param name="module"></param>
        public void AddModule(IModuleHandle module)
        {
            _handlerFactory.TryAddModule(module);
        }
    }
}
