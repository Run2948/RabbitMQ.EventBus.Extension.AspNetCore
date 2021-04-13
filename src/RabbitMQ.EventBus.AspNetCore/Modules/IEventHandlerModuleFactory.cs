namespace RabbitMQ.EventBus.Extension.AspNetCore.Modules
{
    public interface IEventHandlerModuleFactory
    {
        void TryAddModule(IModuleHandle module);
       
        void PublishEvent(EventBusArgs e);
        
        void SubscribeEvent(EventBusArgs e);
    }
}
