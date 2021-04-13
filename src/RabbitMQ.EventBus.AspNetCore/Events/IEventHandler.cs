using System.Threading.Tasks;

namespace RabbitMQ.EventBus.Extension.AspNetCore.Events
{
    /// <summary>
    /// EventBus消息处理
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// 消息处理方法
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        Task Handle(EventHandlerArgs<TEvent> args);
    }
}
